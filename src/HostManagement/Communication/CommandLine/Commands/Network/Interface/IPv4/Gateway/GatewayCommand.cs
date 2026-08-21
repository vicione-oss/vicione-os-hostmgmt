using System.CommandLine;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.Gateway;

/// <summary>
/// Represents the gateway command.
/// </summary>
internal sealed class GatewayCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="GatewayCommand"/>.
    /// </summary>
    public GatewayCommand()
        : base(
            CliStrings.Network.Interface.Gateway.CommandName,
            CliStrings.Network.Interface.Gateway.CommandDescription)
    {
        Add(new GatewaySetCommand(CommonOptions.s_interfaceNameOption, CommonOptions.s_ipAddressOption));
        Add(new GatewayRemoveCommand());
    }
}
