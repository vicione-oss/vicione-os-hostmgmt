using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the <see cref="ProxyTypeCommandGroupProvider"/>.
/// </summary>
internal static class ProxyTypeCommandGroupProvider
{
    private const string HttpCommandName = "http";
    private const string HttpsCommandName = "https";
    private const string FtpCommandName = "ftp";
    private const string SftpCommandName = "sftp";
    private const string SocksCommandName = "socks";

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
    /// Adds the command group to configure HTTP proxy settings.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddHttpProxyCommand(this Command parentCommand)
        => AddProxyTypeCommand(parentCommand, HttpCommandName);

    /// <summary>
    /// Adds the command group to configure HTTPS proxy settings.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddHttpsProxyCommand(this Command parentCommand)
        => AddProxyTypeCommand(parentCommand, HttpsCommandName);

    /// <summary>
    /// Adds the command group to configure FTP proxy settings.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddFtpProxyCommand(this Command parentCommand)
        => AddProxyTypeCommand(parentCommand, FtpCommandName);

    /// <summary>
    /// Adds the command group to configure SFTP proxy settings.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddSftpProxyCommand(this Command parentCommand)
        => AddProxyTypeCommand(parentCommand, SftpCommandName);

    /// <summary>
    /// Adds the command group to configure SOCKS proxy settings.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddSocksProxyCommand(this Command parentCommand)
        => AddProxyTypeCommand(parentCommand, SocksCommandName);

    private static void AddProxyTypeCommand(this Command parentCommand, string proxyCommandName)
    {
        var proxyCommand = new Command(
            proxyCommandName,
            $"Configure {proxyCommandName.ToUpperInvariant()} proxy settings.");

        proxyCommand.AddOption(s_enableOption);
        proxyCommand.AddOption(s_serverOption);
        proxyCommand.AddOption(s_portOption);
        proxyCommand.AddOption(s_userOption);
        proxyCommand.AddOption(s_passwordOption);

        proxyCommand.AddListCommand(proxyCommandName);
        proxyCommand.SetHandler((ctx) => Handler(ctx, proxyCommandName));

        parentCommand.AddCommand(proxyCommand);
    }

    private static void AddListCommand(this Command parentCommand, string proxyCommandName)
    {
        var listCommand = new Command(
            "list",
            $"List {proxyCommandName.ToUpperInvariant()} proxy settings.");

        listCommand.SetHandler(() => ListHandler(proxyCommandName));

        parentCommand.Add(listCommand);
    }

    private static async Task ListHandler(string proxyCommandName)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var proxyDetail = proxyCommandName switch
        {
            HttpCommandName => config.NetworkProxySettings.HTTP,
            HttpsCommandName => config.NetworkProxySettings.HTTPS,
            FtpCommandName => config.NetworkProxySettings.FTP,
            SftpCommandName => config.NetworkProxySettings.SFTP,
            SocksCommandName => config.NetworkProxySettings.SOCKS,
            _ => throw new InvalidOperationException($"{proxyCommandName} is not a valid proxy name.")
        };

        var builder = new StringBuilder();

        builder.AppendLine($"{proxyCommandName.ToUpperInvariant()} proxy");
        builder.AppendLine($"  Enabled:  {proxyDetail.Enabled}");
        builder.AppendLine($"  Server:   {proxyDetail.Server ?? CommandLineStrings.NotSet}");
        builder.AppendLine($"  Port:     {proxyDetail.Port?.ToString() ?? CommandLineStrings.NotSet}");
        builder.AppendLine($"  Username: {proxyDetail.Username ?? CommandLineStrings.NotSet}");
        builder.AppendLine($"  Password: {proxyDetail.Password ?? CommandLineStrings.NotSet}");

        Console.WriteLine(builder.ToString());
    }

    private static async Task Handler(InvocationContext ctx, string proxyCommandName)
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

        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var networkProxyDetail = proxyCommandName switch
        {
            HttpCommandName => config.NetworkProxySettings.HTTP,
            HttpsCommandName => config.NetworkProxySettings.HTTPS,
            FtpCommandName => config.NetworkProxySettings.FTP,
            SftpCommandName => config.NetworkProxySettings.SFTP,
            SocksCommandName => config.NetworkProxySettings.SOCKS,
            _ => throw new InvalidOperationException($"{proxyCommandName} is not a valid proxy name.")
        };

        networkProxyDetail.Enabled = enableOptionValue;

        if (!useLastAppliedProxy)
        {
            networkProxyDetail.Server = serverOptionValue;
            networkProxyDetail.Port = portOptionValue;
            networkProxyDetail.Username = userOptionValue;
            networkProxyDetail.Password = passwordOptionValue;
        }

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            $"{proxyCommandName.ToUpperInvariant()} proxy has been configured.");
    }
}
