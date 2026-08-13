using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    public DHCPListCommand()
        : base(
            CliStrings.Common.ListCommandName,
            CommandLineStrings_NetworkInterfaces.DHCPListCommandDescription)
        => SetAction((parseResult, cancellationToken) => ListHandler(cancellationToken));

    private static async Task ListHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var output = new PropertyListOutput().WithTitle(CommandLineStrings_NetworkInterfaces.Interfaces);

        foreach (var @interface in config.NetworkInterfaces)
        {
            var dhcpEnabledString = @interface.IPv4.DHCPEnabled
                ? CliStrings.Common.EnabledState
                : CliStrings.Common.DisabledState;

            output.With(
                @interface.CommonInformation.Name,
                new(CommandLineStrings_NetworkInterfaces.DHCP, dhcpEnabledString));
        }

        output.WriteLine();
    }
}
