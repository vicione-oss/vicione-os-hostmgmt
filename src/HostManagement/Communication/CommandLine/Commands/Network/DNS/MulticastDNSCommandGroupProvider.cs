using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
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
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var enableMessage = config.NetworkDNSSettings.MulticastDNSEnabled
            ? CommandLineStrings.MulticastDNSIsEnabled
            : CommandLineStrings.MulticastDNSIsDisabled;

        SimpleOutput.WriteLine(enableMessage);
    }

    private static async Task EnableDisableHandler(bool enable)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.MulticastDNSEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings.MulticastDNSAlreadyEnabled
                : CommandLineStrings.MulticastDNSAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info);

            return;
        }

        config.NetworkDNSSettings.MulticastDNSEnabled = enable;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings.MulticastDNSEnabled
            : CommandLineStrings.MulticastDNSDisabled;

        SimpleOutput.WriteLine(enableMessage);
    }
}
