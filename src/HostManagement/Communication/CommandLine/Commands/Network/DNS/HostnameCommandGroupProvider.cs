using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the <see cref="HostnameCommandGroupProvider"/>.
/// </summary>
internal static class HostnameCommandGroupProvider
{
    private static readonly Option<string> s_hostnameOption = new(["-s", "--set"])
    {
        Description = "Set a new hostname.",
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Command s_hostnameCommand = CommandHelper.ConstructCommand(
        "hostname",
        "Configure hostname.",
        s_hostnameOption,
        Handler);

    /// <summary>
    /// Adds the command group to configure hostname.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddHostnameCommandGroup(this Command parentCommand)
    {
        parentCommand.AddCommand(s_hostnameCommand);
    }

    private static async Task Handler(string hostname)
    {
        if (string.IsNullOrWhiteSpace(hostname))
        {
            if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
                return;

            Console.WriteLine(config.NetworkDNSSettings.Hostname);
        }
        else
        {
            if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
                return;

            config.NetworkDNSSettings.Hostname = hostname;

            if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
                return;

            Console.WriteLine(CommandLineStrings.DNSHostnameSet, hostname);
        }

    }
}
