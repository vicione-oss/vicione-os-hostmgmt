using System.Globalization;
using System.Text;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the proxy type disable command.
/// </summary>
internal sealed class ProxyTypeDisableCommand : ProxyTypeCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="ProxyTypeDisableCommand"/>.
    /// </summary>
    /// <param name="proxyType">The type of the proxy.</param>
    public ProxyTypeDisableCommand(ProxyType proxyType)
        : base(
            CliStrings.Common.DisableCommandName,
            string.Format(
                CultureInfo.InvariantCulture,
                CompositeFormat.Parse(CommandLineStrings_Proxy.ProxyTypeDisableCommandDescription),
                proxyType))
        => SetAction((parseResult, cancellationToken) => DisableHandler(proxyType, cancellationToken));

    private static async Task DisableHandler(ProxyType proxyType, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var proxyDetail = GetNetworkProxyDetail(proxyType, config);

        if (!proxyDetail.Enabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_Proxy.ProxyTypeAlreadyDisabled, proxyType.ToString());
            return;
        }

        proxyDetail.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_Proxy.ProxyTypeDisabled, proxyType.ToString());
    }
}
