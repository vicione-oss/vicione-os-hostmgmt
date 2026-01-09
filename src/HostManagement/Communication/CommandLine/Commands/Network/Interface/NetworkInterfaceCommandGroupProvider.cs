using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface;

/// <summary>
/// Represents the <see cref="NetworkInterfaceCommandGroupProvider"/>.
/// </summary>
internal static class NetworkInterfaceCommandGroupProvider
{
    private static readonly Option<bool> s_enableOption = new(["-e", "--enable"])
    {
        Description = "Enable or disable the specified network interface.",
        IsRequired = true,
        Arity = ArgumentArity.ZeroOrOne
    };

    /// <summary>
    /// Adds the command group to configure network interface settings.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    /// <exception cref="InvalidOperationException"></exception>
    internal static void AddNetworkInterfaceCommandGroup(this Command parentCommand)
    {
        var interfaceCommand = parentCommand.AddCommand(
            "interface",
            "Configure a specified network interface.");

        interfaceCommand.AddOption(CommonOptions.s_interfaceNameOption);
        interfaceCommand.AddOption(s_enableOption);
        interfaceCommand.AddIPv4CommandGroup();
        interfaceCommand.SetHandler(Handler, CommonOptions.s_interfaceNameOption, s_enableOption);
    }

    private static async Task Handler(string nameOptionValue, bool enableOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var networkInterfacesSettings = config.NetworkInterfacesSettings;

        foreach (var networkInterface in networkInterfacesSettings.NetworkInterfaces)
        {
            if (networkInterface.CommonInformation.Name.Equals(nameOptionValue))
            {
                if (networkInterface.CommonInformation.Enabled &&
                    enableOptionValue)
                {
                    Console.WriteLine(
                        CommandLineStrings.NetworkInterfaceAlreadyEnabled,
                        nameOptionValue);

                    return;
                }

                if (!networkInterface.CommonInformation.Enabled &&
                    !enableOptionValue)
                {
                    Console.WriteLine(
                        CommandLineStrings.NetworkInterfaceAlreadyDisabled,
                        nameOptionValue);

                    return;
                }

                networkInterface.CommonInformation.Enabled = enableOptionValue;

                if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
                    return;

                Console.WriteLine(
                    CommandLineStrings.NetworkInterfaceEnabledDisabled,
                    nameOptionValue,
                    enableOptionValue ? CommandLineStrings.enabled : CommandLineStrings.disabled);

                return;
            }
        }

        ConsoleEx.WriteLineError(CommandLineStrings.NetworkInterfaceNotFound, nameOptionValue);
    }
}
