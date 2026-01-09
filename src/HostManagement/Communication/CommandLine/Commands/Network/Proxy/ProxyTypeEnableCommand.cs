using System.Globalization;
using System.Text;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the proxy type enable command.
/// </summary>
internal sealed class ProxyTypeEnableCommand : ProxyTypeCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="ProxyTypeEnableCommand"/>.
    /// </summary>
    /// <param name="proxyType">The type of the proxy.</param>
    public ProxyTypeEnableCommand(ProxyType proxyType) : base(
        CommandLineStrings_Common.EnableCommandName,
        string.Format(
            CultureInfo.InvariantCulture,
            CompositeFormat.Parse(CommandLineStrings_Proxy.ProxyTypeEnableCommandDescription),
            proxyType))
    {
        SetAction((parseResult, cancellationToken) => EnableHandler(proxyType, cancellationToken));
    }

    private static async Task EnableHandler(ProxyType proxyType, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var proxyDetail = GetNetworkProxyDetail(proxyType, config);

        if (proxyDetail.Enabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_Proxy.ProxyTypeAlreadyEnabled, proxyType.ToString());
            return;
        }

        if (proxyDetail.Server is null &&
            proxyDetail.Port is null &&
            proxyDetail.Username is null &&
            proxyDetail.Password is null)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_Proxy.ProxyTypeNotConfigured, proxyType.ToString());
            return;
        }

        proxyDetail.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_Proxy.ProxyTypeEnabled, proxyType.ToString());
    }
}
