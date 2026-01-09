using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using System.Text;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the abstract base class for a proxy command.
/// </summary>
internal abstract class ProxyTypeCommandBase : ConfigCommandBase
{
    internal static readonly Option<bool> s_enableOption = new(["-e", "--enabled"])
    {
        Description = "Enable or disable the proxy.",
        IsRequired = true,
        Arity = ArgumentArity.ZeroOrOne
    };

    internal static readonly Option<string?> s_serverOption = new(["-s", "--server"])
    {
        Description = "The proxy server address.",
        Arity = ArgumentArity.ExactlyOne
    };

    internal static readonly Option<int?> s_portOption = new(["-p", "--port"])
    {
        Description = "The proxy server port.",
        Arity = ArgumentArity.ExactlyOne
    };

    internal static readonly Option<string?> s_userOption = new(["-u", "--user"])
    {
        Description = "The username for authentication.",
        Arity = ArgumentArity.ExactlyOne
    };

    internal static readonly Option<string?> s_passwordOption = new(["-pw", "--password"])
    {
        Description = "The password for authentication.",
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="ProxyTypeCommandBase"/>.
    /// </summary>
    public ProxyTypeCommandBase(ProxyType proxyType) : base(
        proxyType.ToString().ToLowerInvariant(),
        string.Format(
            CultureInfo.InvariantCulture,
            CompositeFormat.Parse(CommandLineStrings_Proxy.ProxyTypeCommandDescription),
            proxyType))
    {
        AddOption(s_enableOption);
        AddOption(s_serverOption);
        AddOption(s_portOption);
        AddOption(s_userOption);
        AddOption(s_passwordOption);

        Add(
            CommandLineStrings_Common.ListCommandName,
            string.Format(
                CultureInfo.InvariantCulture,
                CompositeFormat.Parse(CommandLineStrings_Proxy.ProxyTypeListCommandDescription),
                proxyType),
            () => ListHandler(proxyType));

        this.SetHandler((ctx) => ProxyHandler(ctx, proxyType));
    }

    private static async Task ListHandler(ProxyType proxyType)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var proxyDetail = GetNetworkProxyDetail(proxyType, config);

        var password = proxyDetail.Password is not null
            ? MaskPasswordPartial(proxyDetail.Password)
            : proxyDetail.Password;

        new KeyValueOutput()
            .WithTitle($"{proxyType} proxy")
            .With(new("Enabled", proxyDetail.Enabled))
            .With(new("Server", proxyDetail.Server))
            .With(new("Port", proxyDetail.Port))
            .With(new("Username", proxyDetail.Username))
            .With(new("Password", password))
            .WriteLine();


        static string MaskPasswordPartial(string password)
        {
            if (password.Length == 1)
                return "*";

            if (password.Length == 2)
                return "**";

            return password[..1] + new string('*', password.Length - 2) + password[^1];
        }
    }

    private static async Task ProxyHandler(InvocationContext ctx, ProxyType proxyType)
    {
        var useLastAppliedProxy = false;

        var enableOptionValue = ctx.ParseResult.GetValueForOption(s_enableOption);
        var serverOptionValue = ctx.ParseResult.GetValueForOption(s_serverOption);
        var portOptionValue = ctx.ParseResult.GetValueForOption(s_portOption);
        var userOptionValue = ctx.ParseResult.GetValueForOption(s_userOption);
        var passwordOptionValue = ctx.ParseResult.GetValueForOption(s_passwordOption);

        if (serverOptionValue is null &&
            portOptionValue is null &&
            userOptionValue is null &&
            passwordOptionValue is null)
        {
            useLastAppliedProxy = true;
        }

        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var proxyDetail = GetNetworkProxyDetail(proxyType, config);

        proxyDetail.Enabled = enableOptionValue;

        if (!useLastAppliedProxy)
        {
            proxyDetail.Server = serverOptionValue;
            proxyDetail.Port = portOptionValue;
            proxyDetail.Username = userOptionValue;
            proxyDetail.Password = passwordOptionValue;
        }

        if (!await SetConfiguration(config))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_Proxy.ProxyConfigured, proxyType.ToString());
    }

    private static NetworkProxyDetail GetNetworkProxyDetail(
        ProxyType proxyType,
        SystemConfiguration config)
    {
        return proxyType switch
        {
            ProxyType.HTTP => config.NetworkProxySettings.HTTP,
            ProxyType.HTTPS => config.NetworkProxySettings.HTTPS,
            ProxyType.FTP => config.NetworkProxySettings.FTP,
            ProxyType.SFTP => config.NetworkProxySettings.SFTP,
            ProxyType.SOCKS => config.NetworkProxySettings.SOCKS,
            _ => throw new InvalidOperationException($"{proxyType} is not a valid proxy type.")
        };
    }
}
