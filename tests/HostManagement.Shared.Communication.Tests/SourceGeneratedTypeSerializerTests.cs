using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using AwesomeAssertions;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Xunit;
using SourceGenerationContext = HostManagement.Shared.Communication.Contracts.SourceGenerationContext;

namespace HostManagement.Shared.Communication.Tests;

public class SourceGeneratedTypeSerializerTests
{
    private const string IndentedSampleDHCPLeaseJson =
        """
        {
          "LeaseObtained": "2025-06-23T12:34:56+02:00",
          "LeaseExpires": "2025-06-30T12:34:56+02:00",
          "Gateway": "120.86.52.18",
          "NetworkDNSSettings": {
            "Hostname": "hostname",
            "MulticastDNSEnabled": true,
            "StaticHostsEnabled": true,
            "StaticHosts": [
              {
                "IPAddress": "120.86.52.18",
                "Hostname": "statichost"
              }
            ],
            "NameServersEnabled": true,
            "NameServers": [
              "120.86.52.18"
            ],
            "DNSSuffixEnabled": true,
            "DNSSuffix": "dnsSuffix",
            "SearchDomainsEnabled": true,
            "SearchDomains": [
              "searchDomain"
            ]
          },
          "IPv4Detail": {
            "IPAddress": "120.86.52.18",
            "Netmask": "120.86.52.18"
          }
        }
        """;

    private const string IndentedSampleNetworkStatusJson =
        """
        {
          "IsDefaultRouteConfigured": true,
          "IsDefaultGatewayAvailable": true,
          "IsInternetAvailable": false,
          "IsDNSFunctional": true,
          "InterfaceState": "UP"
        }
        """;

    private const string IndentedSampleFallbackNTPInformationJson =
        """
        [
          "0.debian.pool.ntp.org",
          "1.debian.pool.ntp.org",
          "2.debian.pool.ntp.org",
          "3.debian.pool.ntp.org"
        ]
        """;

    private const string IndentedSampleSystemConfigurationJson =
        $$"""
        {
          "Version": 1,
          "NetworkInterfacesSettings": {
            "NetworkInterfaces": [
              {
                "CommonInformation": {
                  "Enabled": true,
                  "Name": "lan1",
                  "PhysicalAddress": "01:23:45:67:89:AB"
                },
                "IPv4": {
                  "Gateway": "120.86.52.18",
                  "DHCPEnabled": true,
                  "DHCPLease": {{IndentedSampleDHCPLeaseJson}},
                  "IPv4Details": [
                    {
                      "IPAddress": "120.86.52.18",
                      "Netmask": "120.86.52.18"
                    }
                  ],
                  "VLANEnabled": true,
                  "VLANID": 1
                }
              }
            ]
          },
          "NetworkDNSSettings": {
            "Hostname": "hostname",
            "MulticastDNSEnabled": true,
            "StaticHostsEnabled": true,
            "StaticHosts": [
              {
                "IPAddress": "120.86.52.18",
                "Hostname": "statichost"
              }
            ],
            "NameServersEnabled": true,
            "NameServers": [
              "120.86.52.18"
            ],
            "DNSSuffixEnabled": true,
            "DNSSuffix": "dnsSuffix",
            "SearchDomainsEnabled": true,
            "SearchDomains": [
              "searchDomain"
            ]
          },
          "NetworkProxySettings": {
            "FTP": {
              "Enabled": true,
              "Server": "ftpServer",
              "Port": 21,
              "Username": "ftpUser",
              "Password": "ftpPassword"
            },
            "HTTP": {
              "Enabled": true,
              "Server": "httpServer",
              "Port": 80,
              "Username": "httpUser",
              "Password": "httpPassword"
            },
            "HTTPS": {
              "Enabled": true,
              "Server": "httpsServer",
              "Port": 443,
              "Username": "httpsUser",
              "Password": "httpsPassword"
            },
            "SFTP": {
              "Enabled": true,
              "Server": "sftpServer",
              "Port": 22,
              "Username": "sftpUser",
              "Password": "sftpPassword"
            },
            "SOCKS": {
              "Enabled": true,
              "Server": "socksServer",
              "Port": 1080,
              "Username": "socksUser",
              "Password": "socksPassword"
            },
            "DoNotProxyListEnabled": true,
            "DoNotProxyList": [
              "InternalProxy"
            ]
          },
          "NetworkNTPSettings": {
            "NTPServersEnabled": true,
            "NTPServers": [
              "ptbtime1.ptb.de",
              "ptbtime2.ptb.de",
              "ptbtime3.ptb.de"
            ]
          },
          "Services": [
            {
              "Name": "TestService",
              "State": "Enabled"
            }
          ]
        }
        """;

