using System.Globalization;
using System.Text;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the proxy type list command.
/// </summary>
internal sealed class ProxyTypeListCommand : ProxyTypeCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="ProxyTypeListCommand"/>.
    /// </summary>
    /// <param name="proxyType">The proxy type.</param>
    public ProxyTypeListCommand(ProxyType proxyType) : base(
        CommandLineStrings_Common.ListCommandName,
        string.Format(
            CultureInfo.InvariantCulture,
            CompositeFormat.Parse(CommandLineStrings_Proxy.ProxyTypeListCommandDescription),
            proxyType))
                => SetAction((parseResult, cancellationToken) => ListHandler(proxyType, cancellationToken));

    private static async Task ListHandler(ProxyType proxyType, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
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
}
