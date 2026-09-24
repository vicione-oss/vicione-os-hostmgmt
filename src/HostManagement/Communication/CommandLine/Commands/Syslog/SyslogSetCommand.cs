using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Syslog;

namespace HostManagement.Communication.CommandLine.Commands.Syslog;

/// <summary>
/// Represents the syslog set command.
/// </summary>
internal sealed class SyslogSetCommand : ConfigCommandBase
{
    private static readonly Option<string> s_addressOption = new("--address", "-a")
    {
        Description = "The destination address and port (host:port).",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Option<SyslogTransportProtocol> s_protocolOption = new("--protocol", "-p")
    {
        Description = "The transport protocol.",
        Arity = ArgumentArity.ExactlyOne,
        DefaultValueFactory = _ => SyslogTransportProtocol.Udp
    };

    private static readonly Option<SyslogLogFormat> s_formatOption = new("--format", "-f")
    {
        Description = "The syslog message format.",
        Arity = ArgumentArity.ExactlyOne,
        DefaultValueFactory = _ => SyslogLogFormat.Rfc5424
    };

    private static readonly Option<uint> s_retrySecondsOption = new("--retry-seconds", "-r")
    {
        Description = "The reconnect delay in seconds.",
        Arity = ArgumentArity.ExactlyOne,
        DefaultValueFactory = _ => 30
    };

    private static readonly Option<SyslogFacility[]> s_excludeFacilitiesOption = new("--exclude-facilities", "-ef")
    {
        Description = "The list of facilities to exclude (e.g.: 'auth authpriv').",
        Arity = ArgumentArity.ZeroOrMore,
        AllowMultipleArgumentsPerToken = true,
        DefaultValueFactory = _ => [],
        CustomParser = result =>
        {
            var facilities = new SyslogFacility[result.Tokens.Count];

            for (var i = 0; i < result.Tokens.Count; i++)
            {
                var value = result.Tokens[i].Value;

                if (value.Equals("solaris-cron", StringComparison.OrdinalIgnoreCase))
                    value = nameof(SyslogFacility.SolarisCron);

                if (!Enum.TryParse(value, ignoreCase: true, out facilities[i]) ||
                    !Enum.IsDefined(facilities[i]))
                {
                    result.AddError($"Unknown syslog facility '{result.Tokens[i].Value}'.");
                    return [];
                }
            }

            return facilities;
        }
    };

    private static readonly Option<SyslogLevel[]> s_excludeLevelsOption = new("--exclude-levels", "-el")
    {
        Description = "The list of severity levels to exclude (e.g.: 'debug info').",
        Arity = ArgumentArity.ZeroOrMore,
        AllowMultipleArgumentsPerToken = true,
        DefaultValueFactory = _ => []
    };

    /// <summary>
    /// Initializes a new <see cref="SyslogSetCommand"/>.
    /// </summary>
    public SyslogSetCommand()
        : base(CliStrings.Common.SetCommandName, CliStrings.Syslog.SetCommandDescription)
    {
        Add(s_addressOption);
        Add(s_protocolOption);
        Add(s_formatOption);
        Add(s_retrySecondsOption);
        Add(s_excludeFacilitiesOption);
        Add(s_excludeLevelsOption);

        SetAction((parseResult, cancellationToken)
            => SetHandler(
                parseResult.GetRequiredValue(s_addressOption),
                parseResult.GetValue(s_protocolOption),
                parseResult.GetValue(s_formatOption),
                parseResult.GetValue(s_retrySecondsOption),
                parseResult.GetValue(s_excludeFacilitiesOption) ?? [],
                parseResult.GetValue(s_excludeLevelsOption) ?? [],
                cancellationToken));
    }

    private static async Task SetHandler(
        string address,
        SyslogTransportProtocol protocol,
        SyslogLogFormat format,
        uint retrySeconds,
        SyslogFacility[] excludeFacilities,
        SyslogLevel[] excludeLevels,
        CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (retrySeconds > int.MaxValue)
            retrySeconds = int.MaxValue;

        config.Syslog = new()
        {
            Enabled = true,
            Address = address,
            TransportProtocol = protocol,
            LogFormat = format,
            ConnectionRetrySeconds = (int)retrySeconds,
            ExcludeSyslogFacilities = [.. excludeFacilities],
            ExcludeSyslogLevels = [.. excludeLevels]
        };

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Syslog.Set);
    }
}
