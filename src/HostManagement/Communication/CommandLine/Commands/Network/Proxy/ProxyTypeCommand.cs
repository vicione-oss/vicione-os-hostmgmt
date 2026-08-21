using System.CommandLine;
using System.Globalization;
using System.Text;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the proxy type command.
/// </summary>
internal sealed class ProxyTypeCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="ProxyTypeCommand"/>.
    /// </summary>
    /// <param name="proxyType">The type of the proxy.</param>
    public ProxyTypeCommand(ProxyType proxyType)
        : base(
            proxyType.ToString().ToLowerInvariant(),
            string.Format(
                CultureInfo.InvariantCulture,
                CompositeFormat.Parse(CliStrings.Network.Proxy.TypeCommandDescription),
                proxyType))
    {
        Add(new ProxyTypeStatusCommand(proxyType));
        Add(new ProxyTypeEnableCommand(proxyType));
        Add(new ProxyTypeDisableCommand(proxyType));
        Add(new ProxyTypeSetCommand(proxyType));
    }
}
