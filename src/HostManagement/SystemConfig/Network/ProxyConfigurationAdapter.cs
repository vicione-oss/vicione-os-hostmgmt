using System.Text;
using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using HostManagement.Scripts.ScriptOptionNames;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.SystemConfig.Network;

/// <summary>
/// Represents the proxy configuration adapter.
/// </summary>
/// <param name="current">The current system configuration.</param>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
/// <param name="bypassHasChanges">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassHasChanges']"/>
/// </param>
/// <param name="bypassApplyConfig">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassApplyConfig']"/>
/// </param>
internal sealed class ProxyConfigurationAdapter(
    SystemConfiguration current,
    IScriptHandler scriptHandler,
    bool bypassHasChanges,
    bool bypassApplyConfig)
        : ConfigurationAdapter<SystemConfigurationAdapter>(bypassHasChanges, bypassApplyConfig)
{
    private readonly SystemConfiguration _current = current;
    private readonly IScriptHandler _scriptHandler = scriptHandler;

    protected internal override async Task<OperationStatus> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var (status, networkProxySettings) = await _scriptHandler
            .ExecuteGetNetworkProxySettingsScript()
            .ConfigureAwait(false);

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        /*
         * If a zone identifier was specified while setting a no-proxy item, we had to replace it
         * with the complete interface name, e.g. 'lan1@ens33' instead of 'lan1'.
         * 
         * Now we need to reverse this, because virtual interfaces are shown to the user
         * without the physical interface.
         */
        networkProxySettings = networkProxySettings with
        {
            DoNotProxyList = networkProxySettings.DoNotProxyList
                .Select(item => new string([.. item.TakeWhile(character => character != '@')]))
                .ToList()
        };

        config.NetworkProxySettings = networkProxySettings;

        return OperationStatus.Success;
    }

    private protected override bool HasChangesInternal(
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig)
    {
        return !currentConfig.NetworkProxySettings.Equals(proposedConfig.NetworkProxySettings);
    }

    private protected override async Task<OperationStatus> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        ScriptOptions scriptOptions;

        var proxySettings = config.NetworkProxySettings;
        var currentProxySettings = _current.NetworkProxySettings;

        if (!proxySettings.HTTP.Equals(currentProxySettings.HTTP) &&
            !proxySettings.HTTP.Equals(NetworkProxyDetail.Empty))
        {
            scriptOptions = GetProxyScriptOptions(proxySettings.HTTP);

            var status = await _scriptHandler
                .ExecuteSetNetworkProxySettingsScript(scriptOptions)
                .ConfigureAwait(false);

            if (status == OperationStatus.Error)
                return OperationStatus.Error;
        }

        if (!proxySettings.HTTPS.Equals(currentProxySettings.HTTPS) &&
            !proxySettings.HTTPS.Equals(NetworkProxyDetail.Empty))
        {
            scriptOptions = GetProxyScriptOptions(proxySettings.HTTPS);

            var status = await _scriptHandler
                .ExecuteSetNetworkProxySettingsScript(scriptOptions)
                .ConfigureAwait(false);

            if (status == OperationStatus.Error)
                return OperationStatus.Error;
        }

        if (!proxySettings.FTP.Equals(currentProxySettings.FTP) &&
            !proxySettings.FTP.Equals(NetworkProxyDetail.Empty))
        {
            scriptOptions = GetProxyScriptOptions(proxySettings.FTP);

            var status = await _scriptHandler
                .ExecuteSetNetworkProxySettingsScript(scriptOptions)
                .ConfigureAwait(false);

            if (status == OperationStatus.Error)
                return OperationStatus.Error;
        }

        if (!proxySettings.SFTP.Equals(currentProxySettings.SFTP) &&
            !proxySettings.SFTP.Equals(NetworkProxyDetail.Empty))
        {
            scriptOptions = GetProxyScriptOptions(proxySettings.SFTP);

            var status = await _scriptHandler
                .ExecuteSetNetworkProxySettingsScript(scriptOptions)
                .ConfigureAwait(false);

            if (status == OperationStatus.Error)
                return OperationStatus.Error;
        }

        if (!proxySettings.SOCKS.Equals(currentProxySettings.SOCKS) &&
            !proxySettings.SOCKS.Equals(NetworkProxyDetail.Empty))
        {
            scriptOptions = GetProxyScriptOptions(proxySettings.SOCKS);

            var status = await _scriptHandler
                .ExecuteSetNetworkProxySettingsScript(scriptOptions)
                .ConfigureAwait(false);

            if (status == OperationStatus.Error)
                return OperationStatus.Error;
        }

        if (!proxySettings.DoNotProxyList.Equals(currentProxySettings.DoNotProxyList) ||
            !proxySettings.DoNotProxyListEnabled.Equals(currentProxySettings.DoNotProxyListEnabled))
        {
            scriptOptions = await GetNoProxyListScriptOptions(
                proxySettings.DoNotProxyList,
                proxySettings.DoNotProxyListEnabled);

            var status = await _scriptHandler
                .ExecuteSetNetworkProxySettingsScript(scriptOptions)
                .ConfigureAwait(false);

            if (status == OperationStatus.Error)
                return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    private static ScriptOptions GetProxyScriptOptions(NetworkProxyDetail proxyDetail)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            ProxyScriptOptionName.Enabled,
            proxyDetail.Enabled);

        if (proxyDetail.Server is null)
            throw new InvalidOperationException("Proxy server can not be null.");

        scriptOptions.Add(
            ProxyScriptOptionName.URL,
            proxyDetail.Server);

        if (proxyDetail.Port is null)
            throw new InvalidOperationException("Proxy port can not be null.");

        scriptOptions.Add(
            ProxyScriptOptionName.Port,
            (int)proxyDetail.Port);

        if (proxyDetail.Username is not null &&
            proxyDetail.Password is not null)
        {
            scriptOptions.Add(
                ProxyScriptOptionName.Username,
                proxyDetail.Username);

            scriptOptions.Add(
                ProxyScriptOptionName.Password,
                proxyDetail.Password);
        }

        return scriptOptions;
    }

    private static async Task<ScriptOptions> GetNoProxyListScriptOptions(
        List<string> noProxyList,
        bool noProxyListEnabled)
    {
        var scriptOptions = new ScriptOptions();

        var noProxyListString = noProxyList.Count > 0
            ? await GetNoProxyListString(noProxyList)
            : "NONE";

        scriptOptions.Add(
            ProxyScriptOptionName.NoProxyList,
            noProxyListString);

        scriptOptions.Add(
            ProxyScriptOptionName.NoProxyListEnabled,
            noProxyListEnabled);

        return scriptOptions;
    }

    private static async Task<string> GetNoProxyListString(List<string> noProxyList)
    {
        // No no-proxy items with zone identifier specified
        if (noProxyList.Any(item => !item.Contains('%')))
            return string.Join(',', noProxyList);

        /*
         * Zone identifier found (link-local IPv6 address was specified)
         * -> we must use the complete interface name
         * 
         * a) for virtual interface use e.g. 'lan1@ens33' instead of 'lan1'
         * b) for physical interface nothing to do
        */
        var virtualInterfaceNames = await GetAllVirtualInterfaceNames().ConfigureAwait(false);

        var noProxyListStringBuilder = new StringBuilder();

        for (var i = 0; i < noProxyList.Count; i++)
        {
            var noProxyItem = noProxyList[i].AsSpan();

            if (!HasItemZoneIdentifier(noProxyItem))
            {
                noProxyListStringBuilder.Append($"{noProxyItem},");
                continue;
            }

            /*
             * Zone identifier
             * 
             * a) with a physical interface -> use as specified,
             *    e.g. 'ens42' in 'fe80::1%ens42' or '[fe80::1%ens42]:123'
             * b) with a virtual interface -> use complete name,
             *    e.g. 'lan1@ens35' in 'fe80::1%lan1@ens35' or '[fe80::1%lan1@ens35]:123'
            */

            var indexOfZoneIdSeparator = noProxyItem.IndexOf('%');
            var left = noProxyItem[..indexOfZoneIdSeparator];
            var right = noProxyItem[(indexOfZoneIdSeparator + 1)..];

            var hasItemPort = HasItemPort(noProxyItem);

            var ipAddress = hasItemPort ? left[1..] : left;
            var zoneIdentifier = hasItemPort ? right[..noProxyItem.IndexOf(']')] : right;

            zoneIdentifier = GetCompleteZoneIdentifier(virtualInterfaceNames, zoneIdentifier);

            var port = hasItemPort ? right[(noProxyItem.IndexOf("]:") + 1)..] : string.Empty;

            if (hasItemPort)
                noProxyListStringBuilder.Append($"[{ipAddress}%{zoneIdentifier}]:{port},");
            else
                noProxyListStringBuilder.Append($"{ipAddress}%{zoneIdentifier},");
        }

        return noProxyListStringBuilder.ToString().TrimEnd(',');
    }

    private static async Task<List<string>> GetAllVirtualInterfaceNames()
    {
        var scriptResponse = await ScriptRunner
            .Run(
                ScriptIdentifier.NetworkInterfacesGet,
                ["virtual"])
            .ConfigureAwait(false);

        if (scriptResponse.HasErrors)
            throw new InvalidOperationException($"{scriptResponse.StandardError}");

        var virtualInterfaceNames = new List<string>();
        var interfaces = scriptResponse.StandardOutput.Split("\n");

        foreach (var item in interfaces)
            virtualInterfaceNames.Add(item.Split(' ')[0]);

        return virtualInterfaceNames;
    }

    private static bool HasItemZoneIdentifier(ReadOnlySpan<char> noProxyItem)
        => noProxyItem.Contains('%');

    private static bool HasItemPort(ReadOnlySpan<char> noProxyItem)
        => noProxyItem.StartsWith('[');

    private static ReadOnlySpan<char> GetCompleteZoneIdentifier(
        List<string> virtualInterfaceNames,
        ReadOnlySpan<char> zoneIdentifier)
    {
        foreach (var completeInterfaceName in virtualInterfaceNames)
        {
            if (completeInterfaceName.AsSpan().StartsWith(zoneIdentifier, StringComparison.Ordinal) &&
                completeInterfaceName.Contains('@'))
            {
                zoneIdentifier = completeInterfaceName;
                break;
            }
        }

        return zoneIdentifier;
    }
}
