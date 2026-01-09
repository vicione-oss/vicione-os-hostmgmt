using System.Globalization;
using System.IO.Abstractions;
using System.Text;
using HostManagement.Configuration;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Validation;
using HostManagement.SystemConfig.InternalValidation;

namespace HostManagement.SystemConfig;

/// <summary>
/// Represents the system configuration service used to load or apply a system configuration.
/// </summary>
/// <param name="fileSystem">A <see cref="IFileSystem"/>.</param>
internal sealed class SystemConfigurationService(IFileSystem fileSystem)
{
    private readonly SystemConfigurationPersistence _systemConfigurationPersistence = new(fileSystem);

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
    public Task<GetSystemConfigurationResult> GetConfiguration(CancellationToken cancellationToken = default)
        => GetConfigurationFromScripts(cancellationToken);

    /// <summary>
    /// Gets the system configuration from 'SystemConfiguration.json' or
    /// 'DefaultSystemConfiguration.json' if not present or returns no configuration (null).
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <remarks>
    /// <see cref="GetSystemConfigurationResult.Configuration"/> contains:
    /// <br/>- <see cref="SystemConfiguration"/> if <see cref="OperationStatus.Success"/> or
    /// <see cref="OperationStatus.Warning"/>
    /// <br/>- <see langword="null"/> if <see cref="OperationStatus.Error"/>
    /// </remarks>
    public async Task<GetSystemConfigurationResult> GetConfigurationFromJson(
        CancellationToken cancellationToken = default)
    {
        var type = JsonConfigurationType.None;
        if (File.Exists(AppPaths.SystemConfigurationFilePath))
        {
            type = JsonConfigurationType.System;
        }
        else if (File.Exists(AppPaths.DefaultSystemConfigurationFilePath))
        {
            type = JsonConfigurationType.Default;
        }
        if (type == JsonConfigurationType.None)
        {
            var noConfigMessage = string.Format(
                CultureInfo.InvariantCulture,
                CompositeFormat.Parse(SystemConfigResources.NoConfigFound),
                $"{AppPaths.SystemConfigurationFilePath} / {AppPaths.DefaultSystemConfigurationFilePath}");

            return GetSystemConfigurationResult.CreateErrorResult(noConfigMessage);
        }
        var (status, message, configuration) = await GetConfigurationFromJsonInternal(type, cancellationToken)
            .ConfigureAwait(false);
        return new()
        {
            Status = status,
            Message = message,
            Configuration = configuration?.ToSharedContracts(),
        };
    }

    /// <summary>
    /// Prepares the system by writing changes of the 'SystemConfiguration.json' or
    /// 'DefaultSystemConfiguration.json' to the system without applying them yet.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="SetSystemConfigurationResult"/></returns>
    public async Task<SetSystemConfigurationResult> PrepareConfiguration(
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(AppPaths.SystemConfigurationFilePath) &&
            !File.Exists(AppPaths.DefaultSystemConfigurationFilePath))
        {
            var format = CompositeFormat.Parse(SystemConfigResources.NoConfigFound);

            return SetSystemConfigurationResult.CreateSuccessResult(
                string.Format(CultureInfo.InvariantCulture, format, AppPaths.SystemConfigurationFile) +
                Environment.NewLine +
                string.Format(CultureInfo.InvariantCulture, format, AppPaths.DefaultSystemConfigurationFile) +
                Environment.NewLine +
                SystemConfigResources.PrepareConfigSkipped);
        }

        // Prepare from SystemConfiguration.json
        var systemConfigResult = await
            PrepareConfigurationInternal(JsonConfigurationType.System, cancellationToken)
            .ConfigureAwait(false);

        if (systemConfigResult.Status == OperationStatus.Success)
            return systemConfigResult;

        // Prepare from DefaultSystemConfiguration.json
        var defaultConfigResult = await
            PrepareConfigurationInternal(JsonConfigurationType.Default, cancellationToken)
            .ConfigureAwait(false);

        var combinedMessage = string.Join(
            Environment.NewLine,
            systemConfigResult.Message,
            defaultConfigResult.Message);

        if (defaultConfigResult.Status == OperationStatus.Success)
            return SetSystemConfigurationResult.CreateSuccessResult(combinedMessage);

        return SetSystemConfigurationResult.CreateErrorResult(
            combinedMessage +
            Environment.NewLine +
            SystemConfigResources.PrepareConfigNotPossible);
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
            : string.Empty;

