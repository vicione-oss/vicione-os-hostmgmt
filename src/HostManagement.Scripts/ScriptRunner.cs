using System.Diagnostics;
using HostManagement.Configuration;
using HostManagement.Scripts.Enums;
using HostManagement.Scripts.Extensions;

namespace HostManagement.Scripts;

/// <summary>
/// The <see cref="ScriptRunner"/> provides functionality to execute scripts based on the given
/// <see cref="ScriptIdentifier"/> and script arguments. Optionally a custom timeout can be specified.
/// </summary>
public static class ScriptRunner
{
    /// <summary>
    /// The default timeout in milliseconds for script execution.
    /// </summary>
    public static int DefaultTimeoutInMilliseconds => 60_000;

    /// <summary>
    /// Executes the corresponding script based on the given <see cref="ScriptIdentifier"/> and
    /// <paramref name="scriptArguments"/>.
    /// </summary>
    /// <inheritdoc cref="Run(int, ScriptIdentifier, IEnumerable{string})"/>
    public static async Task<ScriptResponse> Run(
        ScriptIdentifier scriptIdentifier,
        params IEnumerable<string>? scriptArguments)
    {
        return await Run(DefaultTimeoutInMilliseconds, scriptIdentifier, scriptArguments);
    }

    /// <summary>
    /// Executes the corresponding script based on the given <paramref name="timeoutInMilliseconds"/>,
    /// <see cref="ScriptIdentifier"/> and <paramref name="scriptArguments"/>.
    /// </summary>
    /// <param name="timeoutInMilliseconds">The timeout in milliseconds.</param>
    /// <param name="scriptIdentifier">The script identifier.</param>
    /// <param name="scriptArguments">Optional script arguments.</param>
    /// <remarks>
    /// <see cref="ScriptResponse.StandardOutput"/> contains:
    /// <para>
    /// - JSON string when getting information via script succeeded (<see cref="ScriptResponse.ExitCode"/> 0)
    /// </para>
    /// <para>
    /// - Non JSON string when setting properties via script succeeded (<see cref="ScriptResponse.ExitCode"/> 0)
    /// or failed (<see cref="ScriptResponse.ExitCode"/> > 0)
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
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="TimeoutException"></exception>
    public static async Task<ScriptResponse> Run(
        int timeoutInMilliseconds,
        ScriptIdentifier scriptIdentifier,
        params IEnumerable<string>? scriptArguments)
    {
        if (timeoutInMilliseconds < DefaultTimeoutInMilliseconds)
            timeoutInMilliseconds = DefaultTimeoutInMilliseconds;

        using var process = new Process()
        {
            StartInfo = GetProcessStartInfo(
                GetScriptContent(scriptIdentifier),
                GetFormattedArguments(scriptArguments))
        };

#if DEBUG
        var stopwatch = Stopwatch.StartNew();
#endif
        try
        {
            while (ResourceManager.GetInstance().IsDeploymentInProgress)
                await Task.Delay(500);

            process.Start();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to start process for script with identifier '{scriptIdentifier}'.", ex);
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutInMilliseconds));

        try
        {
            await process.WaitForExitAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(true);

                throw new TimeoutException(
                    $"Script with identifier '{scriptIdentifier}' exceeded the timeout of " +
                    $"{timeoutInMilliseconds:N0} milliseconds and was aborted.");
            }
        }

        var output = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
        var error = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
#if DEBUG
        stopwatch.Stop();
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Script with identifier '{scriptIdentifier}' took {stopwatch.Elapsed}");
        Console.ResetColor();
