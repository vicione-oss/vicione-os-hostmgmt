using System.Diagnostics;
using HostManagement.Scripts.Enums;
using HostManagement.Scripts.Extensions;

namespace HostManagement.Scripts;

/// <summary>
/// Represents the <see cref="ScriptRunner"/> responsible for executing scripts based on the given
/// <see cref="ScriptIdentifier"/> and script arguments. Optionally a custom timeout can be specified.
/// </summary>
/// <remarks>
/// <see cref="ScriptRunner"/> has to be initialized via <see cref="Initialize(string)"/>.
/// </remarks>
public sealed class ScriptRunner
{
    private static readonly Lock s_instanceLock = new();
    private static ScriptRunner? s_instance;

    private readonly string _resourceDeployDirectory;

    private ScriptRunner(string resourceDeployDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceDeployDirectory);

        _resourceDeployDirectory = resourceDeployDirectory;
    }

    /// <summary>
    /// Gets the <see cref="ScriptRunner"/> instance after it was initialized via <see cref="Initialize(string)"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException"/>
    public static ScriptRunner GetInstance()
    {
        if (s_instance is null)
        {
            throw new InvalidOperationException(
                $"{nameof(ScriptRunner)} is not initialized, use {nameof(Initialize)} first.");
        }

        return s_instance;
    }

    /// <summary>
    /// Initializes the <see cref="ScriptRunner"/>.
    /// </summary>
    /// <param name="resourceDeployDirectory">The resource deploy directory.</param>
    /// <returns>The <see cref="ScriptRunner"/> instance</returns>
    public static ScriptRunner Initialize(string resourceDeployDirectory)
    {
        if (s_instance is null)
        {
            lock (s_instanceLock)
            {
#pragma warning disable CA1508
                s_instance ??= new ScriptRunner(resourceDeployDirectory);
#pragma warning restore CA1508
            }
        }

        return s_instance;
    }

    /// <summary>
    /// Resets the <see cref="ScriptRunner"/> instance.
    /// </summary>
    /// <remarks>Must only be used for unit tests.</remarks>
    internal static void ResetInstance()
    {
        lock (s_instanceLock)
        {
            s_instance = null;
        }
    }

    /// <summary>
    /// The default timeout in milliseconds for script execution.
    /// </summary>
    public static int DefaultTimeoutInMilliseconds => 60_000;

    /// <summary>
    /// Executes the corresponding script based on the given <paramref name="scriptIdentifier"/> and
    /// <paramref name="scriptArguments"/>.
    /// </summary>
    /// <inheritdoc cref="Run(int, ScriptIdentifier, IEnumerable{string})"/>
    public async Task<ScriptResponse> Run(
        ScriptIdentifier scriptIdentifier,
        params IEnumerable<string> scriptArguments)
        => await Run(DefaultTimeoutInMilliseconds, scriptIdentifier, scriptArguments);

    /// <summary>
    /// Executes the corresponding script based on the given <paramref name="timeoutInMilliseconds"/>,
    /// <paramref name="scriptIdentifier"/> and <paramref name="scriptArguments"/>.
    /// </summary>
    /// <inheritdoc cref="Run(int, ScriptIdentifier, IProcessWrapper, CancellationToken, IEnumerable{string})"/>
    public async Task<ScriptResponse> Run(
        int timeoutInMilliseconds,
        ScriptIdentifier scriptIdentifier,
        params IEnumerable<string> scriptArguments)
    {
        return await Run(
            timeoutInMilliseconds,
            scriptIdentifier,
            new ProcessWrapper(),
            CancellationToken.None,
            scriptArguments);
    }

    /// <summary>
    /// Executes the corresponding script based on the given <paramref name="scriptIdentifier"/> and
    /// <paramref name="scriptArguments"/>.
    /// </summary>
    /// <inheritdoc cref="Run(int, ScriptIdentifier, CancellationToken, IEnumerable{string})"/>
    public async Task<ScriptResponse> Run(
        ScriptIdentifier scriptIdentifier,
        CancellationToken cancellationToken,
        params IEnumerable<string> scriptArguments)
    {
        return await Run(DefaultTimeoutInMilliseconds, scriptIdentifier, cancellationToken, scriptArguments);
    }

    /// <summary>
    /// Executes the corresponding script based on the given <paramref name="timeoutInMilliseconds"/>,
    /// <paramref name="scriptIdentifier"/> and <paramref name="scriptArguments"/>.
    /// </summary>
    /// <inheritdoc cref="Run(int, ScriptIdentifier, IProcessWrapper, CancellationToken, IEnumerable{string})"/>
    public async Task<ScriptResponse> Run(
        int timeoutInMilliseconds,
        ScriptIdentifier scriptIdentifier,
        CancellationToken cancellationToken,
        params IEnumerable<string> scriptArguments)
    {
        return await Run(
            timeoutInMilliseconds,
            scriptIdentifier,
            new ProcessWrapper(),
            cancellationToken,
            scriptArguments);
    }

    /// <summary>
    /// Executes the corresponding script based on the given <paramref name="timeoutInMilliseconds"/>,
    /// <paramref name="scriptIdentifier"/> and <paramref name="scriptArguments"/>.
    /// </summary>
    /// <param name="timeoutInMilliseconds">The timeout in milliseconds.</param>
    /// <param name="scriptIdentifier">The script identifier.</param>
    /// <param name="processWrapper">An <see cref="IProcessWrapper"/> implementation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <param name="scriptArguments">Optional script arguments.</param>
    /// <remarks>
    /// <see cref="ScriptResponse.StandardOutput"/> contains:
    /// <para>
    /// - JSON string or Non JSON string when getting information via script succeeded
    /// (<see cref="ScriptResponse.ExitCode"/> 0)
    /// </para>
    /// <see cref="ScriptResponse.StandardError"/> contains:
    /// <para>
    /// - Errors if the script failed (<see cref="ScriptResponse.ExitCode"/> > 0)
    /// </para>
    /// <para>
    /// - Optional warnings if the script succeeded (<see cref="ScriptResponse.ExitCode"/> 0)
    /// </para>
    /// </remarks>
    /// <returns><see cref="ScriptResponse"/></returns>
    /// <exception cref="OperationCanceledException"></exception>
    /// <exception cref="ProcessWrapperStartException"></exception>
    /// <exception cref="TimeoutException"></exception>
    internal async Task<ScriptResponse> Run(
        int timeoutInMilliseconds,
        ScriptIdentifier scriptIdentifier,
        IProcessWrapper processWrapper,
        CancellationToken cancellationToken,
        params IEnumerable<string> scriptArguments)
    {
        if (timeoutInMilliseconds < DefaultTimeoutInMilliseconds)
            timeoutInMilliseconds = DefaultTimeoutInMilliseconds;

        var processStartInfo = GetProcessStartInfo(
            GetScriptContent(scriptIdentifier),
            ScriptArgumentFormatter.GetFormattedArguments(scriptArguments));

        while (ResourceManager.GetInstance().IsDeploymentInProgress)
            await Task.Delay(100);

#if DEBUG
        var stopwatch = Stopwatch.StartNew();
#endif

        ScriptResponse scriptResponse;

        try
        {
            scriptResponse = await processWrapper.StartScriptAsync(
                processStartInfo,
                timeoutInMilliseconds,
                cancellationToken);
        }
        catch (ProcessWrapperStartException ex)
        {
            return ScriptResponse.CreateErrorResponse(
                $"Failed to start process for script with identifier '{scriptIdentifier}'. {ex}");
        }
        catch (TimeoutException)
        {
            return ScriptResponse.CreateErrorResponse(
                $"Script with identifier '{scriptIdentifier}' exceeded the timeout of " +
                $"{timeoutInMilliseconds:N0} milliseconds and was aborted.");
        }
        catch (OperationCanceledException)
        {
            return ScriptResponse.CreateErrorResponse(
                $"Script with identifier '{scriptIdentifier}' was cancelled.");
        }
#if DEBUG
        finally
        {
            stopwatch.Stop();
        }

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Script with identifier '{scriptIdentifier}' took {stopwatch.Elapsed}");
        Console.ResetColor();
#endif

        return scriptResponse;
    }

    private ProcessStartInfo GetProcessStartInfo(string scriptContent, string scriptArguments)
    {
        var argument = $"temp_function () {{ {scriptContent} }} && temp_function {scriptArguments}";

        return new ProcessStartInfo()
        {
            FileName = @"/bin/bash",
            WorkingDirectory = _resourceDeployDirectory,
            ArgumentList = { "-c", argument },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };
    }

    private static string GetScriptContent(ScriptIdentifier scriptIdentifier)
    {
        var resourceManager = ResourceManager.GetInstance();

        var scriptContent = scriptIdentifier switch
        {
            // System
            ScriptIdentifier.SetExecutePermission => resourceManager.GetResource("set_execute_permission"),
            ScriptIdentifier.CheckUserExists => resourceManager.GetResource("check_user_exists"),
            ScriptIdentifier.CheckGroupExists => resourceManager.GetResource("check_group_exists"),
            ScriptIdentifier.SetOwnership => resourceManager.GetResource("set_ownership"),

            ScriptIdentifier.CommonSystemArchitectureGet => resourceManager.GetResource("get_system_architecture"),

            ScriptIdentifier.ResetSystem => resourceManager.GetResource("reset_system"),
            ScriptIdentifier.ShutdownSystem => resourceManager.GetResource("shutdown_system"),
            ScriptIdentifier.UpdateSystem => resourceManager.GetResource("update_system"),

            ScriptIdentifier.SystemdUnitActiveStateGet => resourceManager.GetResource("get_systemd_unit_active_state"),
            ScriptIdentifier.SystemdUnitStateGet => resourceManager.GetResource("get_systemd_unit_state"),
            ScriptIdentifier.SystemdUnitStateSet => resourceManager.GetResource("set_systemd_unit_state"),

            // Network
            ScriptIdentifier.ApplyNetplanConfiguration => resourceManager.GetResource("apply_netplan_configuration"),
            ScriptIdentifier.IsNetworkInterfaceAvailable => resourceManager.GetResource("is_network_interface_available"),
            ScriptIdentifier.NetworkInterfacesGet => resourceManager.GetResource("get_network_interfaces"),
            ScriptIdentifier.OriginalPhysicalAddressGet => resourceManager.GetResource("get_original_physical_address"),
            ScriptIdentifier.NetworkInterfacesCommonInformationGet => resourceManager.GetResource("get_network_interfaces_common_information"),
            ScriptIdentifier.NetworkInterfaceSettingsGet => resourceManager.GetResource("get_network_interface_settings"),
            ScriptIdentifier.NetworkInterfaceSettingsSet => resourceManager.GetResource("set_network_interface_settings"),
            ScriptIdentifier.NetworkInterfaceIPv4DHCPLeaseGet => resourceManager.GetResource("get_network_interface_ipv4_dhcp_lease"),
            ScriptIdentifier.RenewDHCPLease => resourceManager.GetResource("renew_dhcp_lease"),
            ScriptIdentifier.NetworkStatusInformationGet => resourceManager.GetResource("get_network_status_information"),

            // DNS
            ScriptIdentifier.HostnameSet => resourceManager.GetResource("set_hostname"),
            ScriptIdentifier.NetworkStaticHostsSet => resourceManager.GetResource("set_static_hosts"),
            ScriptIdentifier.NetworkMulticastDNSSettingsSet => resourceManager.GetResource("set_network_multicast_dns_settings"),
            ScriptIdentifier.NetworkDNSSettingsGet => resourceManager.GetResource("get_network_dns_settings"),
            ScriptIdentifier.NetworkDNSSettingsSet => resourceManager.GetResource("set_network_dns_settings"),

            // Proxy
            ScriptIdentifier.NetworkProxySettingsGet => resourceManager.GetResource("get_network_proxy_settings"),
            ScriptIdentifier.NetworkProxySettingsSet => resourceManager.GetResource("set_network_proxy_settings"),

            // NTP
            ScriptIdentifier.NetworkNTPFallbackInformationGet => resourceManager.GetResource("get_network_ntp_fallback_information"),
            ScriptIdentifier.NetworkNTPSettingsGet => resourceManager.GetResource("get_network_ntp_settings"),
            ScriptIdentifier.NetworkNTPSettingsSet => resourceManager.GetResource("set_network_ntp_settings"),

            // Install
            ScriptIdentifier.InstallDebianPackage => resourceManager.GetResource("install_deb_package"),

            // Test
            ScriptIdentifier.UnitTesting => resourceManager.GetResource("UnitTesting"),

            // Unknown
            _ => throw new InvalidOperationException($"Unknown script identifier '{scriptIdentifier}'."),
        };

        if (string.IsNullOrWhiteSpace(scriptContent))
        {
            throw new InvalidOperationException(
                $"Script content for script with identifier '{scriptIdentifier}' was empty.");
        }

        return scriptContent.EnsureLFLineEndings();
    }
}
