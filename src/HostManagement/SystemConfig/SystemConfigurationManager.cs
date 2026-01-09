using System.Text.Json;
using HostManagement.Configuration;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Validation;
using SourceGenerationContext = HostManagement.Shared.Contracts.SourceGenerationContext;

namespace HostManagement.SystemConfig;

/// <summary>
/// Represents the system configuration manager.
/// </summary>
internal sealed class SystemConfigurationManager
{
    /// <summary>
    /// Applies the current system configuration json if it exists, bypassing the internal check
    /// for changes. If no system configuration json exists, determines the current configuration
    /// from the system, creates a new system configuration json and applies it.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <remarks>
    /// <para>
    /// The <see cref="OperationResult.Message"/> is a non JSON string if
    /// <see cref="OperationResult.Status"/> is <see cref="OperationStatus.Warning"/>
    /// or <see cref="OperationStatus.Error"/> otherwise <see langword="null"/>.
    /// </para>
    /// </remarks>
    /// <returns><see cref="ApplySystemConfigurationResult"/></returns>
    public async Task<ApplySystemConfigurationResult> ApplyConfigurationOnStartup(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var loadedConfigResult = await LoadLastAppliedConfiguration(cancellationToken);

            if (loadedConfigResult.Status == OperationStatus.Error ||
                loadedConfigResult.Configuration is null)
            {
                return new ApplySystemConfigurationResult
                {
                    Status = OperationStatus.Error,
                    Message = $"Load configuration on startup failed.{Environment.NewLine}{loadedConfigResult.Message}"
                };
            }

            if (loadedConfigResult.Status == OperationStatus.Warning)
            {
                return new ApplySystemConfigurationResult
                {
                    Status = OperationStatus.Warning,
                    Message = $"Load configuration on startup completed with warnings.{Environment.NewLine}{loadedConfigResult.Message}"
                };
            }

            var applyConfigResult = await ApplyConfigurationInternal(loadedConfigResult.Configuration, true, cancellationToken);

            if (applyConfigResult.Status == OperationStatus.Error)
            {
                return new ApplySystemConfigurationResult
                {
                    Status = OperationStatus.Error,
                    Message = $"Apply configuration on startup failed.{Environment.NewLine}{applyConfigResult.Message}"
                };
            }

            if (applyConfigResult.Status == OperationStatus.Warning)
            {
                return new ApplySystemConfigurationResult
                {
                    Status = OperationStatus.Warning,
                    Message = $"Apply configuration on startup completed with warnings.{Environment.NewLine}{applyConfigResult.Message}"
                };
            }

            return new ApplySystemConfigurationResult
            {
                Status = OperationStatus.Success,
                Message = $"Apply configuration on startup succeeded."
            };
        }
        catch (Exception ex)
        {
            return new ApplySystemConfigurationResult
            {
                Status = OperationStatus.Error,
                Message = $"Apply configuration on startup failed.{Environment.NewLine}{ex.Message}"
            };
        }
    }

    /// <summary>
    /// Applies a system configuration to the system.
    /// </summary>
    /// <param name="proposedConfig">The proposed system configuration.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <remarks>
    /// <para>
    /// The <see cref="OperationResult.Message"/> is a non JSON string if
    /// <see cref="OperationResult.Status"/> is <see cref="OperationStatus.Warning"/>
    /// or <see cref="OperationStatus.Error"/> otherwise <see langword="null"/>.
    /// </para>
    /// </remarks>
    /// <returns><see cref="ApplySystemConfigurationResult"/></returns>
    public async Task<ApplySystemConfigurationResult> ApplyConfiguration(
        SystemConfiguration proposedConfig,
        CancellationToken cancellationToken = default)
    {
        return await ApplyConfigurationInternal(proposedConfig, cancellationToken: cancellationToken);
    }

    private async Task<ApplySystemConfigurationResult> ApplyConfigurationInternal(
        SystemConfiguration proposedConfig,
        bool bypassHasChangesInternal = false,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult;
        SystemConfiguration? loadedConfig;

        var hasWarnings = false;

        try
        {
            var loadedConfigResult = await LoadConfiguration(cancellationToken).ConfigureAwait(false);

            if (loadedConfigResult.Status == OperationStatus.Error)
            {
                return new ApplySystemConfigurationResult
                {
                    Status = OperationStatus.Error,
                    Message = $"Loading configuration failed.{Environment.NewLine}{loadedConfigResult.Message}"
                };
            }

            if (loadedConfigResult.Status == OperationStatus.Warning)
                hasWarnings = true;

            if (loadedConfigResult.Configuration is null)
                throw new InvalidOperationException("Loaded system configuration was null.");

            loadedConfig = loadedConfigResult.Configuration;
            validationResult = await SystemConfigurationValidator
                .Validate(loadedConfig, proposedConfig)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new ApplySystemConfigurationResult
            {
                Status = OperationStatus.Error,
                Message = $"Validation failed.{Environment.NewLine}{ex.Message}"
            };
        }

        var configurationResult = new Result();

        if (!validationResult.IsValid)
        {
            foreach (var entry in validationResult.Entries)
            {
                configurationResult.AddError($"{entry.Code}: {entry.Message}");
            }

            return new ApplySystemConfigurationResult
            {
                Status = OperationStatus.Error,
                Message = $"Validation failed.{Environment.NewLine}{configurationResult.Messages}"
            };
        }

        try
        {
            var adapter = new SystemConfigurationAdapter(
                loadedConfig,
                configurationResult,
                new ScriptHandler(configurationResult),
                bypassHasChangesInternal);

            var operationStatus = await adapter
                .ApplyConfiguration(proposedConfig, cancellationToken)
                .ConfigureAwait(false);

            if (operationStatus == OperationStatus.Error)
            {
                // TODO
                // potential revert of config because of error (maybe retry)

                return new ApplySystemConfigurationResult
                {
                    Status = OperationStatus.Error,
                    Message = $"Apply configuration failed.{Environment.NewLine}{configurationResult.Messages}"
                };
            }
            else if (operationStatus == OperationStatus.Warning)
            {
                hasWarnings = true;

                // TODO
                // maybe some services have to be restarted to apply changes
            }

            await SaveLastAppliedConfiguration(proposedConfig, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // TODO
            // potential revert of config because of error (maybe retry)

            return new ApplySystemConfigurationResult
            {
                Status = OperationStatus.Error,
                Message = $"Apply configuration failed.{Environment.NewLine}{ex.StackTrace}"
            };
        }

        if (hasWarnings)
        {
            return new ApplySystemConfigurationResult
            {
                Status = OperationStatus.Warning,
                Message = $"Apply configuration completed with warnings.{Environment.NewLine}{configurationResult.Messages}"
            };
        }

        return new ApplySystemConfigurationResult
        {
            Status = OperationStatus.Success,
            Message = $"Apply configuration succeeded."
        };
    }

    /// <summary>
    /// Loads the system configuration from the system.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <remarks>
    /// <see cref="LoadSystemConfigurationResult.Message"/> contains:
    /// <para>
    /// - JSON string if <see cref="OperationStatus.Success"/> or <see cref="OperationStatus.Warning"/>
    /// </para>
    /// <para>
    /// - Non JSON string if <see cref="OperationStatus.Error"/>
    /// </para>
    /// <see cref="LoadSystemConfigurationResult.Configuration"/> contains:
    /// <para>
    /// - <see cref="SystemConfiguration"/> object if <see cref="OperationStatus.Success"/> or
    /// <see cref="OperationStatus.Warning"/>
    /// </para>
    /// <para>
    /// - <see langword="null"/> if <see cref="OperationStatus.Error"/>
    /// </para>
    /// </remarks>
    public async Task<LoadSystemConfigurationResult> LoadConfiguration(
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Currently we assume the config is unchanged by external factors
            // var lastAppliedConfig = await LoadLastAppliedConfiguration(cancellationToken).ConfigureAwait(false);
            // if (currentConfig != lastAppliedConfig) //TODO: proper comparison
            //     //TODO: Warn, but continue
            //     await SaveLastAppliedConfiguration(currentConfig, cancellationToken).ConfigureAwait(false);
            return await LoadLastAppliedConfiguration(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new LoadSystemConfigurationResult
            {
                Status = OperationStatus.Error,
                Message = $"Loading configuration failed.{Environment.NewLine}{ex.Message}",
                Configuration = null
            };
        }
    }

    private async Task<LoadSystemConfigurationResult> LoadCurrentConfiguration(
        CancellationToken cancellationToken)
    {
        var configurationResult = new Result();
        var currentConfig = new SystemConfiguration();
        var adapter = new SystemConfigurationAdapter(
            currentConfig,
            configurationResult,
            new ScriptHandler(configurationResult));

        var operationStatus = await adapter
            .LoadConfiguration(currentConfig, cancellationToken)
            .ConfigureAwait(false);

        if (operationStatus == OperationStatus.Error)
        {
            return new LoadSystemConfigurationResult
            {
                Status = OperationStatus.Error,
                Message = $"Loading configuration failed.{Environment.NewLine}{configurationResult.Messages}",
                Configuration = null
            };
        }
        else if (operationStatus == OperationStatus.Warning)
        {
            return new LoadSystemConfigurationResult
            {
                Status = OperationStatus.Warning,
                Message = $"Loading configuration completed with warnings.{Environment.NewLine}{configurationResult.Messages}",
                Configuration = currentConfig
            };
        }

        return new LoadSystemConfigurationResult
        {
            Status = OperationStatus.Success,
            Message = $"Loading configuration succeeded.",
            Configuration = currentConfig
        };
    }

    private async Task<LoadSystemConfigurationResult> LoadLastAppliedConfiguration(
        CancellationToken cancellationToken)
    {
        SystemConfiguration? loadedConfig;

        if (!File.Exists(ApplicationPaths.SystemConfigurationFilePath))
        {
            var loadedConfigResult = await LoadCurrentConfiguration(cancellationToken);

            if (loadedConfigResult.Status == OperationStatus.Success &&
                loadedConfigResult.Configuration is not null)
            {
                await SaveLastAppliedConfiguration(loadedConfigResult.Configuration, cancellationToken);
            }

            return loadedConfigResult;
        }
        else
        {
            var json = await File.ReadAllTextAsync(ApplicationPaths.SystemConfigurationFilePath, cancellationToken);

            loadedConfig = JsonSerializer
                .Deserialize(
                    json,
                    SourceGenerationContext.Default.SystemConfiguration)
                ?? throw new InvalidOperationException(
                    $"Last applied system configuration could not be deserialized to {nameof(SystemConfiguration)}");

            return new LoadSystemConfigurationResult
            {
                Status = OperationStatus.Success,
                Message = $"Loading last applied configuration succeeded.",
                Configuration = loadedConfig
            };
        }
    }

    private static async Task SaveLastAppliedConfiguration(SystemConfiguration appliedConfig,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var directoryPath = Path.GetDirectoryName(ApplicationPaths.SystemConfigurationFilePath)
                ?? throw new InvalidOperationException("SystemConfiguration file path was null.");

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            await using var fileStream = new FileStream(
                ApplicationPaths.SystemConfigurationFilePath,
                FileMode.Create,
                FileAccess.Write);

            await JsonSerializer
                .SerializeAsync(
                    fileStream,
                    appliedConfig,
                    SourceGenerationContext.Default.SystemConfiguration,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            throw;
        }
    }
}
