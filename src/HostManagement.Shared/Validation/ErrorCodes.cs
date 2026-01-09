namespace HostManagement.Shared.Validation;

/// <summary>
/// Represents <see cref="ErrorCodes"/>.
/// </summary>
public enum ErrorCodes // TODO: Strukturieren der ErrorCodes
{
    /// <summary>
    /// ValidationFailed
    /// </summary>
    ValidationFailed = -1,

    /// <summary>
    /// Valid
    /// </summary>
    Valid = 0,

    // NetworkProxySettings: 1000 - 1100
    //NetworkProxySettingsHttpServer = 1001,
    //NetworkProxySettingsHttpPort = 1002,
    //NetworkProxySettingsHttpUsername = 1003,
    //NetworkProxySettingsHttpPassword = 1004,


    //NetworkProxySettingsHttpsServer = 1011,
    //NetworkProxySettingsHttpsPort = 1012,
    //NetworkProxySettingsHttpsUsername = 1013,
    //NetworkProxySettingsHttpsPassword = 1014,


    //NetworkProxySettingsDoNotProxyListEntryInvalidFormat = 1038,
}
