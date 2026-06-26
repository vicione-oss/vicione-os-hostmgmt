using System.Globalization;
using System.Text;
using HostManagement.Configuration;
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
internal sealed partial class SystemConfigurationService(
    IPersistence persistence,
    IScriptHandler scriptHandler,
    ILogger logger)
{
    private readonly IPersistence _persistence = persistence;
    private readonly IScriptHandler _scriptHandler = scriptHandler;
    private readonly ILogger _logger = logger;

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
    /// Gets the system configuration from '<c>SystemConfiguration.json</c>', or
    /// '<c>DefaultSystemConfiguration.json</c>' if not present, or returns no configuration (null).
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <remarks>
    /// This is usually only done, if the '<c>set-config.flag</c>' file was created during device start-up.
    /// See <see cref="ServiceRunner.PrepareConfiguration"/>.
    /// </remarks>
    /// <returns><see cref="GetSystemConfigurationResult"/></returns>
    public async Task<GetSystemConfigurationResult> GetConfigurationOnDeviceStartup(CancellationToken cancellationToken)
    {
        var jsonConfigType = GetAvailableJsonConfigurationType();

        if (jsonConfigType == JsonConfigurationType.None)
        {
            var noConfigMessage = string.Format(
                CultureInfo.InvariantCulture,
                CompositeFormat.Parse(SystemConfigResources.NoConfigFound),
                $"'{_persistence.GetUserConfigurationPath()}' or '{_persistence.GetPreparationConfigurationPath()}'");

            return GetSystemConfigurationResult.CreateErrorResult(noConfigMessage);
        }

        var (status, message, configuration) = await GetConfigurationFromJson(jsonConfigType, cancellationToken)
            .ConfigureAwait(false);

        return new()
        {
            Status = status,
            Message = message,
            Configuration = configuration?.ToSharedContracts(),
        };
    }

    /// <summary>
    /// Prepares the system by writing changes of the '<c>SystemConfiguration.json</c>' or
    /// '<c>DefaultSystemConfiguration.json</c>' to the system without applying them yet.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="SetSystemConfigurationResult"/></returns>
    public async Task<SetSystemConfigurationResult> PrepareConfigurationOnDeviceStartup(CancellationToken cancellationToken)
    {
        var jsonConfigType = GetAvailableJsonConfigurationType();

        if (jsonConfigType == JsonConfigurationType.None)
        {
            var noConfigMessage = string.Format(
                CultureInfo.InvariantCulture,
                CompositeFormat.Parse(SystemConfigResources.NoConfigFound),
                $"'{_persistence.GetUserConfigurationPath()}' or '{_persistence.GetPreparationConfigurationPath()}'");

            return SetSystemConfigurationResult.CreateWarningResult(noConfigMessage);
        }

        return await PrepareConfigurationFromJson(jsonConfigType, cancellationToken).ConfigureAwait(false);
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

        if (configResult.Status == OperationStatus.Error)
        {
            var message =
                SystemConfigResources.SetConfigFailed +
                Environment.NewLine +
                configResult.Message;

            return SetSystemConfigurationResult.CreateErrorResult(message);
        }

        if (configResult.Configuration is null)
        {
            var message =
                SystemConfigResources.SetConfigFailed +
                Environment.NewLine +
                SystemConfigResources.ConfigWasNull;

            return SetSystemConfigurationResult.CreateErrorResult(message);
        }

        var validationResult = await new SystemConfigurationValidator()
            .ValidateAsync(proposedConfig, cancellationToken)
            .ConfigureAwait(false);

        if (!validationResult.IsValid)
        {
            var message =
                SystemConfigResources.SetConfigFailed +
                Environment.NewLine +
                validationResult.EntriesString;

            return SetSystemConfigurationResult.CreateErrorResult(message);
        }

        var internalValidationResult = await SystemConfigurationValidatorInternal
            .Validate(proposedConfig)
            .ConfigureAwait(false);

        if (!internalValidationResult.IsValid)
        {
            var message =
                SystemConfigResources.SetConfigFailed +
                Environment.NewLine +
                internalValidationResult.EntriesString;

            return SetSystemConfigurationResult.CreateErrorResult(message);
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

            if (!_persistence.PreparationConfigurationExists())
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
                .NetworkInterfacesSettings?
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
        if (_persistence.UserConfigurationExists())
            return JsonConfigurationType.System;

        if (_persistence.PreparationConfigurationExists())
            return JsonConfigurationType.Default;

        return JsonConfigurationType.None;
    }

    private async Task<SetSystemConfigurationResult> PrepareConfigurationFromJson(
        JsonConfigurationType jsonConfigType,
        CancellationToken cancellationToken = default)
    {
        var jsonConfigPath = GetJsonConfigurationPath(jsonConfigType);

        var prepareConfigFailedBaseMessage = string.Format(
            CultureInfo.InvariantCulture,
            CompositeFormat.Parse(SystemConfigResources.PrepareConfigFailed),
            jsonConfigPath);

        var (status, message, configFromJson) = await
            GetConfigurationFromJson(jsonConfigType, cancellationToken)
            .ConfigureAwait(false);

        if (status == OperationStatus.Error)
        {
            return SetSystemConfigurationResult.CreateErrorResult(
                message +
                Environment.NewLine +
                prepareConfigFailedBaseMessage);
        }

        if (configFromJson is null)
        {
            message =
                SystemConfigResources.ConfigWasNull +
                Environment.NewLine +
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(SystemConfigResources.AffectedConfigWasJsonConfig),
                    jsonConfigPath) +
                Environment.NewLine +
                prepareConfigFailedBaseMessage;

            return SetSystemConfigurationResult.CreateErrorResult(message);
        }

        string? warningMessage = default;
        var configurationResult = new Result();
        _scriptHandler.UseResult(configurationResult);

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

            var operationStatus = await adapter
                .SetConfiguration(configFromJson, cancellationToken)
                .ConfigureAwait(false);

            if (operationStatus == OperationStatus.Error)
            {
                return SetSystemConfigurationResult.CreateErrorResult(
                    configurationResult.Messages +
                    Environment.NewLine +
                    prepareConfigFailedBaseMessage);
            }

            if (operationStatus == OperationStatus.Warning)
                warningMessage = configurationResult.Messages;
        }
        catch (Exception ex)
        {
            return SetSystemConfigurationResult.CreateErrorResult(
                ex.Message +
                Environment.NewLine +
                prepareConfigFailedBaseMessage);
        }

        if (warningMessage is not null)
        {
            message =
                configurationResult.Messages +
                Environment.NewLine +
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(SystemConfigResources.PrepareConfigCompletedWithWarnings),
                    jsonConfigPath);

            return SetSystemConfigurationResult.CreateWarningResult(message);
        }

        var successMessage = string.Format(
            CultureInfo.InvariantCulture,
            CompositeFormat.Parse(SystemConfigResources.PrepareConfigSucceeded),
            jsonConfigPath);

        return SetSystemConfigurationResult.CreateSuccessResult(successMessage);
    }

    private async Task<SetSystemConfigurationResult> SetConfigurationInternal(
        string? currentWarningMessage,
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig,
        bool shouldPersistConfiguration = true,
        CancellationToken cancellationToken = default)
    {
        var configurationResult = new Result();
        _scriptHandler.UseResult(configurationResult);

        try
        {
            // Don't bypass check for changes / Don't bypass apply of config
            // -> All relevant adapters are executed (prepare and apply phase)
            var adapter = new SystemConfigurationAdapter(
                currentConfig.ToInternalContracts(),
                _scriptHandler,
                bypassHasChanges: false,
                bypassApplyConfig: false);

            var operationStatus = await adapter
                .SetConfiguration(proposedConfig.ToInternalContracts(), cancellationToken)
                .ConfigureAwait(false);

            if (operationStatus == OperationStatus.Error)
            {
                return
                    await RevertConfiguration(
                        currentConfig,
                        configurationResult.Messages,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            if (operationStatus == OperationStatus.Warning)
            {
                currentWarningMessage += string.IsNullOrWhiteSpace(currentWarningMessage)
                    ? configurationResult.Messages
                    : Environment.NewLine + configurationResult.Messages;
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
                        persistResult.Messages;

                    return SetSystemConfigurationResult.CreateErrorResult(message);
                }
            }
        }
        catch (Exception ex)
        {
            return
                await RevertConfiguration(
                    currentConfig,
                    ex.Message,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (currentWarningMessage is not null)
        {
            var message =
                SystemConfigResources.SetConfigCompletedWithWarnings +
                Environment.NewLine +
                currentWarningMessage;

            return SetSystemConfigurationResult.CreateWarningResult(message);
        }

        return SetSystemConfigurationResult.CreateSuccessResult(
            SystemConfigResources.SetConfigSucceeded);
    }

    private async Task<(OperationStatus Status, string Message, InternalContracts.SystemConfiguration? SystemConfiguration)>
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

            var message =
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(SystemConfigResources.GetConfigFromJsonSucceeded),
                    jsonConfigPath);

            return (OperationStatus.Success, message, loadedJsonConfig);
        }
        catch (OperationCanceledException)
        {
            var message =
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(SystemConfigResources.GetConfigFromJsonCanceled),
                    jsonConfigPath);

            return (OperationStatus.Warning, message, null);
        }
        catch (Exception ex)
        {
            var message =
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(SystemConfigResources.GetConfigFromJsonFailed),
                    jsonConfigPath) +
                Environment.NewLine +
                ex.Message;

            return (OperationStatus.Error, message, null);
        }
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

    private async Task<GetSystemConfigurationResult> GetConfigurationFromScripts(
        CancellationToken cancellationToken)
    {
        try
        {
            var configurationResult = new Result();
            _scriptHandler.UseResult(configurationResult);
            Contracts.SystemConfiguration currentConfig = new();

            // Don't bypass check for changes / Don't bypass apply of config
            // -> Setting is not relevant, since we only load the configuration
            var adapter = new SystemConfigurationAdapter(
                currentConfig,
                _scriptHandler,
                bypassHasChanges: false,
                bypassApplyConfig: false);

            var operationStatus = await adapter
                .GetConfiguration(currentConfig, cancellationToken)
                .ConfigureAwait(false);

            if (operationStatus == OperationStatus.Error)
            {
                var message =
                    SystemConfigResources.GetConfigFromScriptsFailed +
                    Environment.NewLine +
                    configurationResult.Messages;

                return GetSystemConfigurationResult.CreateErrorResult(message);
            }
            else if (operationStatus == OperationStatus.Warning)
            {
                var message =
                    SystemConfigResources.GetConfigFromScriptsCompletedWithWarnings +
                    Environment.NewLine +
                    configurationResult.Messages;

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
            if (defaultCommon.Enabled.HasValue)
                currentInterface.CommonInformation.Enabled = defaultCommon.Enabled.Value;

            if (defaultCommon.PhysicalAddress is not null)
                currentInterface.CommonInformation.PhysicalAddress = defaultCommon.PhysicalAddress;
        }

        var defaultIpv4 = defaultInterface.IPv4;

        if (defaultIpv4 is null)
            return;

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

        if (defaultIpv4.VLANEnabled.HasValue)
            currentInterface.IPv4.VLANEnabled = defaultIpv4.VLANEnabled.Value;

        if (defaultIpv4.VLANID.HasValue)
            currentInterface.IPv4.VLANID = defaultIpv4.VLANID.Value;
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
        string message,
        CancellationToken cancellationToken = default)
    {
        var config = configToRevert.ToInternalContracts();

        // Bypass check for changes / Don't bypass apply of config
        // -> All adapters are executed (prepare and apply phase)
        var adapter = new SystemConfigurationAdapter(
            config,
            _scriptHandler,
            bypassHasChanges: true,
            bypassApplyConfig: false);

        var operationStatus = await adapter
            .SetConfiguration(config, cancellationToken)
            .ConfigureAwait(false);

        var baseErrorMessage = operationStatus == OperationStatus.Error
            ? SystemConfigResources.RevertConfigFailedAfterSetFailed
            : SystemConfigResources.RevertConfigSucceededAfterSetFailed;

        var errorMessage = baseErrorMessage + Environment.NewLine + message;

        return SetSystemConfigurationResult.CreateErrorResult(errorMessage);
    }

    [LoggerMessage(LogLevel.Information, "{Message}")]
    private static partial void SystemConfigCached(ILogger logger, string? message);

    [LoggerMessage(LogLevel.Information, "{Message}")]
    private static partial void SystemConfigCacheInvalidated(ILogger logger, string? message);

    [LoggerMessage(LogLevel.Error, "{Message}")]
    private static partial void SystemConfigCacheInvalidationFailed(ILogger logger, string? message);
}