    private static readonly JsonTypeInfo<RedeployScriptsResult> s_redeployContext = SourceGenerationContext.Default.RedeployScriptsResult;
    private static readonly JsonTypeInfo<GetSystemConfigurationResult> s_loadContext = SourceGenerationContext.Default.GetSystemConfigurationResult;
    private static readonly JsonTypeInfo<SetSystemConfigurationResult> s_applyContext = SourceGenerationContext.Default.SetSystemConfigurationResult;
    private static readonly JsonTypeInfo<GetDHCPLeaseInformationResult> s_getDHCPLeaseContext = SourceGenerationContext.Default.GetDHCPLeaseInformationResult;
    private static readonly JsonTypeInfo<GetNetworkStatusInformationResult> s_getNetworkContext = SourceGenerationContext.Default.GetNetworkStatusInformationResult;
    private static readonly JsonTypeInfo<GetNTPFallbackInformationResult> s_getNTPFallbackContext = SourceGenerationContext.Default.GetNTPFallbackInformationResult;
    private static readonly JsonTypeInfo<GetServiceStateResult> s_getServiceStateContext = SourceGenerationContext.Default.GetServiceStateResult;
    private static readonly JsonTypeInfo<RenewDHCPLeaseResult> s_renewDHCPLeaseContext = SourceGenerationContext.Default.RenewDHCPLeaseResult;
    private static readonly JsonTypeInfo<ServiceControlResult> s_serviceControlContext = SourceGenerationContext.Default.ServiceControlResult;
    private static readonly JsonTypeInfo<UpdateSystemResult> s_updateSystemContext = SourceGenerationContext.Default.UpdateSystemResult;
    private static readonly JsonTypeInfo<RestartSystemResult> s_restartSystemContext = SourceGenerationContext.Default.RestartSystemResult;
    private static readonly JsonTypeInfo<ShutdownSystemResult> s_shutdownSystemContext = SourceGenerationContext.Default.ShutdownSystemResult;
    private static readonly JsonTypeInfo<ResetSystemResult> s_resetSystemContext = SourceGenerationContext.Default.ResetSystemResult;
    private static readonly JsonTypeInfo<InstallSignedDebianPackageResult> s_installPackageContext = SourceGenerationContext.Default.InstallSignedDebianPackageResult;

    private static readonly string s_sampleDHCPLeaseJson = GetNonIndentedJson(IndentedSampleDHCPLeaseJson);
    private static readonly string s_sampleNetworkStatusJson = GetNonIndentedJson(IndentedSampleNetworkStatusJson);
    private static readonly string s_sampleFallbackNTPInformationJson = GetNonIndentedJson(IndentedSampleFallbackNTPInformationJson);
    private static readonly string s_sampleSystemConfigurationJson = GetNonIndentedJson(IndentedSampleSystemConfigurationJson);

    private static string GetNonIndentedJson(string indentedJson)
    {
        // Serialize JSON again to ensure it matches the expected format (non-indented).
        using var doc = JsonDocument.Parse(indentedJson);
        return JsonSerializer.Serialize(doc.RootElement);
    }

