using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.Hostname;

/// <summary>
/// Represents the hostname command.
/// </summary>
internal sealed class DNSHostnameCommand : ConfigCommandBase
{
    private static readonly Argument<string> s_hostnameArgument = new("name")
    {
        Description = "The new hostname.",
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="DNSHostnameCommand"/>.
    /// </summary>
    public DNSHostnameCommand() : base(
        CommandLineStrings_DNS.DNSHostnameCommandName,
        CommandLineStrings_DNS.DNSHostnameCommandDescription)
    {
        Add(s_hostnameArgument);

        SetAction((parseResult, cancellationToken) =>
        {
            return HostNameHandler(
                parseResult.GetValue(s_hostnameArgument),
                cancellationToken);
        });
    }

    private static async Task HostNameHandler(string? hostname, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(hostname))
        {
            if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
                return;

            new KeyValueOutput()
                .With(new(CommandLineStrings_DNS.Hostname, config.NetworkDNSSettings.Hostname))
                .WriteLine();
        }
        else
        {
            if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
                return;

            config.NetworkDNSSettings.Hostname = hostname;

            if (!await SetConfiguration(config, cancellationToken))
                return;

            SimpleOutput.WriteLine(CommandLineStrings_DNS.HostnameSet, hostname);
        }
    }
}
