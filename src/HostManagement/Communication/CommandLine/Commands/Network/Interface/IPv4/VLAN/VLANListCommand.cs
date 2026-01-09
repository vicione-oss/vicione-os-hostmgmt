using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.VLAN;

/// <summary>
/// Represents the VLAN list command.
/// </summary>
internal sealed class VLANListCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="VLANListCommand"/>.
    /// </summary>
    public VLANListCommand() : base(
        CommandLineStrings_Common.ListCommandName,
        CommandLineStrings_NetworkInterfaces.VLANListCommandDescription)
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
            var vlanId = @interface.IPv4.VLANID.Equals(0)
                ? CliOutputBase.NotSetValue
                : @interface.IPv4.VLANID.ToString();

            output.With(
                @interface.CommonInformation.Name,
                new(CommandLineStrings_NetworkInterfaces.VLANID, vlanId),
                @interface.IPv4.VLANEnabled);
        }

        output.WriteLine();
    }
}