    public static IEnumerable<object[]> JsonNullData
    {
        get
        {
            var nullJson = "null";

            return
            [
                ["RedeployScriptsResult", SourceGenerationContext.Default.RedeployScriptsResult, nullJson],
                ["GetSystemConfigurationResult", SourceGenerationContext.Default.GetSystemConfigurationResult, nullJson],
                ["SetSystemConfigurationResult", SourceGenerationContext.Default.SetSystemConfigurationResult, nullJson],
                ["GetDHCPLeaseInformationResult", SourceGenerationContext.Default.GetDHCPLeaseInformationResult, nullJson],
                ["GetNetworkStatusInformationResult", SourceGenerationContext.Default.GetNetworkStatusInformationResult, nullJson],
                ["GetNTPFallbackInformationResult", SourceGenerationContext.Default.GetNTPFallbackInformationResult, nullJson],
                ["GetServiceStateResult", SourceGenerationContext.Default.GetServiceStateResult, nullJson],
                ["RenewDHCPLeaseResult", SourceGenerationContext.Default.RenewDHCPLeaseResult, nullJson],
                ["ServiceControlResult", SourceGenerationContext.Default.ServiceControlResult, nullJson],
                ["UpdateSystemResult", SourceGenerationContext.Default.UpdateSystemResult, nullJson],
                ["RestartSystemResult", SourceGenerationContext.Default.RestartSystemResult, nullJson],
                ["ShutdownSystemResult", SourceGenerationContext.Default.ShutdownSystemResult, nullJson],
                ["ResetSystemResult", SourceGenerationContext.Default.ResetSystemResult, nullJson],
                ["InstallSignedDebianPackageResult", SourceGenerationContext.Default.InstallSignedDebianPackageResult, nullJson]
            ];
        }
    }

    public static IEnumerable<object[]> MissingResultTypeData =>
    [
        [ "RedeployScriptsResult", s_redeployContext, """{"ResultType":"","Status":"Success","Message":null}""" ],
        [ "GetSystemConfigurationResult", s_loadContext, $$"""{"ResultType":"","Status":"Warning","Message":"Warning","Configuration":{{s_sampleSystemConfigurationJson}}}""" ],
        [ "SetSystemConfigurationResult", s_applyContext, """{"ResultType":"","Status":"Error","Message":"Error"}""" ],
        [ "GetDHCPLeaseInformationResult", s_getDHCPLeaseContext, $$"""{"ResultType":"","Status":"Warning","Message":"Warning","DHCPLease":{{s_sampleDHCPLeaseJson}}}""" ],
        [ "GetNetworkStatusInformationResult", s_getNetworkContext, $$"""{"ResultType":"","Status":"Warning","Message":"Warning","NetworkStatusInformation":{{s_sampleNetworkStatusJson}}}""" ],
        [ "GetNTPFallbackInformationResult", s_getNTPFallbackContext, $$"""{"ResultType":"","Status":"Warning","Message":"Warning","FallbackNTPServers":{{s_sampleFallbackNTPInformationJson}}}""" ],
        [ "GetServiceStateResult", s_getServiceStateContext, """{"ResultType":"","Status":"Warning","Message":"Warning","ActiveState":"active"}""" ],
        [ "GetDHCPLeaseInformationResult", s_renewDHCPLeaseContext, """{"ResultType":"","Status":"Warning","Message":"Warning","DHCPLease":null}""" ],
        [ "ServiceControlResult", s_serviceControlContext, """{"ResultType":"","Status":"Warning","Message":"Warning"}""" ],
        [ "SystemControlResult", s_updateSystemContext, """{"ResultType":"","Status":"Warning","Message":"Warning"}""" ],
        [ "SystemControlResult", s_restartSystemContext, """{"ResultType":"","Status":"Warning","Message":"Warning"}""" ],
        [ "SystemControlResult", s_shutdownSystemContext, """{"ResultType":"","Status":"Warning","Message":"Warning"}""" ],
        [ "SystemControlResult", s_resetSystemContext, """{"ResultType":"","Status":"Warning","Message":"Warning"}""" ],
        [ "SystemControlResult", s_installPackageContext, """{"ResultType":"","Status":"Warning","Message":"Warning"}""" ]
    ];

