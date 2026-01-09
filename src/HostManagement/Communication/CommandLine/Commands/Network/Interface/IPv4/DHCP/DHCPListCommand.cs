using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.DHCP;

/// <summary>
/// Represents the DHCP list command.
/// </summary>
internal sealed class DHCPListCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DHCPListCommand"/>.
    /// </summary>
    public DHCPListCommand() : base(
        CommandLineStrings_Common.ListCommandName,
        CommandLineStrings_NetworkInterfaces.DHCPListCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => ListHandler(cancellationToken));
    }

    private static async Task ListHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var output = new PropertyListOutput().WithTitle(CommandLineStrings_NetworkInterfaces.Interfaces);

        foreach (var @interface in config.NetworkInterfacesSettings.NetworkInterfaces)
        {
            var dhcpEnabledString = @interface.IPv4.DHCPEnabled
                ? CommandLineStrings_Common.enabled
                : CommandLineStrings_Common.disabled;

            output.With(
                @interface.CommonInformation.Name,
                new(CommandLineStrings_NetworkInterfaces.DHCP, dhcpEnabledString));
        }

        output.WriteLine();
    }
}
