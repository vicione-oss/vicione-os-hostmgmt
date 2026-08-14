using System.Globalization;
using System.Text;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    public ProxyTypeEnableCommand(ProxyType proxyType)
        : base(
            CliStrings.Common.EnableCommandName,
            string.Format(
                CultureInfo.InvariantCulture,
                CompositeFormat.Parse(CliStrings.Network.Proxy.EnableCommandDescription),
                proxyType))
        => SetAction((parseResult, cancellationToken) => EnableHandler(proxyType, cancellationToken));

    private static async Task EnableHandler(ProxyType proxyType, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var proxyDetail = GetNetworkProxyDetail(proxyType, config);

        if (proxyDetail.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.Proxy.AlreadyEnabled, proxyType.ToString());
            return;
        }

        if (proxyDetail.Server is null &&
            proxyDetail.Port is null &&
            proxyDetail.Username is null &&
            proxyDetail.Password is null)
        {
            SimpleOutput.WriteLineError(CliStrings.Network.Proxy.NotConfigured, proxyType.ToString());
            return;
        }

        proxyDetail.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.Proxy.Enabled, proxyType.ToString());
    }
}