    public static IEnumerable<object[]> ResultTypeMismatchData =>
    [
        [ "RedeployScriptsResult", s_redeployContext, """{"ResultType":"Mismatch","Status":"Success","Message":null}""" ],
        [ "GetSystemConfigurationResult", s_loadContext, $$"""{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","Configuration":{{s_sampleSystemConfigurationJson}}}""" ],
        [ "SetSystemConfigurationResult", s_applyContext, """{"ResultType":"Mismatch","Status":"Error","Message":"Error"}""" ],
        [ "GetDHCPLeaseInformationResult", s_getDHCPLeaseContext, $$"""{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","DHCPLease":{{s_sampleDHCPLeaseJson}}}""" ],
        [ "GetNetworkStatusInformationResult", s_getNetworkContext, $$"""{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","NetworkStatusInformation":{{s_sampleNetworkStatusJson}}}""" ],
        [ "GetNTPFallbackInformationResult", s_getNTPFallbackContext, $$"""{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","FallbackNTPServers":{{s_sampleFallbackNTPInformationJson}}}""" ],
        [ "GetServiceStateResult", s_getServiceStateContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","ActiveState":"active"}""" ],
        [ "GetDHCPLeaseInformationResult", s_renewDHCPLeaseContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","DHCPLease":null}""" ],
        [ "ServiceControlResult", s_serviceControlContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning"}""" ],
        [ "SystemControlResult", s_updateSystemContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning"}""" ],
        [ "SystemControlResult", s_restartSystemContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning"}""" ],
        [ "SystemControlResult", s_shutdownSystemContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning"}""" ],
        [ "SystemControlResult", s_resetSystemContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning"}""" ],
        [ "SystemControlResult", s_installPackageContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning"}""" ]
    ];

    public static IEnumerable<object[]> DefaultResponseData
    {
        get
        {
            var defaultResponseJson = """{"Message":"Error processing request: Exception."}""";

            return
            [
                [ SourceGenerationContext.Default.RedeployScriptsResult, defaultResponseJson ],
                [ SourceGenerationContext.Default.GetSystemConfigurationResult, defaultResponseJson ],
                [ SourceGenerationContext.Default.SetSystemConfigurationResult, defaultResponseJson ],
                [ SourceGenerationContext.Default.GetDHCPLeaseInformationResult, defaultResponseJson ],
                [ SourceGenerationContext.Default.GetNetworkStatusInformationResult, defaultResponseJson ],
                [ SourceGenerationContext.Default.GetNTPFallbackInformationResult, defaultResponseJson ],
                [ SourceGenerationContext.Default.GetServiceStateResult, defaultResponseJson ],
                [ SourceGenerationContext.Default.RenewDHCPLeaseResult, defaultResponseJson ],
                [ SourceGenerationContext.Default.ServiceControlResult, defaultResponseJson ],
                [ SourceGenerationContext.Default.UpdateSystemResult, defaultResponseJson ],
                [ SourceGenerationContext.Default.RestartSystemResult, defaultResponseJson ],
                [ SourceGenerationContext.Default.ShutdownSystemResult, defaultResponseJson ],
                [ SourceGenerationContext.Default.ResetSystemResult, defaultResponseJson ],
                [ SourceGenerationContext.Default.InstallSignedDebianPackageResult, defaultResponseJson ]
            ];
        }
    }

