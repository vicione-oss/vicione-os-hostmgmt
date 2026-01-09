using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the <see cref="MulticastDNSCommandGroupProvider"/>.
/// </summary>
internal static class MulticastDNSCommandGroupProvider
{
    private static readonly Command s_statusCommand = CommandHelper.ConstructCommand(
        "status",
        "Show mDNS enabled status.",
        StatusHandler);

    private static readonly Command s_enableCommand = CommandHelper.ConstructCommand(
        "enable",
        "Enable mDNS.",
        () => EnableDisableHandler(enable: true));

    private static readonly Command s_disableCommand = CommandHelper.ConstructCommand(
        "disable",
        "Disable mDNS.",
        () => EnableDisableHandler(enable: false));

    /// <summary>
    /// Adds the command group to configure multicast DNS (mDNS).
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddMulticastDNSCommandGroup(this Command parentCommand)
    {
        var multicastDnsCommand = parentCommand.AddCommand(
            "mdns",
            "Configure multicast DNS (mDNS).");

        multicastDnsCommand.AddCommand(s_statusCommand);
        multicastDnsCommand.AddCommand(s_enableCommand);
        multicastDnsCommand.AddCommand(s_disableCommand);
    }

    private static async Task StatusHandler()
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        Console.WriteLine(
            config.NetworkDNSSettings.MulticastDNSEnabled
            ? CommandLineStrings.MulticastDNSIsEnabled
            : CommandLineStrings.MulticastDNSIsDisabled);
    }

    private static async Task EnableDisableHandler(bool enable)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.MulticastDNSEnabled.Equals(enable))
        {
            ConsoleEx.WriteLineWarning(
                enable
                ? CommandLineStrings.MulticastDNSAlreadyEnabled
                : CommandLineStrings.MulticastDNSAlreadyDisabled);

            return;
        }

        config.NetworkDNSSettings.MulticastDNSEnabled = enable;

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            enable
            ? CommandLineStrings.MulticastDNSEnabled
            : CommandLineStrings.MulticastDNSDisabled);
    }
}
