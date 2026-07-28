using System.Text;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;

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

    protected internal override async Task<Result> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var (result, networkProxySettings) = await _scriptHandler
            .ExecuteGetNetworkProxySettingsScript()
            .ConfigureAwait(false);

        if (result.OperationStatus is OperationStatus.Error)
            return result;

        // In /etc/environment we had to percent-encode special characters in username or password
        // for applications like apt, wget, curl etc. Now we need to reverse this.
        UnescapeNetworkProxySettingsAuthentication(networkProxySettings);

        config.NetworkProxySettings = networkProxySettings;

        if (networkProxySettings.DoNotProxyList is not null)
        {
            var modifiedNoProxyList = new List<string>();

            foreach (var item in networkProxySettings.DoNotProxyList)
            {
                var modifiedItem = item;

                if (modifiedItem.Contains("%25"))
                {
                    /*
                     * In URIs or /etc/environment we had to percent-encode '%' as '%25'
                     * to satisfy RFC 6874.
                     * 
                     * Now we need to reverse this, and remember that an interface could be
                     * named "25". So the zone id marker would have been escaped to "%2525".
                     */
                    if (modifiedItem.Contains("%2525"))
                        modifiedItem = modifiedItem.Replace("%2525", "%25");
                    else
                        modifiedItem = modifiedItem.Replace("%25", "%");
                }

                modifiedNoProxyList.Add(modifiedItem);
            }

            config.NetworkProxySettings.DoNotProxyList = modifiedNoProxyList;
        }

        return result;


        static void UnescapeNetworkProxySettingsAuthentication(NetworkProxySettings networkProxySettings)
        {
            UnescapeNetworkProxyDetailAuthentication(networkProxySettings.HTTP);
            UnescapeNetworkProxyDetailAuthentication(networkProxySettings.HTTPS);
            UnescapeNetworkProxyDetailAuthentication(networkProxySettings.FTP);
            UnescapeNetworkProxyDetailAuthentication(networkProxySettings.SFTP);
            UnescapeNetworkProxyDetailAuthentication(networkProxySettings.SOCKS);
        }

        static void UnescapeNetworkProxyDetailAuthentication(NetworkProxyDetail? networkProxyDetail)
        {
            if (networkProxyDetail is not null &&
                networkProxyDetail.Username is not null &&
                networkProxyDetail.Password is not null)
            {
                networkProxyDetail.Username = Uri.UnescapeDataString(networkProxyDetail.Username);
                networkProxyDetail.Password = Uri.UnescapeDataString(networkProxyDetail.Password);
            }
        }
    }

    private protected override bool HasChangesInternal(
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig)
    {
        return SystemConfigurationChangeDetector.HasChanges(
            currentConfig.NetworkProxySettings,
            proposedConfig.NetworkProxySettings);
    }

    private protected override async Task<Result> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var proposedProxySettings = config.NetworkProxySettings;
        var currentProxySettings = _current.NetworkProxySettings;

        if (proposedProxySettings is null)
            return Result.SuccessResult;

        var proxyDetails = new (NetworkProxyDetail? Proposed, NetworkProxyDetail? Current)[]
        {
            (proposedProxySettings.HTTP, currentProxySettings?.HTTP),
            (proposedProxySettings.HTTPS, currentProxySettings?.HTTPS),
            (proposedProxySettings.FTP, currentProxySettings?.FTP),
            (proposedProxySettings.SFTP, currentProxySettings?.SFTP),
            (proposedProxySettings.SOCKS, currentProxySettings?.SOCKS)
        };

        var combinedResult = Result.SuccessResult;

        foreach (var (proposed, current) in proxyDetails)
        {
            combinedResult.AddResult(await SetProxyDetail(proposed, current).ConfigureAwait(false));

            if (combinedResult.OperationStatus is OperationStatus.Error)
                return combinedResult;
        }

        return combinedResult.AddResult(
            await SetDoNotProxyList(
                proposedProxySettings.DoNotProxyListEnabled,
                proposedProxySettings.DoNotProxyList,
                currentProxySettings?.DoNotProxyListEnabled,
                currentProxySettings?.DoNotProxyList)
                .ConfigureAwait(false));
    }

    private async Task<Result> SetProxyDetail(
        NetworkProxyDetail? proposedProxyDetail,
        NetworkProxyDetail? currentProxyDetail)
    {
        if (proposedProxyDetail is not null &&
            !proposedProxyDetail.Equals(new NetworkProxyDetail()) &&
            !EqualityComparer<NetworkProxyDetail>.Default.Equals(proposedProxyDetail, currentProxyDetail))
        {
            return await _scriptHandler
                .ExecuteSetNetworkProxySettingsScript(GetProxyScriptOptions(proposedProxyDetail))
                .ConfigureAwait(false);
        }

        return Result.SuccessResult;
    }

    private static ScriptOptions GetProxyScriptOptions(NetworkProxyDetail proxyDetail)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(ScriptOptionNames.Proxy.Enabled, proxyDetail.Enabled);

        // Make the compiler happy. A validator prevents us from getting here with invalid values for server and port.
        scriptOptions.Add(ScriptOptionNames.Proxy.URL, proxyDetail.Server ?? string.Empty);
        scriptOptions.Add(ScriptOptionNames.Proxy.Port, proxyDetail.Port ?? -1);

        if (proxyDetail.Username is not null &&
            proxyDetail.Password is not null)
        {
            /*
             * We need to percent-encode special characters in /etc/environment
             * for applications like apt, wget, curl etc. to parse correctly.

             * Characters that are not escaped by Uri.EscapeDataString are the unreserved set (RFC 3986 §2.3):
             * A–Z, a–z, 0–9, "-", "_", ".", "~"
             */
            scriptOptions.Add(ScriptOptionNames.Proxy.Username, Uri.EscapeDataString(proxyDetail.Username));
            scriptOptions.Add(ScriptOptionNames.Proxy.Password, Uri.EscapeDataString(proxyDetail.Password));
        }

        return scriptOptions;
    }

    private async Task<Result> SetDoNotProxyList(
        bool? proposedListEnabled,
        List<string>? proposedList,
        bool? currentListEnabled,
        List<string>? currentList)
    {
        if (proposedListEnabled.HasValue &&
            proposedList is not null &&
            (
                !EqualityComparer<string>.Default.SequenceEqual(proposedList, currentList) ||
                !EqualityComparer<bool?>.Default.Equals(proposedListEnabled, currentListEnabled)
            ))
        {
            var scriptOptions = GetNoProxyListScriptOptions(proposedList, proposedListEnabled.Value);

            var result = await _scriptHandler
                .ExecuteSetNetworkProxySettingsScript(scriptOptions)
                .ConfigureAwait(false);

            if (result.OperationStatus is OperationStatus.Error)
                return result;
        }

        return Result.SuccessResult;
    }

    private static ScriptOptions GetNoProxyListScriptOptions(List<string> noProxyList, bool noProxyListEnabled)
    {
        var noProxyListString = noProxyList.Count > 0
            ? GetNoProxyListString(noProxyList)
            : "NONE";

        var scriptOptions = new ScriptOptions();
        scriptOptions.Add(ScriptOptionNames.Proxy.NoProxyList, noProxyListString);
        scriptOptions.Add(ScriptOptionNames.Proxy.NoProxyListEnabled, noProxyListEnabled);

        return scriptOptions;
    }

    private static string GetNoProxyListString(List<string> noProxyList)
    {
        // No no-proxy items with zone identifier specified
        if (!noProxyList.Any(item => item.Contains('%')))
            return string.Join(',', noProxyList);

        var noProxyListStringBuilder = new StringBuilder();

        for (var i = 0; i < noProxyList.Count; i++)
        {
            var noProxyItem = noProxyList[i].AsSpan();

            if (!HasItemZoneIdentifier(noProxyItem))
            {
                noProxyListStringBuilder.Append($"{noProxyItem},");
                continue;
            }

            // Zone identifier found (link-local IPv6 address was specified)
            var indexOfZoneIdSeparator = noProxyItem.IndexOf('%');
            var left = noProxyItem[..indexOfZoneIdSeparator];
            var right = noProxyItem[(indexOfZoneIdSeparator + 1)..];

            var hasItemPort = HasItemPort(noProxyItem);

            var ipAddress = hasItemPort ? left[1..] : left;
            var zoneIdentifier = hasItemPort ? right[..right.IndexOf(']')] : right;

            var port = hasItemPort ? right[(right.IndexOf("]:") + 2)..] : string.Empty;

            // In URIs or /etc/environment we must percent-encode '%' as '%25' to satisfy RFC 6874
            if (hasItemPort)
                noProxyListStringBuilder.Append($"[{ipAddress}%25{zoneIdentifier}]:{port},");
            else
                noProxyListStringBuilder.Append($"{ipAddress}%25{zoneIdentifier},");
        }

        return noProxyListStringBuilder.ToString().TrimEnd(',');
    }

    private static bool HasItemZoneIdentifier(ReadOnlySpan<char> noProxyItem)
        => noProxyItem.Contains('%');

    private static bool HasItemPort(ReadOnlySpan<char> noProxyItem)
        => noProxyItem.StartsWith('[');
}