    public static IEnumerable<object[]> MissingMemberData =>
    [
        [s_redeployContext, """{"ResultType":"RedeployScriptsResult","Status":"Success"}"""],
        [s_redeployContext, """{"ResultType":"RedeployScriptsResult","Message":null}"""],

        [s_loadContext, """{"ResultType":"GetSystemConfigurationResult","Status":"Warning"}"""],
        [s_loadContext, """{"ResultType":"GetSystemConfigurationResult","Message":"Warning"}"""],
        [s_loadContext, """{"ResultType":"GetSystemConfigurationResult","Configuration":{}}"""],
        [s_loadContext, """{"ResultType":"GetSystemConfigurationResult","Status":"Warning","Message":"Warning"}"""],
        [s_loadContext, """{"ResultType":"GetSystemConfigurationResult","Status":"Warning","Configuration":{}}"""],
        [s_loadContext, """{"ResultType":"GetSystemConfigurationResult","Message":"Warning","Configuration":{}}"""],

        [s_applyContext, """{"ResultType":"SetSystemConfigurationResult","Status":"Error"}"""],
        [s_applyContext, """{"ResultType":"SetSystemConfigurationResult","Message":"Error"}"""],

        [s_getDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning"}"""],
        [s_getDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Message":"Warning"}"""],
        [s_getDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","DHCPLease":null}"""],
        [s_getDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","Message":"Warning"}"""],
        [s_getDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","DHCPLease":null}"""],
        [s_getDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Message":"Warning","DHCPLease":null}"""],

        [s_getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning"}"""],
        [s_getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","Message":"Warning"}"""],
        [s_getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","NetworkStatusInformation":{}}"""],
        [s_getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","Message":"Warning"}"""],
        [s_getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","NetworkStatusInformation":{}}"""],
        [s_getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","Message":"Warning","NetworkStatusInformation":{}}"""],

        [s_getNTPFallbackContext, """{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning"}"""],
        [s_getNTPFallbackContext, """{"ResultType":"GetNTPFallbackInformationResult","Message":"Warning"}"""],
        [s_getNTPFallbackContext, """{"ResultType":"GetNTPFallbackInformationResult","FallbackNTPServers":[]}"""],
        [s_getNTPFallbackContext, """{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning","Message":"Warning"}"""],
        [s_getNTPFallbackContext, """{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning","FallbackNTPServers":[]}"""],
        [s_getNTPFallbackContext, """{"ResultType":"GetNTPFallbackInformationResult","Message":"Warning","FallbackNTPServers":[]}"""],

        [s_getServiceStateContext, """{"ResultType":"GetServiceStateResult","Status":"Warning"}"""],
        [s_getServiceStateContext, """{"ResultType":"GetServiceStateResult","Message":"Warning"}"""],
        [s_getServiceStateContext, """{"ResultType":"GetServiceStateResult","Status":"Warning","Message":"Warning"}"""],
        [s_getServiceStateContext, """{"ResultType":"GetServiceStateResult","Status":"Warning","ActiveState":"failed"}"""],
        [s_getServiceStateContext, """{"ResultType":"GetServiceStateResult","Message":"Warning","ActiveState":"failed"}"""],

        [s_renewDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning"}"""],
        [s_renewDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Message":"Warning"}"""],
        [s_renewDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","DHCPLease":null}"""],
        [s_renewDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","Message":"Warning"}"""],
        [s_renewDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","DHCPLease":null}"""],
        [s_renewDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Message":"Warning","DHCPLease":null}"""],

        [s_serviceControlContext, """{"ResultType":"ServiceControlResult","Status":"Warning"}"""],
        [s_serviceControlContext, """{"ResultType":"ServiceControlResult","Message":"Warning"}"""],

        [s_updateSystemContext, """{"ResultType":"SystemControlResult","Status":"Warning"}"""],
        [s_updateSystemContext, """{"ResultType":"SystemControlResult","Message":"Warning"}"""],

        [s_restartSystemContext, """{"ResultType":"SystemControlResult","Status":"Warning"}"""],
        [s_restartSystemContext, """{"ResultType":"SystemControlResult","Message":"Warning"}"""],

        [s_shutdownSystemContext, """{"ResultType":"SystemControlResult","Status":"Warning"}"""],
        [s_shutdownSystemContext, """{"ResultType":"SystemControlResult","Message":"Warning"}"""],

        [s_resetSystemContext, """{"ResultType":"SystemControlResult","Status":"Warning"}"""],
        [s_resetSystemContext, """{"ResultType":"SystemControlResult","Message":"Warning"}"""],

        [s_installPackageContext, """{"ResultType":"SystemControlResult","Status":"Warning"}"""],
        [s_installPackageContext, """{"ResultType":"SystemControlResult","Message":"Warning"}"""]
    ];