        return
            await SetConfigurationInternal(
                warningMessage,
                configResult.Configuration,
                proposedConfig,
                shouldPersistConfiguration,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<SetSystemConfigurationResult> PrepareConfigurationInternal(
        JsonConfigurationType jsonConfigType,
        CancellationToken cancellationToken = default)
    {
        var configFilePath = jsonConfigType switch
        {
            JsonConfigurationType.Default => AppPaths.DefaultSystemConfigurationFilePath,
            JsonConfigurationType.System => AppPaths.SystemConfigurationFilePath,
            _ => throw new ArgumentOutOfRangeException(nameof(jsonConfigType), jsonConfigType, null)
        };

        if (!File.Exists(configFilePath))
        {
            var noConfigMessage = string.Format(
                CultureInfo.InvariantCulture,
                CompositeFormat.Parse(SystemConfigResources.NoConfigFound),
                configFilePath);

            return SetSystemConfigurationResult.CreateWarningResult(noConfigMessage);
        }

        var prepareConfigFailedBaseMessage = string.Format(
            CultureInfo.InvariantCulture,
            CompositeFormat.Parse(SystemConfigResources.PrepareConfigFailed),
            configFilePath);

        var (status, message, configFromJson) = await
            GetConfigurationFromJsonInternal(jsonConfigType, cancellationToken)
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
                    configFilePath) +
                Environment.NewLine +
                prepareConfigFailedBaseMessage;

            return SetSystemConfigurationResult.CreateErrorResult(message);
        }

        var warningMessage = string.Empty;
        var configurationResult = new Result();

        try
        {
            // Bypass check for changes / Bypass apply of config
            // -> All adapters are executed (prepare phase only!)
            // -> Use 'configFromJson' also for the current config because we can't use getter
            // scripts at boot time (required information may not be available yet)
            var adapter = new SystemConfigurationAdapter(
                configFromJson,
                new ScriptHandler(configurationResult),
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

        if (!string.IsNullOrWhiteSpace(warningMessage))
        {
            message =
                configurationResult.Messages +
                Environment.NewLine +
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(SystemConfigResources.PrepareConfigCompletedWithWarnings),
                    configFilePath);

            return SetSystemConfigurationResult.CreateWarningResult(message);
        }

        var successMessage = string.Format(
            CultureInfo.InvariantCulture,
            CompositeFormat.Parse(SystemConfigResources.PrepareConfigSucceeded),
            configFilePath);

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

        try
        {
            // Don't bypass check for changes / Don't bypass apply of config
            // -> All relevant adapters are executed (prepare and apply phase)
            var adapter = new SystemConfigurationAdapter(
                currentConfig.ToInternalContracts(),
                new ScriptHandler(configurationResult),
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
                        configurationResult,
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
                    configurationResult,
                    ex.Message,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (!string.IsNullOrWhiteSpace(currentWarningMessage))
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

    private async Task<(OperationStatus Status, string Message, Contracts.SystemConfiguration? SystemConfiguration)>
        GetConfigurationFromJsonInternal(
            JsonConfigurationType jsonConfiguration,
            CancellationToken cancellationToken)
    {
        var configFilePath = jsonConfiguration switch
        {
            JsonConfigurationType.Default => AppPaths.DefaultSystemConfigurationFilePath,
            JsonConfigurationType.System => AppPaths.SystemConfigurationFilePath,
            _ => throw new ArgumentOutOfRangeException(nameof(jsonConfiguration), jsonConfiguration, null)
        };

        try
        {
            var loadedJsonConfig = await _systemConfigurationPersistence
                .LoadSystemConfiguration(configFilePath, cancellationToken)
                .ConfigureAwait(false);

            var message =
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(SystemConfigResources.GetConfigFromJsonSucceeded),
                    configFilePath);

            return (OperationStatus.Success, message, loadedJsonConfig);
        }
        catch (Exception ex)
        {
            var message =
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(SystemConfigResources.GetConfigFromJsonFailed),
                    configFilePath) +
                Environment.NewLine +
                ex.Message;

            return (OperationStatus.Error, message, default);
        }
    }

    private static async Task<GetSystemConfigurationResult> GetConfigurationFromScripts(
        CancellationToken cancellationToken)
    {
        try
        {
            var configurationResult = new Result();
            Contracts.SystemConfiguration currentConfig = new();

            // Don't bypass check for changes / Don't bypass apply of config
            // -> Setting is not relevant, since we only load the configuration
            var adapter = new SystemConfigurationAdapter(
                currentConfig,
                new ScriptHandler(configurationResult),
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

    private async Task<Result> PersistConfiguration(
        SystemConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var result = new Result();

        try
        {
            await _systemConfigurationPersistence
                .SaveSystemConfiguration(configuration.ToInternalContracts(), AppPaths.SystemConfigurationFilePath, cancellationToken)
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

    private static async Task<SetSystemConfigurationResult> RevertConfiguration(
        SystemConfiguration configToRevert,
        Result configurationResult,
        string message,
        CancellationToken cancellationToken = default)
    {
        var config = configToRevert.ToInternalContracts();

        // Bypass check for changes / Don't bypass apply of config
        // -> All adapters are executed (prepare and apply phase)
        var adapter = new SystemConfigurationAdapter(
            config,
            new ScriptHandler(configurationResult),
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
}
