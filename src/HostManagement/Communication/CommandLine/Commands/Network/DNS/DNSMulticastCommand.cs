using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the DNS multicast command.
/// </summary>
internal sealed class DNSMulticastCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSMulticastCommand"/>.
    /// </summary>
    public DNSMulticastCommand() : base(
        CommandLineStrings_DNS.DNSMulticastCommandName,
        CommandLineStrings_DNS.DNSMulticastCommandDescription)
    {
        Add(
            CommandLineStrings_Common.EnableCommandName,
            CommandLineStrings_DNS.DNSMulticastEnableCommandDescription,
            () => EnableDisableHandler(true));

        Add(
            CommandLineStrings_Common.DisableCommandName,
            CommandLineStrings_DNS.DNSMulticastDisableCommandDescription,
            () => EnableDisableHandler(false));

        Add(
            CommandLineStrings_Common.StatusCommandName,
            CommandLineStrings_DNS.DNSMulticastStatusCommandDescription,
            StatusHandler);
    }

    private static async Task StatusHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var enableMessage = config.NetworkDNSSettings.MulticastDNSEnabled
            ? CommandLineStrings_DNS.MulticastDNSIsEnabled
            : CommandLineStrings_DNS.MulticastDNSIsDisabled;

        SimpleOutput.WriteLine(enableMessage);
    }

    private static async Task EnableDisableHandler(bool enable)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.MulticastDNSEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings_DNS.MulticastDNSAlreadyEnabled
                : CommandLineStrings_DNS.MulticastDNSAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info);

            return;
        }

        config.NetworkDNSSettings.MulticastDNSEnabled = enable;

        if (!await SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings_DNS.MulticastDNSEnabled
            : CommandLineStrings_DNS.MulticastDNSDisabled;

        SimpleOutput.WriteLine(enableMessage);
    }
}