    public static IEnumerable<object[]> AdditionalMemberData =>
    [
        [s_redeployContext, $$"""{"ResultType":"RedeployScriptsResult","Status":"Success","Message":null,"Configuration":{{s_sampleSystemConfigurationJson}}}"""],
        [s_redeployContext, """{"ResultType":"RedeployScriptsResult","AdditionalMember":"Additional","Status":"Success","Message":null}"""],

        [s_loadContext, $$"""{"ResultType":"GetSystemConfigurationResult","Status":"Warning","Message":"Warning","Configuration":{{s_sampleSystemConfigurationJson}},"AdditionalMember":"Additional"}"""],
        [s_loadContext, $$"""{"ResultType":"GetSystemConfigurationResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","Configuration":{{s_sampleSystemConfigurationJson}}}"""],

        [s_applyContext, """{"ResultType":"SetSystemConfigurationResult","Status":"Error","Message":"Error","AdditionalMember":"Additional"}"""],
        [s_applyContext, """{"ResultType":"SetSystemConfigurationResult","AdditionalMember":"Additional","Status":"Error","Message":"Error"}"""],

        [s_getDHCPLeaseContext, $$"""{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","Message":"Warning","DHCPLease":{{s_sampleDHCPLeaseJson}},"AdditionalMember":"Additional"}"""],
        [s_getDHCPLeaseContext, $$"""{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","DHCPLease":{{s_sampleDHCPLeaseJson}}}"""],

        [s_getNetworkContext, $$"""{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","Message":"Warning","NetworkStatusInformation":{{s_sampleNetworkStatusJson}},"AdditionalMember":"Additional"}"""],
        [s_getNetworkContext, $$"""{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","NetworkStatusInformation":{{s_sampleNetworkStatusJson}}}"""],

        [s_getNTPFallbackContext, $$"""{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning","Message":"Warning","FallbackNTPServers":{{s_sampleFallbackNTPInformationJson}},"AdditionalMember":"Additional"}"""],
        [s_getNTPFallbackContext, $$"""{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","FallbackNTPServers":{{s_sampleFallbackNTPInformationJson}}}"""],

        [s_getServiceStateContext, """{"ResultType":"GetServiceStateResult","Status":"Warning","Message":"Warning","ActiveState":"active","AdditionalMember":"Additional"}"""],
        [s_getServiceStateContext, """{"ResultType":"GetServiceStateResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","ActiveState":"active"}"""],

        [s_renewDHCPLeaseContext, $$"""{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","Message":"Warning","DHCPLease":{{s_sampleDHCPLeaseJson}},"AdditionalMember":"Additional"}"""],
        [s_renewDHCPLeaseContext, $$"""{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","DHCPLease":{{s_sampleDHCPLeaseJson}}}"""],

        [s_serviceControlContext, """{"ResultType":"ServiceControlResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional"}"""],
        [s_serviceControlContext, """{"ResultType":"ServiceControlResult","AdditionalMember":"Additional","Status":"Warning","Message":"Warning"}"""],

        [s_updateSystemContext, """{"ResultType":"SystemControlResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional"}"""],
        [s_updateSystemContext, """{"ResultType":"SystemControlResult","AdditionalMember":"Additional","Status":"Warning","Message":"Warning"}"""],

        [s_restartSystemContext, """{"ResultType":"SystemControlResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional"}"""],
        [s_restartSystemContext, """{"ResultType":"SystemControlResult","AdditionalMember":"Additional","Status":"Warning","Message":"Warning"}"""],

        [s_shutdownSystemContext, """{"ResultType":"SystemControlResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional"}"""],
        [s_shutdownSystemContext, """{"ResultType":"SystemControlResult","AdditionalMember":"Additional","Status":"Warning","Message":"Warning"}"""],

        [s_resetSystemContext, """{"ResultType":"SystemControlResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional"}"""],
        [s_resetSystemContext, """{"ResultType":"SystemControlResult","AdditionalMember":"Additional","Status":"Warning","Message":"Warning"}"""],

        [s_installPackageContext, """{"ResultType":"SystemControlResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional"}"""],
        [s_installPackageContext, """{"ResultType":"SystemControlResult","AdditionalMember":"Additional","Status":"Warning","Message":"Warning"}"""]
    ];

    public static IEnumerable<object[]> InvalidNullMemberData
    {
        get
        {
            var invalidNullData = "null";

            return
            [
                [s_redeployContext, $$"""{"ResultType":{{invalidNullData}},"Status":"Success","Message":""}"""],
                [s_getNetworkContext, $$"""{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","Message":"Warning","NetworkStatusInformation":{{invalidNullData}}}"""],
                [s_getNTPFallbackContext, $$"""{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning","Message":"Warning","FallbackNTPServers":{{invalidNullData}}}"""],
                [s_getServiceStateContext, $$"""{"ResultType":"GetServiceStateResult","Status":"Warning","Message":"Warning","ActiveState":{{invalidNullData}}}"""]
            ];
        }
    }

