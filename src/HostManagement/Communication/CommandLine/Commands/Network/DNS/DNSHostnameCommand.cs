using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the hostname command.
/// </summary>
internal sealed class DNSHostnameCommand : ConfigCommandBase
{
    private static readonly Option<string> s_hostnameOption = new(["-s", "--set"])
    {
        Description = "Set a new hostname.",
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="DNSHostnameCommand"/>.
    /// </summary>
    public DNSHostnameCommand() : base(
        CommandLineStrings_DNS.DNSHostnameCommandName,
        CommandLineStrings_DNS.DNSHostnameCommandDescription)
    {
        AddOption(s_hostnameOption);
        this.SetHandler(HostNameHandler, s_hostnameOption);
    }

    private static async Task HostNameHandler(string hostname)
    {
        if (string.IsNullOrWhiteSpace(hostname))
        {
            if (await GetConfiguration() is not SystemConfiguration config)
                return;

            new KeyValueOutput()
                .With(new(CommandLineStrings_DNS.Hostname, config.NetworkDNSSettings.Hostname))
                .WriteLine();
        }
        else
        {
            if (await GetConfiguration() is not SystemConfiguration config)
                return;

            config.NetworkDNSSettings.Hostname = hostname;

            if (!await SetConfiguration(config))
                return;

            SimpleOutput.WriteLine(CommandLineStrings_DNS.HostnameSet, hostname);
        }
    }
}
