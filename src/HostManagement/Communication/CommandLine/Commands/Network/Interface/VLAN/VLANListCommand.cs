using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.VLAN;

/// <summary>
/// Represents the VLAN list command.
/// </summary>
internal sealed class VLANListCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="VLANListCommand"/>.
    /// </summary>
    public VLANListCommand()
        : base(CliStrings.Common.ListCommandName, CliStrings.Network.Interface.VLAN.ListCommandDescription)
        => SetAction((parseResult, cancellationToken) => ListHandler(cancellationToken));

    private static async Task ListHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var output = new PropertyListOutput().WithTitle(CliStrings.Network.Interface.Interfaces);

        foreach (var @interface in config.NetworkInterfaces)
        {
            var vlanId = @interface.VLAN.ID.Equals(0)
                ? CliOutputBase.NotSetValue
                : @interface.VLAN.ID.ToString();

            output.With(
                @interface.CommonInformation.Name,
                new(CliStrings.Network.Interface.VLAN.ID, vlanId),
                @interface.VLAN.Enabled);
        }

        output.WriteLine();
    }
}