    public static IEnumerable<object[]> ValidMemberData =>
    [
        [s_redeployContext, """{"ResultType":"RedeployScriptsResult","Status":"Success","Message":null}"""],
        [s_loadContext, $$"""{"ResultType":"GetSystemConfigurationResult","Status":"Warning","Message":"Warning","Configuration":{{s_sampleSystemConfigurationJson}}}"""],
        [s_applyContext, """{"ResultType":"SetSystemConfigurationResult","Status":"Error","Message":"Error"}"""],

        [s_getDHCPLeaseContext, $$"""{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","Message":"Warning","DHCPLease":{{s_sampleDHCPLeaseJson}}}"""],
        [s_getDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","Message":"Warning","DHCPLease":null}"""],

        [s_getNetworkContext, $$"""{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","Message":"Warning","NetworkStatusInformation":{{s_sampleNetworkStatusJson}}}"""],

        [s_getNTPFallbackContext, $$"""{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning","Message":"Warning","FallbackNTPServers":{{s_sampleFallbackNTPInformationJson}}}"""],
        [s_getNTPFallbackContext, """{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning","Message":"Warning","FallbackNTPServers":[]}"""],

        [s_getServiceStateContext, """{"ResultType":"GetServiceStateResult","Status":"Warning","Message":"Warning","ActiveState":"active"}"""],

        [s_renewDHCPLeaseContext, $$"""{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","Message":"Warning","DHCPLease":{{s_sampleDHCPLeaseJson}}}"""],
        [s_serviceControlContext, """{"ResultType":"ServiceControlResult","Status":"Success","Message":"Success"}"""],
        [s_updateSystemContext, """{"ResultType":"SystemControlResult","Status":"Success","Message":"Success"}"""],
        [s_restartSystemContext, """{"ResultType":"SystemControlResult","Status":"Success","Message":"Success"}"""],
        [s_shutdownSystemContext, """{"ResultType":"SystemControlResult","Status":"Success","Message":"Success"}"""],
        [s_resetSystemContext, """{"ResultType":"SystemControlResult","Status":"Success","Message":"Success"}"""],
        [s_installPackageContext, """{"ResultType":"SystemControlResult","Status":"Success","Message":"Success"}"""]
    ];

    public class Deserialize : SourceGeneratedTypeSerializerTests
    {
        [Theory]
        [MemberData(nameof(JsonNullData))]
        public void ShouldThrow_WhenJsonIsNull<TType>(
            string jsonTypeInfoTypeName,
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = () => SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(
                    $"JSON string could not be deserialized to '{jsonTypeInfoTypeName}'." +
                    Environment.NewLine +
                    $"JSON string was: 'null'");
        }

        [Theory]
        [MemberData(nameof(MissingResultTypeData))]
        public void ShouldThrow_WhenResultTypeIsMissing<TType>(
            string jsonTypeInfoTypeName,
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = () => SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage(
                    $"Required property 'ResultType' is missing or empty " +
                    $"(expected type '{jsonTypeInfoTypeName}').");
        }

        [Theory]
        [MemberData(nameof(ResultTypeMismatchData))]
        public void ShouldThrow_WhenResultTypeIsMismatch<TType>(
            string jsonTypeInfoTypeName,
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = () => SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage(
                    $"Type mismatch: Expected '{jsonTypeInfoTypeName}', but got 'Mismatch'.");
        }

        [Theory]
        [MemberData(nameof(DefaultResponseData))]
        [MemberData(nameof(MissingMemberData))]
        public void ShouldThrow_WhenMissingMemberFound<TType>(
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = () => SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage("*missing*");
        }

        [Theory]
        [MemberData(nameof(AdditionalMemberData))]
        public void ShouldThrow_WhenUnmappedMemberFound<TType>(
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = () => SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage("*could not be mapped*");
        }

        [Theory]
        [MemberData(nameof(InvalidNullMemberData))]
        public void ShouldThrow_WhenNullabilityIsNotRespected<TType>(
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = () => SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage("*doesn't allow null values*");
        }

        [Theory]
        [MemberData(nameof(ValidMemberData))]
        public void ShouldReturnResultType_WhenJsonValid<TType>(
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should().BeOfType<TType>();
        }
    }

    public class Serialize : SourceGeneratedTypeSerializerTests
    {
        [Theory]
        [MemberData(nameof(ValidMemberData))]
        public void ShouldSerializeResultType_WhenCalled<TType>(
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Arrange
            var deserializedType = SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Act
            var result = SourceGeneratedTypeSerializer.Serialize(deserializedType, jsonTypeInfo);

            // Assert
            result.Should().Be(json);
        }
    }
}
