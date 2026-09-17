using System.Globalization;
using System.Text;
using HostManagement.Configuration;
using HostManagement.Shared.Capabilities;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;
using HostManagement.Shared.Validation;
using HostManagement.SystemConfig.InternalValidation;
using Microsoft.Extensions.Logging;
using InternalContracts = HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig;

/// <summary>
/// Represents the system configuration service used to load or apply a system configuration.
/// </summary>
/// <param name="persistence">The persistence to load and save configurations.</param>
/// <param name="scriptHandler">A script handler to execute scripts.</param>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
/// <param name="supportedSettings">The supported settings.</param>
internal sealed partial class SystemConfigurationService(
    IPersistence persistence,
    IScriptHandler scriptHandler,
    ILogger logger,
    SupportedSettings supportedSettings)
{
    private readonly IPersistence _persistence = persistence;
    private readonly IScriptHandler _scriptHandler = scriptHandler;
    private readonly ILogger _logger = logger;
    private readonly SupportedSettings _supportedSettings = supportedSettings;

    /// <summary>
    /// Prepares the system by writing changes of the '<c>SystemConfiguration.json</c>' or
    /// '<c>DefaultSystemConfiguration.json</c>' to the system without applying them yet.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>
    /// <see cref="OperationStatus"/>
    /// <para>
    /// This method only returns <see cref="OperationStatus.Success"/> or <see cref="OperationStatus.Warning"/>,
    /// so service start does not fail.
    /// </para>
    /// </returns>
    public async Task<OperationStatus> PrepareConfigurationOnDeviceStartup(CancellationToken cancellationToken)
    {
        PrepareConfigurationStarted(_logger);

        var jsonConfigType = GetAvailableJsonConfigurationType();

        if (jsonConfigType == JsonConfigurationType.None)
        {
            PrepareConfigurationFromJsonNotPossible(_logger);
            return OperationStatus.Warning;
        }

        return await PrepareConfigurationFromJson(jsonConfigType, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the system configuration from '<c>SystemConfiguration.json</c>', or
    /// '<c>DefaultSystemConfiguration.json</c>' if not present, or returns no configuration (null).
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <remarks>
    /// This is usually only done, if the '<c>set-config.flag</c>' file was created during device start-up.
    /// See <see cref="ServiceRunner.PrepareConfiguration"/>.
    /// </remarks>
    /// <returns>
    /// <see cref="OperationStatus"/> and the <see cref="SystemConfiguration"/> or <see langword="null"/>
    /// <para>
    /// This method only returns <see cref="OperationStatus.Success"/> or <see cref="OperationStatus.Warning"/>,
    /// so service start does not fail.
    /// </para>
    /// </returns>
    public async Task<(OperationStatus Status, SystemConfiguration? Configuration)> GetConfigurationOnDeviceStartup(
        CancellationToken cancellationToken)
    {
        var jsonConfigType = GetAvailableJsonConfigurationType();

        if (jsonConfigType == JsonConfigurationType.None)
        {
            GetSystemConfigurationFromJsonNotPossible(_logger);
            return (OperationStatus.Warning, null);
        }

        var (status, configuration) = await GetConfigurationFromJson(jsonConfigType, cancellationToken)
            .ConfigureAwait(false);

        return (status, configuration?.ToSharedContracts());
    }

    /// <summary>
    /// Sets a previously prepared system configuration to the system.
    /// </summary>
    /// <param name="jsonConfig">The JSON config used during prepare.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    public async Task<OperationStatus> SetConfigurationOnDeviceStartup(
        SystemConfiguration jsonConfig,
        CancellationToken cancellationToken)
    {
        var setConfigResult = await SetConfiguration(jsonConfig, shouldPersistConfiguration: false, cancellationToken)
            .ConfigureAwait(false);

        return setConfigResult.Status;
    }

    /// <summary>
    /// Gets the system configuration from the system.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <remarks>
    /// <see cref="GetSystemConfigurationResult.Configuration"/> contains:
    /// <br/>- <see cref="SystemConfiguration"/> if <see cref="OperationStatus.Success"/> or
    /// <see cref="OperationStatus.Warning"/>
    /// <br/>- <see langword="null"/> if <see cref="OperationStatus.Error"/>
    /// </remarks>
    public async Task<GetSystemConfigurationResult> GetConfiguration(CancellationToken cancellationToken = default)
    {
        var cachedConfig = SystemConfigurationCache.Instance.GetCachedConfig();

        if (cachedConfig is not null)
        {
            return GetSystemConfigurationResult.CreateSuccessResult(
                SystemConfigResources.GetConfigFromCacheSucceeded,
                cachedConfig);
        }

        var getConfigResult = await GetConfigurationFromScripts(cancellationToken).ConfigureAwait(false);

        if (getConfigResult.Status != OperationStatus.Error &&
            getConfigResult.Configuration is not null)
        {
            SystemConfigurationCache.Instance.SetCachedConfig(
                getConfigResult.Configuration,
                cacheLog: message => SystemConfigCached(_logger, message),
                invalidateCacheLog: message => SystemConfigCacheInvalidated(_logger, message),
                invalidateCacheFailedLog: message => SystemConfigCacheInvalidationFailed(_logger, message));
        }

        return getConfigResult;
    }

    /// <summary>
    /// Sets a system configuration to the system.
    /// </summary>
    /// <param name="proposedConfig">The proposed system configuration.</param>
    /// <param name="shouldPersistConfiguration">If set to <see langword="false"/> skips persisting
    /// the <paramref name="proposedConfig"/>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="SetSystemConfigurationResult"/></returns>
    public async Task<SetSystemConfigurationResult> SetConfiguration(
        SystemConfiguration proposedConfig,
        bool shouldPersistConfiguration = true,
        CancellationToken cancellationToken = default)
    {
        var configResult = await GetConfiguration(cancellationToken).ConfigureAwait(false);

        if (configResult.Status == OperationStatus.Error ||
            configResult.Configuration is null)
        {
            var message =
                SystemConfigResources.SetConfigFailed +
                Environment.NewLine +
                configResult.Message;

            SetConfigurationFailedWithMessage(_logger, message);
            return SetSystemConfigurationResult.CreateErrorResult(message);
        }

        var validationMessage =
            await ValidateConfigurationChangeAsync(configResult.Configuration, proposedConfig, cancellationToken)
                .ConfigureAwait(false);

        if (validationMessage is not null)
        {
            validationMessage = SystemConfigResources.SetConfigFailed + Environment.NewLine + validationMessage;

            SetConfigurationFailedWithMessage(_logger, validationMessage);
            return SetSystemConfigurationResult.CreateErrorResult(validationMessage);
        }

        var warningMessage = configResult.Status == OperationStatus.Warning
            ? configResult.Message
            : default;

        return
            await SetConfigurationInternal(
                warningMessage,
                configResult.Configuration,
                proposedConfig,
                shouldPersistConfiguration,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<string?> ValidateConfigurationChangeAsync(
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig,
        CancellationToken cancellationToken)
    {
        var originalMACAddresses = new List<string>();

        foreach (var proposedInterface in proposedConfig.NetworkInterfaces)
        {
            var (scriptResult, originalAddress) = await _scriptHandler
                .ExecuteGetOriginalPhysicalAddressScript(proposedInterface.CommonInformation.Name);

            if (scriptResult.OperationStatus == OperationStatus.Success)
                originalMACAddresses.Add(originalAddress);
        }

        var validationResult = await new SystemConfigurationValidator()
            .ValidateAsync(proposedConfig, originalMACAddresses, cancellationToken)
            .ConfigureAwait(false);

        if (!validationResult.IsValid)
            return validationResult.EntriesString;

        var policyValidationResult = SystemConfigurationPolicyValidator.Validate(proposedConfig);

        if (!policyValidationResult.IsValid)
            return policyValidationResult.FormattedMessages;

        var supportedSettingsValidationResult = SupportedSettingsValidator
            .Validate(currentConfig, proposedConfig, _supportedSettings);

        if (!supportedSettingsValidationResult.IsValid)
            return supportedSettingsValidationResult.FormattedMessages;

        return null;
    }

    /// <summary>
    /// Resets the specified network interface to its default configuration.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="SystemControlResult"/></returns>
    public async Task<SystemControlResult> ResetNetworkInterface(
        string interfaceName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(interfaceName, nameof(interfaceName));

            var currentConfigResult = await GetConfiguration(cancellationToken).ConfigureAwait(false);

            if (currentConfigResult.Status == OperationStatus.Error)
            {
                return SystemControlResult.CreateErrorResult(
                    GetResetFailedMessage() +
                    Environment.NewLine +
                    currentConfigResult.Message);
            }

            if (currentConfigResult.Configuration is null)
            {
                return SystemControlResult.CreateErrorResult(
                    GetResetFailedMessage() +
                    Environment.NewLine +
                    SystemConfigResources.ConfigWasNull);
            }

            if (!_persistence.ExistsPreparationConfiguration())
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(SystemConfigResources.NoConfigFound),
                    $"'{AppPaths.DefaultSystemConfigurationFile}'");

                return SystemControlResult.CreateErrorResult(
                    GetResetFailedMessage() +
                    Environment.NewLine +
                    message);
            }

            var defaultConfig = await _persistence
                .LoadPreparationConfiguration(cancellationToken)
                .ConfigureAwait(false);

            var defaultInterface = defaultConfig
                .NetworkInterfaces?
                .FirstOrDefault(@interface => string.Equals(
                    @interface.CommonInformation?.Name,
                    interfaceName,
                    StringComparison.Ordinal));

            if (defaultInterface is null)
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(SystemConfigResources.ResetNetworkInterfaceNotFoundInConfig),
                    interfaceName,
                    AppPaths.DefaultSystemConfigurationFile);

                return SystemControlResult.CreateErrorResult(message);
            }

            var currentInterface = currentConfigResult.Configuration.GetNetworkInterface(interfaceName);

            if (currentInterface is null)
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(SystemConfigResources.ResetNetworkInterfaceNotFoundInCurrent),
                    interfaceName);

                return SystemControlResult.CreateErrorResult(message);
            }

            ApplyDefaultNetworkInterfaceSettings(currentInterface, defaultInterface);

            var setConfigResult = await SetConfiguration(
                currentConfigResult.Configuration,
                shouldPersistConfiguration: true,
                cancellationToken)
                .ConfigureAwait(false);

            if (setConfigResult.Status == OperationStatus.Error)
            {
                return SystemControlResult.CreateErrorResult(
                    GetResetFailedMessage() +
                    Environment.NewLine +
                    setConfigResult.Message);
            }

            var resultMessage = setConfigResult.Status == OperationStatus.Warning
                ? setConfigResult.Message
                : string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(SystemConfigResources.ResetNetworkInterfaceSucceeded),
                    interfaceName);

            return new SystemControlResult
            {
                Status = setConfigResult.Status,
                Message = resultMessage
            };
        }
        catch (Exception ex)
        {
            return SystemControlResult.CreateErrorResult(
                GetResetFailedMessage() +
                Environment.NewLine +
                ex.Message);
        }


        string GetResetFailedMessage()
            => string.Format(
                CultureInfo.InvariantCulture,
                CompositeFormat.Parse(SystemConfigResources.ResetNetworkInterfaceFailed),
                interfaceName);
    }

    private JsonConfigurationType GetAvailableJsonConfigurationType()
    {
        if (_persistence.ExistsUserConfiguration())
        {
            FoundExistingSystemConfiguration(_logger, _persistence.GetUserConfigurationPath());
            return JsonConfigurationType.System;
        }
        else
        {
            if (_persistence.ExistsDefectiveUserConfiguration())
                FoundOnlyDefectiveSystemConfiguration(_logger, _persistence.GetDefectiveUserConfigurationPath());
            else
                FoundNoSystemConfiguration(_logger, _persistence.GetUserConfigurationPath());
        }

        if (_persistence.ExistsPreparationConfiguration())
        {
            FoundExistingSystemConfiguration(_logger, _persistence.GetPreparationConfigurationPath());

            if (_persistence.ExistsDefectiveUserConfiguration())
            {
                // Only use 'DefaultSystemConfiguration.json' if
                // no 'SystemConfiguration.json' and 'SystemConfiguration.json.defective' exist.
                // Otherwise a previously applied config would be overwritten with defaults!
                return JsonConfigurationType.None;
            }

            return JsonConfigurationType.Default;
        }
        else
        {
            if (_persistence.ExistsDefectivePreparationConfiguration())
                FoundOnlyDefectiveSystemConfiguration(_logger, _persistence.GetDefectivePreparationConfigurationPath());
            else
                FoundNoSystemConfiguration(_logger, _persistence.GetPreparationConfigurationPath());
        }

        return JsonConfigurationType.None;
    }

    /// <remarks>
    /// This method only returns <see cref="OperationStatus.Success"/> or <see cref="OperationStatus.Warning"/>,
    /// so service start does not fail.
    /// </remarks>
    private async Task<OperationStatus> PrepareConfigurationFromJson(
        JsonConfigurationType jsonConfigType,
        CancellationToken cancellationToken)
    {
        var jsonConfigPath = GetJsonConfigurationPath(jsonConfigType);

        var (status, configFromJson) = await GetConfigurationFromJson(jsonConfigType, cancellationToken)
            .ConfigureAwait(false);

        if (status == OperationStatus.Warning ||
            configFromJson is null)
        {
            PrepareConfigurationFailed(_logger, jsonConfigPath);
            return OperationStatus.Warning;
        }

        try
        {
            // Bypass check for changes / Bypass apply of config
            // -> All adapters are executed (prepare phase only!)
            // -> Use 'configFromJson' also for the current config because we can't use getter
            // scripts at boot time (required information may not be available yet)
            var adapter = new SystemConfigurationAdapter(
                configFromJson,
                _scriptHandler,
                bypassHasChanges: true,
                bypassApplyConfig: true);

            var result = await adapter.SetConfiguration(configFromJson, cancellationToken).ConfigureAwait(false);

            if (result.OperationStatus is OperationStatus.Error)
            {
                PrepareConfigurationFailedWithMessage(_logger, jsonConfigPath, result.FormattedMessages);
                return OperationStatus.Warning;
            }

            if (result.OperationStatus is OperationStatus.Warning)
            {
                PrepareConfigurationCompletedWithMessage(_logger, jsonConfigPath, result.FormattedMessages);
                return OperationStatus.Warning;
            }
        }
        catch (Exception ex)
        {
            PrepareConfigurationFailedWithException(_logger, jsonConfigPath, ex);
            return OperationStatus.Warning;
        }

        PrepareConfigurationSucceeded(_logger, jsonConfigPath);
        return OperationStatus.Success;
    }

    /// <remarks>
    /// This method only returns <see cref="OperationStatus.Success"/> or <see cref="OperationStatus.Warning"/>,
    /// so service start does not fail.
    /// </remarks>
    private async Task<(OperationStatus Status, InternalContracts.SystemConfiguration? SystemConfiguration)>
        GetConfigurationFromJson(
            JsonConfigurationType jsonConfigType,
            CancellationToken cancellationToken)
    {
        var jsonConfigPath = string.Empty;

        try
        {
            jsonConfigPath = GetJsonConfigurationPath(jsonConfigType);

            var loadedJsonConfig = await LoadJsonConfiguration(jsonConfigType, cancellationToken)
                .ConfigureAwait(false);

            GetSystemConfigurationFromJsonSucceeded(_logger, jsonConfigPath);
            return (OperationStatus.Success, loadedJsonConfig);
        }
        catch (OperationCanceledException)
        {
            GetSystemConfigurationFromJsonCanceled(_logger, jsonConfigPath);
            return (OperationStatus.Warning, null);
        }
        catch (InvalidSystemConfigurationException ex)
        {
            GetSystemConfigurationFromJsonFailed(_logger, jsonConfigPath, ex);
            HandleDefectiveJsonConfiguration(jsonConfigType, jsonConfigPath);
            return (OperationStatus.Warning, null);
        }
        catch (Exception ex)
        {
            GetSystemConfigurationFromJsonFailed(_logger, jsonConfigPath, ex);
            return (OperationStatus.Warning, null);
        }
    }

    private async Task<SetSystemConfigurationResult> SetConfigurationInternal(
        string? currentWarningMessage,
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig,
        bool shouldPersistConfiguration = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Don't bypass check for changes / Don't bypass apply of config
            // -> All relevant adapters are executed (prepare and apply phase)
            var adapter = new SystemConfigurationAdapter(
                currentConfig.ToInternalContracts(),
                _scriptHandler,
                bypassHasChanges: false,
                bypassApplyConfig: false);

            var result = await adapter
                .SetConfiguration(proposedConfig.ToInternalContracts(), cancellationToken)
                .ConfigureAwait(false);

            if (result.OperationStatus is OperationStatus.Error)
            {
                return await RevertConfiguration(currentConfig, result.FormattedMessages, cancellationToken)
                    .ConfigureAwait(false);
            }

            if (result.OperationStatus is OperationStatus.Warning)
            {
                currentWarningMessage += string.IsNullOrWhiteSpace(currentWarningMessage)
                    ? result.FormattedMessages
                    : Environment.NewLine + result.FormattedMessages;
            }

            SystemConfigurationCache.Instance.Invalidate(
                invalidateCacheLog: message => SystemConfigCacheInvalidated(_logger, message),
                invalidateCacheFailedLog: message => SystemConfigCacheInvalidationFailed(_logger, message));

            if (shouldPersistConfiguration)
            {
                var persistResult = await PersistConfiguration(proposedConfig, cancellationToken).ConfigureAwait(false);

                if (persistResult.OperationStatus == OperationStatus.Error)
                {
                    var message =
                        SystemConfigResources.SetConfigFailed +
                        Environment.NewLine +
                        persistResult.FormattedMessages;

                    SetConfigurationFailedWithMessage(_logger, message);
                    return SetSystemConfigurationResult.CreateErrorResult(message);
                }
            }
        }
        catch (Exception ex)
        {
            return await RevertConfiguration(currentConfig, ex.Message, cancellationToken).ConfigureAwait(false);
        }

        if (currentWarningMessage is not null)
        {
            var message =
                SystemConfigResources.SetConfigCompletedWithWarnings +
                Environment.NewLine +
                currentWarningMessage;

            SetConfigurationCompletedWithMessage(_logger, message);
            return SetSystemConfigurationResult.CreateWarningResult(message);
        }

        SetConfigurationSucceeded(_logger, SystemConfigResources.SetConfigSucceeded);
        return SetSystemConfigurationResult.CreateSuccessResult(SystemConfigResources.SetConfigSucceeded);
    }

    private string GetJsonConfigurationPath(JsonConfigurationType jsonConfigType)
    {
        return jsonConfigType switch
        {
            JsonConfigurationType.Default => _persistence.GetPreparationConfigurationPath(),
            JsonConfigurationType.System => _persistence.GetUserConfigurationPath(),
            _ => throw new ArgumentOutOfRangeException(nameof(jsonConfigType), jsonConfigType, null)
        };
    }

    private Task<InternalContracts.SystemConfiguration> LoadJsonConfiguration(
        JsonConfigurationType jsonConfigType,
        CancellationToken cancellationToken)
    {
        return jsonConfigType switch
        {
            JsonConfigurationType.Default => _persistence.LoadPreparationConfiguration(cancellationToken),
            JsonConfigurationType.System => _persistence.LoadUserConfiguration(cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(jsonConfigType), jsonConfigType, null)
        };
    }

    private void HandleDefectiveJsonConfiguration(JsonConfigurationType jsonConfigType, string jsonConfigPath)
    {
        if (jsonConfigType == JsonConfigurationType.System)
        {
            _persistence.MarkUserConfigurationDefective();
            MarkedSystemConfigurationDefective(_logger, jsonConfigPath);
            return;
        }

        if (jsonConfigType == JsonConfigurationType.Default)
        {
            _persistence.MarkPreparationConfigurationDefective();
            MarkedSystemConfigurationDefective(_logger, jsonConfigPath);
            return;
        }
    }

    private async Task<GetSystemConfigurationResult> GetConfigurationFromScripts(
        CancellationToken cancellationToken)
    {
        try
        {
            var currentConfig = new InternalContracts.SystemConfiguration();

            // Don't bypass check for changes / Don't bypass apply of config
            // -> Setting is not relevant, since we only load the configuration
            var adapter = new SystemConfigurationAdapter(
                currentConfig,
                _scriptHandler,
                bypassHasChanges: false,
                bypassApplyConfig: false);

            var result = await adapter.GetConfiguration(currentConfig, cancellationToken).ConfigureAwait(false);

            if (result.OperationStatus is OperationStatus.Error)
            {
                var message =
                    SystemConfigResources.GetConfigFromScriptsFailed +
                    Environment.NewLine +
                    result.FormattedMessages;

                return GetSystemConfigurationResult.CreateErrorResult(message);
            }
            else if (result.OperationStatus is OperationStatus.Warning)
            {
                var message =
                    SystemConfigResources.GetConfigFromScriptsCompletedWithWarnings +
                    Environment.NewLine +
                    result.FormattedMessages;

                return GetSystemConfigurationResult.CreateWarningResult(message, currentConfig.ToSharedContracts());
            }

            return GetSystemConfigurationResult.CreateSuccessResult(
                SystemConfigResources.GetConfigFromScriptsSucceeded,
                currentConfig.ToSharedContracts());
        }
        catch (Exception ex)
        {
            var message =
                SystemConfigResources.GetConfigFromScriptsFailed +
                Environment.NewLine +
                ex.Message;

            return GetSystemConfigurationResult.CreateErrorResult(message);
        }
    }

    private static void ApplyDefaultNetworkInterfaceSettings(
        NetworkInterfaceDetail currentInterface,
        InternalContracts.NetworkInterfaceDetail defaultInterface)
    {
        var defaultCommon = defaultInterface.CommonInformation;

        if (defaultCommon is not null)
        {
            var currentCommon = currentInterface.CommonInformation;

            if (defaultCommon.Enabled.HasValue)
                currentCommon.Enabled = defaultCommon.Enabled.Value;

            if (defaultCommon.UserDefinedMACAddress is { } userDefinedMACAddress)
            {
                currentCommon.UserDefinedMACAddress.Enabled = defaultCommon.UserDefinedMACAddress.Enabled;
                currentCommon.UserDefinedMACAddress.Address = defaultCommon.UserDefinedMACAddress.Address.ToColonNotation();
            }
        }

        var defaultIpv4 = defaultInterface.IPv4;

        if (defaultIpv4 is not null)
        {
            if (defaultIpv4.DHCPEnabled.HasValue)
                currentInterface.IPv4.DHCPEnabled = defaultIpv4.DHCPEnabled.Value;

            if (defaultIpv4.Gateway is not null)
                currentInterface.IPv4.Gateway = defaultIpv4.Gateway;

            if (defaultIpv4.IPv4Details is not null)
            {
                currentInterface.IPv4.IPv4Details.Clear();
                currentInterface.IPv4.IPv4Details.AddRange(
                    defaultIpv4.IPv4Details.Select(static detail => new IPv4Detail
                    {
                        IPAddress = detail.IPAddress,
                        Netmask = detail.Netmask,
                    }));
            }
        }

        var defaultVlan = defaultInterface.VLAN;

        if (defaultVlan is not null)
        {
            if (defaultVlan.Enabled.HasValue)
                currentInterface.VLAN.Enabled = defaultVlan.Enabled.Value;

            if (defaultVlan.ID.HasValue)
                currentInterface.VLAN.ID = defaultVlan.ID.Value;
        }
    }

    private async Task<Result> PersistConfiguration(
        SystemConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var result = new Result();

        try
        {
            await _persistence
                .SaveUserConfiguration(configuration.ToInternalContracts(), cancellationToken)
                .ConfigureAwait(false);

            if (_persistence.ExistsDefectiveUserConfiguration())
                _persistence.DeleteDefectiveMarkedUserConfiguration();

            return result;
        }
        catch (Exception ex)
        {
            var message =
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(SystemConfigResources.CouldNotPersistConfig),
                    AppPaths.SystemConfigurationFile) +
                Environment.NewLine +
                ex.Message;

            result.AddError(message);

            return result;
        }
    }

    private async Task<SetSystemConfigurationResult> RevertConfiguration(
        SystemConfiguration configToRevert,
        string setConfigurationErrorMessage,
        CancellationToken cancellationToken)
    {
        var config = configToRevert.ToInternalContracts();

        // Bypass check for changes / Don't bypass apply of config
        // -> All adapters are executed (prepare and apply phase)
        var adapter = new SystemConfigurationAdapter(
            config,
            _scriptHandler,
            bypassHasChanges: true,
            bypassApplyConfig: false);

        var result = await adapter
            .SetConfiguration(config, cancellationToken)
            .ConfigureAwait(false);

        var baseErrorMessage = result.OperationStatus is OperationStatus.Error
            ? SystemConfigResources.RevertConfigFailedAfterSetFailed
            : SystemConfigResources.RevertConfigSucceededAfterSetFailed;

        if (!string.IsNullOrWhiteSpace(result.FormattedMessages))
            baseErrorMessage += Environment.NewLine + result.FormattedMessages;

        var errorMessage = baseErrorMessage + Environment.NewLine + setConfigurationErrorMessage;

        SetConfigurationFailedWithMessage(_logger, errorMessage);
        return SetSystemConfigurationResult.CreateErrorResult(errorMessage);
    }

    [LoggerMessage(LogLevel.Information, "Found existing system configuration '{Path}'.")]
    private static partial void FoundExistingSystemConfiguration(ILogger logger, string? path);

    [LoggerMessage(LogLevel.Warning, "Found no system configuration '{Path}'.")]
    private static partial void FoundNoSystemConfiguration(ILogger logger, string? path);

    [LoggerMessage(LogLevel.Warning, "Found only defective system configuration '{Path}'.")]
    private static partial void FoundOnlyDefectiveSystemConfiguration(ILogger logger, string? path);

    [LoggerMessage(LogLevel.Warning, "Get system configuration '{Path}' canceled.")]
    private static partial void GetSystemConfigurationFromJsonCanceled(ILogger logger, string? path);

    [LoggerMessage(LogLevel.Warning, "Get system configuration '{Path}' failed.")]
    private static partial void GetSystemConfigurationFromJsonFailed(ILogger logger, string? path, Exception? exception);

    [LoggerMessage(LogLevel.Warning, "Get system configuration from JSON not possible.")]
    private static partial void GetSystemConfigurationFromJsonNotPossible(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Get system configuration '{path}' succeeded.")]
    private static partial void GetSystemConfigurationFromJsonSucceeded(ILogger logger, string? path);

    [LoggerMessage(LogLevel.Warning, "Marked system configuration '{Path}' defective.")]
    private static partial void MarkedSystemConfigurationDefective(ILogger logger, string? path);

    [LoggerMessage(LogLevel.Warning, "Prepare system configuration '{Path}' completed with warnings. {Message}")]
    private static partial void PrepareConfigurationCompletedWithMessage(ILogger logger, string? path, string? message);

    [LoggerMessage(LogLevel.Warning, "Prepare system configuration '{Path}' failed.")]
    private static partial void PrepareConfigurationFailed(ILogger logger, string? path);

    [LoggerMessage(LogLevel.Warning, "Prepare system configuration '{Path}' failed.")]
    private static partial void PrepareConfigurationFailedWithException(ILogger logger, string? path, Exception? exception);

    [LoggerMessage(LogLevel.Warning, "Prepare system configuration '{Path}' failed. {Message}")]
    private static partial void PrepareConfigurationFailedWithMessage(ILogger logger, string? path, string? message);

    [LoggerMessage(LogLevel.Warning, "Prepare system configuration from JSON not possible.")]
    private static partial void PrepareConfigurationFromJsonNotPossible(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Preparing configuration at boot time started.")]
    private static partial void PrepareConfigurationStarted(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Prepare system configuration '{Path}' succeeded.")]
    private static partial void PrepareConfigurationSucceeded(ILogger logger, string? path);

    [LoggerMessage(LogLevel.Error, "{Message}")]
    private static partial void SetConfigurationFailedWithMessage(ILogger logger, string message);

    [LoggerMessage(LogLevel.Warning, "{Message}")]
    private static partial void SetConfigurationCompletedWithMessage(ILogger logger, string message);

    [LoggerMessage(LogLevel.Information, "{Message}")]
    private static partial void SetConfigurationSucceeded(ILogger logger, string message);

    [LoggerMessage(LogLevel.Information, "{Message}")]
    private static partial void SystemConfigCached(ILogger logger, string? message);

    [LoggerMessage(LogLevel.Information, "{Message}")]
    private static partial void SystemConfigCacheInvalidated(ILogger logger, string? message);

    [LoggerMessage(LogLevel.Error, "{Message}")]
    private static partial void SystemConfigCacheInvalidationFailed(ILogger logger, string? message);
}
