using System.CommandLine;
using System.Globalization;
using System.Text;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the proxy type set command.
/// </summary>
internal sealed class ProxyTypeSetCommand : ProxyTypeCommandBase
{
    private static readonly Option<string> s_serverOption = new("--server", "-s")
    {
        Description = "The proxy server address.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Option<int> s_portOption = new("--port", "-p")
    {
        Description = "The proxy server port.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Option<string?> s_userOption = new("--user", "-u")
    {
        Description = "The username for authentication.",
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Option<string?> s_passwordOption = new("--password", "-pw")
    {
        Description = "The password for authentication.",
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="ProxyTypeSetCommand"/>.
    /// </summary>
    /// <param name="proxyType">The type of the proxy.</param>
    public ProxyTypeSetCommand(ProxyType proxyType)
        : base(
            CommandLineStrings_Common.SetCommandName,
            string.Format(
                CultureInfo.InvariantCulture,
                CompositeFormat.Parse(CommandLineStrings_Proxy.ProxyTypeSetCommandDescription),
                proxyType))
    {
        Add(s_serverOption);
        Add(s_portOption);
        Add(s_userOption);
        Add(s_passwordOption);

        SetAction((parseResult, cancellationToken)
            => SetHandler(
                proxyType,
                parseResult.GetRequiredValue(s_serverOption),
                parseResult.GetRequiredValue(s_portOption),
                parseResult.GetValue(s_userOption),
                parseResult.GetValue(s_passwordOption),
                cancellationToken));
    }

    private static async Task SetHandler(
        ProxyType proxyType,
        string server,
        int port,
        string? user,
        string? password,
        CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var proxyDetail = GetNetworkProxyDetail(proxyType, config);

        proxyDetail.Server = server;
        proxyDetail.Port = port;
        proxyDetail.Username = user;
        proxyDetail.Password = password;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_Proxy.ProxyTypeConfigured, proxyType.ToString());
    }
}