#endif
        return new ScriptResponse(process.ExitCode, output.TrimEnd(), error.TrimEnd());
    }

    private static ProcessStartInfo GetProcessStartInfo(string scriptContent, string scriptArguments)
    {
        var argument = $"temp_function () {{ {scriptContent} }} && temp_function {scriptArguments}";

        return new ProcessStartInfo()
        {
            FileName = @"/bin/bash",
            WorkingDirectory = EnvironmentVariables.ResourceDeployDirectory,
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
        var scriptContent = scriptIdentifier switch
        {
            ScriptIdentifier.SetExecutePermission => ResourceManager.GetInstance().GetResource("set_execute_permission"),
            ScriptIdentifier.CheckUserExists => ResourceManager.GetInstance().GetResource("check_user_exists"),
            ScriptIdentifier.CheckGroupExists => ResourceManager.GetInstance().GetResource("check_group_exists"),
            ScriptIdentifier.SetOwnership => ResourceManager.GetInstance().GetResource("set_ownership"),

            ScriptIdentifier.NetworkInterfacesGet => ResourceManager.GetInstance().GetResource("get_network_interfaces"),

            ScriptIdentifier.ApplyNetplanConfiguration => ResourceManager.GetInstance().GetResource("apply_netplan_configuration"),
            ScriptIdentifier.CommonSystemArchitectureGet => ResourceManager.GetInstance().GetResource("get_system_architecture"),

            ScriptIdentifier.HostManagementInstalledVersionGet => ResourceManager.GetInstance().GetResource("get_hostmanagement_installed_version"),
            ScriptIdentifier.InstalledPackageVersionGet => ResourceManager.GetInstance().GetResource("get_installed_package_version"),

            ScriptIdentifier.NetworkInterfacesCommonInformationGet => ResourceManager.GetInstance().GetResource("get_network_interfaces_common_information"),
            ScriptIdentifier.NetworkInterfaceIPv4SettingsGet => ResourceManager.GetInstance().GetResource("get_network_interface_ipv4_settings"),
            ScriptIdentifier.NetworkInterfaceIPv4SettingsSet => ResourceManager.GetInstance().GetResource("set_network_interface_ipv4_settings"),
            ScriptIdentifier.NetworkInterfaceIPv4DHCPLeaseGet => ResourceManager.GetInstance().GetResource("get_network_interface_ipv4_dhcp_lease"),
            ScriptIdentifier.NetworkDNSSettingsGet => ResourceManager.GetInstance().GetResource("get_network_dns_settings"),
            ScriptIdentifier.NetworkDNSSettingsSet => ResourceManager.GetInstance().GetResource("set_network_dns_settings"),
            ScriptIdentifier.NetworkMulticastDNSSettingsSet => ResourceManager.GetInstance().GetResource("set_network_multicast_dns_settings"),
            ScriptIdentifier.NetworkNTPSettingsGet => ResourceManager.GetInstance().GetResource("get_network_ntp_settings"),
            ScriptIdentifier.NetworkNTPSettingsSet => ResourceManager.GetInstance().GetResource("set_network_ntp_settings"),
            ScriptIdentifier.NetworkProxySettingsGet => ResourceManager.GetInstance().GetResource("get_network_proxy_settings"),
            ScriptIdentifier.NetworkProxySettingsSet => ResourceManager.GetInstance().GetResource("set_network_proxy_settings"),
            ScriptIdentifier.NetworkStaticHostsSet => ResourceManager.GetInstance().GetResource("set_static_hosts"),
            ScriptIdentifier.NetworkStatusInformationGet => ResourceManager.GetInstance().GetResource("get_network_status_information"),
            ScriptIdentifier.RenewDHCPLease => ResourceManager.GetInstance().GetResource("renew_dhcp_lease"),
            ScriptIdentifier.ViciOneSuiteVersionsGet => ResourceManager.GetInstance().GetResource("get_vicione_suite_versions"),
            ScriptIdentifier.ViciOneSuiteInstalledVersionGet => ResourceManager.GetInstance().GetResource("get_vicione_suite_installed_version"),
            ScriptIdentifier.ViciOneSuiteDebFileGet => ResourceManager.GetInstance().GetResource("get_vicione_suite_deb_file"),
            ScriptIdentifier.ViciOneSuiteInstallDebFile => ResourceManager.GetInstance().GetResource("install_deb_file"),
            ScriptIdentifier.ServiceStateGet => ResourceManager.GetInstance().GetResource("get_service_state"),
            ScriptIdentifier.ServiceStateSet => ResourceManager.GetInstance().GetResource("set_service_state"),
            ScriptIdentifier.ResetSystem => ResourceManager.GetInstance().GetResource("reset_system"),
            ScriptIdentifier.RestartSystem => ResourceManager.GetInstance().GetResource("restart_system"),
            _ => throw new InvalidOperationException($"Unknown script identifier '{scriptIdentifier}'."),
        };

        if (string.IsNullOrWhiteSpace(scriptContent))
        {
            throw new InvalidOperationException(
                $"Failed to get script content for script identifier '{scriptIdentifier}'.");
        }

        return scriptContent.EnsureLFLineEndings();
    }

    private static string GetFormattedArguments(params IEnumerable<string>? arguments)
    {
        if (arguments is null)
            return string.Empty;

        // Escape arguments in case they contain spaces or special characters
        var formattedArguments = arguments.Select(argument => $"\"{argument.Replace("\"", "\\\"")}\"");

        return string.Join(" ", formattedArguments);
    }
}
