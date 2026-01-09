using HostManagement.Scripts.ScriptOptionNames;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.SystemConfig.Network;

/// <summary>
/// Represents the proxy configuration adapter.
/// </summary>
internal sealed class ProxyConfigurationAdapter : ConfigurationAdapter<SystemConfigurationAdapter>
{
    private readonly SystemConfiguration _current;
    private readonly IScriptHandler _scriptHandler;

    /// <summary>
    /// Initializes a new <see cref="ProxyConfigurationAdapter"/>.
    /// </summary>
    /// <param name="current">The current system configuration.</param>
    /// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
    /// <param name="bypassHasChangesInternal">If set to <see langword="true"/> bypasses the
    /// internal check for changes and returns <see langword="true"/> when
    /// <see cref="ConfigurationAdapter.HasChanges(SystemConfiguration, SystemConfiguration)"/>
    /// is called.</param>
    public ProxyConfigurationAdapter(
        SystemConfiguration current,
        IScriptHandler scriptHandler,
        bool bypassHasChangesInternal = false)
        : base(bypassHasChangesInternal)
    {
        _current = current;
        _scriptHandler = scriptHandler;
    }

    private protected override bool HasChangesInternal(SystemConfiguration currentConfig, SystemConfiguration proposedConfig)
        => !currentConfig.NetworkProxySettings.Equals(proposedConfig.NetworkProxySettings);

    protected internal override async Task<OperationStatus> LoadConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var (status, networkProxySettings) = await _scriptHandler.ExecuteGetNetworkProxySettingsScript();

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        config.NetworkProxySettings = networkProxySettings;

        return OperationStatus.Success;
    }

    protected internal override async Task<OperationStatus> ApplyConfiguration(
        SystemConfiguration proposed,
        CancellationToken cancellationToken)
    {
        ScriptOptions scriptOptions;

        if (!proposed.NetworkProxySettings.HTTP.Equals(_current.NetworkProxySettings.HTTP))
        {
            scriptOptions = GetProxyScriptOptions(proposed.NetworkProxySettings.HTTP);

            if (await _scriptHandler.ExecuteSetNetworkProxySettingsScript(scriptOptions) == OperationStatus.Error)
                return OperationStatus.Error;
        }

        if (!proposed.NetworkProxySettings.HTTPS.Equals(_current.NetworkProxySettings.HTTPS))
        {
            scriptOptions = GetProxyScriptOptions(proposed.NetworkProxySettings.HTTPS);

            if (await _scriptHandler.ExecuteSetNetworkProxySettingsScript(scriptOptions) == OperationStatus.Error)
                return OperationStatus.Error;
        }

        if (!proposed.NetworkProxySettings.FTP.Equals(_current.NetworkProxySettings.FTP))
        {
            scriptOptions = GetProxyScriptOptions(proposed.NetworkProxySettings.FTP);

            if (await _scriptHandler.ExecuteSetNetworkProxySettingsScript(scriptOptions) == OperationStatus.Error)
                return OperationStatus.Error;
        }

        if (!proposed.NetworkProxySettings.SFTP.Equals(_current.NetworkProxySettings.SFTP))
        {
            scriptOptions = GetProxyScriptOptions(proposed.NetworkProxySettings.SFTP);

            if (await _scriptHandler.ExecuteSetNetworkProxySettingsScript(scriptOptions) == OperationStatus.Error)
                return OperationStatus.Error;
        }

        if (!proposed.NetworkProxySettings.SOCKS.Equals(_current.NetworkProxySettings.SOCKS))
        {
            scriptOptions = GetProxyScriptOptions(proposed.NetworkProxySettings.SOCKS);

            if (await _scriptHandler.ExecuteSetNetworkProxySettingsScript(scriptOptions) == OperationStatus.Error)
                return OperationStatus.Error;
        }

        if (!proposed.NetworkProxySettings.DoNotProxyList.Equals(_current.NetworkProxySettings.DoNotProxyList) ||
            !proposed.NetworkProxySettings.DoNotProxyListEnabled.Equals(_current.NetworkProxySettings.DoNotProxyListEnabled))
        {
            scriptOptions = GetNoProxyListScriptOptions(
                proposed.NetworkProxySettings.DoNotProxyList,
                proposed.NetworkProxySettings.DoNotProxyListEnabled);

            if (await _scriptHandler.ExecuteSetNetworkProxySettingsScript(scriptOptions) == OperationStatus.Error)
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

        scriptOptions.Add(
            ProxyScriptOptionName.URL,
            proxyDetail.Server!);

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

    private static ScriptOptions GetNoProxyListScriptOptions(
        List<string> noProxyList,
        bool noProxyListEnabled)
    {
        var scriptOptions = new ScriptOptions();

        var noProxyListString = noProxyList.Count > 0
            ? string.Join(',', noProxyList)
            : "NONE";

        scriptOptions.Add(
            ProxyScriptOptionName.NoProxyList,
            noProxyListString);

        scriptOptions.Add(
            ProxyScriptOptionName.NoProxyListEnabled,
            noProxyListEnabled);

        return scriptOptions;
    }
}
