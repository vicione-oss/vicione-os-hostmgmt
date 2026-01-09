using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.DHCP;

/// <summary>
/// Represents the <see cref="EnableDHCPCommandGroupProvider"/>.
/// </summary>
internal static class EnableDHCPCommandGroupProvider
{
    private static readonly Option<string> s_nameOption = CommonOptions.s_interfaceNameOption;

    /// <summary>
    /// Adds the command group to enable DHCP of a specified network interface.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddEnableDHCPCommandGroup(this Command parentCommand)
    {
        var enableDHCPCommand = parentCommand.AddCommand(
            "enable-dhcp",
            "Enable DHCP for a specified network interface.");

        enableDHCPCommand.AddOption(s_nameOption);
        enableDHCPCommand.SetHandler(EnableDHCPHandler, s_nameOption);
    }

    private static async Task EnableDHCPHandler(string nameOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var networkInterfaces = config.NetworkInterfacesSettings.NetworkInterfaces;

        foreach (var networkInterface in networkInterfaces)
        {
            if (networkInterface.CommonInformation.Name.Equals(nameOptionValue))
            {
                if (networkInterface.IPv4.DHCPEnabled)
                {
                    Console.WriteLine(CommandLineStrings.DHCPAlreadyEnabled, nameOptionValue);
                    return;
                }

                networkInterface.IPv4.DHCPEnabled = true;
                networkInterface.IPv4.Gateway = null;
                networkInterface.IPv4.IPv4Details.Clear();

                if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
                    return;

                Console.WriteLine(CommandLineStrings.DHCPEnabled, nameOptionValue);
                return;
            }
        }

        ConsoleEx.WriteLineError(CommandLineStrings.NetworkInterfaceNotFound, nameOptionValue);
    }
}
