using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FluentAssertions;
using Xunit;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
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
            "FallbackNTPServers": [
              "0.debian.pool.ntp.org",
              "1.debian.pool.ntp.org",
              "2.debian.pool.ntp.org",
              "3.debian.pool.ntp.org"
            ],
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
    private static readonly JsonTypeInfo<GetViciOneSuiteVersionsResult> s_getSuiteContext = SourceGenerationContext.Default.GetViciOneSuiteVersionsResult;
    private static readonly JsonTypeInfo<InstallViciOneSuiteVersionResult> s_installSuiteContext = SourceGenerationContext.Default.InstallViciOneSuiteVersionResult;
    private static readonly JsonTypeInfo<GetDHCPLeaseInformationResult> s_getDHCPLeaseContext = SourceGenerationContext.Default.GetDHCPLeaseInformationResult;
    private static readonly JsonTypeInfo<GetNetworkStatusInformationResult> s_getNetworkContext = SourceGenerationContext.Default.GetNetworkStatusInformationResult;
    private static readonly JsonTypeInfo<GetNTPFallbackInformationResult> s_getNTPFallbackContext = SourceGenerationContext.Default.GetNTPFallbackInformationResult;
    private static readonly JsonTypeInfo<RenewDHCPLeaseResult> s_renewDHCPLeaseContext = SourceGenerationContext.Default.RenewDHCPLeaseResult;
    private static readonly JsonTypeInfo<ServiceControlResult> s_serviceControlContext = SourceGenerationContext.Default.ServiceControlResult;
    private static readonly JsonTypeInfo<UpdateSystemResult> s_updateSystemContext = SourceGenerationContext.Default.UpdateSystemResult;
    private static readonly JsonTypeInfo<RestartSystemResult> s_restartSystemContext = SourceGenerationContext.Default.RestartSystemResult;
    private static readonly JsonTypeInfo<ResetSystemResult> s_resetSystemContext = SourceGenerationContext.Default.ResetSystemResult;

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

            return new List<object[]>
            {
                new object[] { "RedeployScriptsResult", SourceGenerationContext.Default.RedeployScriptsResult, nullJson },
                new object[] { "GetSystemConfigurationResult", SourceGenerationContext.Default.GetSystemConfigurationResult, nullJson },
                new object[] { "SetSystemConfigurationResult", SourceGenerationContext.Default.SetSystemConfigurationResult, nullJson },
                new object[] { "GetViciOneSuiteVersionsResult", SourceGenerationContext.Default.GetViciOneSuiteVersionsResult, nullJson },
                new object[] { "InstallViciOneSuiteVersionResult", SourceGenerationContext.Default.InstallViciOneSuiteVersionResult, nullJson },
                new object[] { "GetDHCPLeaseInformationResult", SourceGenerationContext.Default.GetDHCPLeaseInformationResult, nullJson },
                new object[] { "GetNetworkStatusInformationResult", SourceGenerationContext.Default.GetNetworkStatusInformationResult, nullJson },
                new object[] { "GetNTPFallbackInformationResult", SourceGenerationContext.Default.GetNTPFallbackInformationResult, nullJson },
                new object[] { "RenewDHCPLeaseResult", SourceGenerationContext.Default.RenewDHCPLeaseResult, nullJson },
                new object[] { "ServiceControlResult", SourceGenerationContext.Default.ServiceControlResult, nullJson },
                new object[] { "UpdateSystemResult", SourceGenerationContext.Default.UpdateSystemResult, nullJson },
                new object[] { "RestartSystemResult", SourceGenerationContext.Default.RestartSystemResult, nullJson },
                new object[] { "ResetSystemResult", SourceGenerationContext.Default.ResetSystemResult, nullJson }
            };
        }
    }

    public static IEnumerable<object[]> MissingResultTypeData => new List<object[]>
    {
        new object[] { "RedeployScriptsResult", s_redeployContext, """{"ResultType":"","Status":"Success","Message":null}""" },
        new object[] { "GetSystemConfigurationResult", s_loadContext, $$"""{"ResultType":"","Status":"Warning","Message":"Warning","Configuration":{{s_sampleSystemConfigurationJson}}}""" },
        new object[] { "SetSystemConfigurationResult", s_applyContext, """{"ResultType":"","Status":"Error","Message":"Error"}""" },
        new object[] { "GetViciOneSuiteVersionsResult", s_getSuiteContext, """{"ResultType":"","Status":"Warning","Message":"Warning","SuiteVersions":[]}""" },
        new object[] { "InstallViciOneSuiteVersionResult", s_installSuiteContext, """{"ResultType":"","Status":"Error","Message":"Error"}""" },
        new object[] { "GetDHCPLeaseInformationResult", s_getDHCPLeaseContext, $$"""{"ResultType":"","Status":"Warning","Message":"Warning","DHCPLease":{{s_sampleDHCPLeaseJson}}}""" },
        new object[] { "GetNetworkStatusInformationResult", s_getNetworkContext, $$"""{"ResultType":"","Status":"Warning","Message":"Warning","NetworkStatusInformation":{{s_sampleNetworkStatusJson}}}""" },
        new object[] { "GetNTPFallbackInformationResult", s_getNTPFallbackContext, $$"""{"ResultType":"","Status":"Warning","Message":"Warning","FallbackNTPServers":{{s_sampleFallbackNTPInformationJson}}}""" },
        new object[] { "RenewDHCPLeaseResult", s_renewDHCPLeaseContext, """{"ResultType":"","Status":"Warning","Message":"Warning","DHCPLease":null}""" },
        new object[] { "ServiceControlResult", s_serviceControlContext, """{"ResultType":"","Status":"Warning","Message":"Warning"}""" },
        new object[] { "UpdateSystemResult", s_updateSystemContext, """{"ResultType":"","Status":"Warning","Message":"Warning"}""" },
        new object[] { "RestartSystemResult", s_restartSystemContext, """{"ResultType":"","Status":"Warning","Message":"Warning"}""" },
        new object[] { "ResetSystemResult", s_resetSystemContext, """{"ResultType":"","Status":"Warning","Message":"Warning"}""" }
    };

    public static IEnumerable<object[]> ResultTypeMismatchData => new List<object[]>
    {
        new object[] { "RedeployScriptsResult", s_redeployContext, """{"ResultType":"Mismatch","Status":"Success","Message":null}""" },
        new object[] { "GetSystemConfigurationResult", s_loadContext, $$"""{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","Configuration":{{s_sampleSystemConfigurationJson}}}""" },
        new object[] { "SetSystemConfigurationResult", s_applyContext, """{"ResultType":"Mismatch","Status":"Error","Message":"Error"}""" },
        new object[] { "GetViciOneSuiteVersionsResult", s_getSuiteContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","SuiteVersions":[]}""" },
        new object[] { "InstallViciOneSuiteVersionResult", s_installSuiteContext, """{"ResultType":"Mismatch","Status":"Error","Message":"Error"}""" },
        new object[] { "GetDHCPLeaseInformationResult", s_getDHCPLeaseContext, $$"""{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","DHCPLease":{{s_sampleDHCPLeaseJson}}}""" },
        new object[] { "GetNetworkStatusInformationResult", s_getNetworkContext, $$"""{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","NetworkStatusInformation":{{s_sampleNetworkStatusJson}}}""" },
        new object[] { "GetNTPFallbackInformationResult", s_getNTPFallbackContext, $$"""{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","FallbackNTPServers":{{s_sampleFallbackNTPInformationJson}}}""" },
        new object[] { "RenewDHCPLeaseResult", s_renewDHCPLeaseContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","DHCPLease":null}""" },
        new object[] { "ServiceControlResult", s_serviceControlContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning"}""" },
        new object[] { "UpdateSystemResult", s_updateSystemContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning"}""" },
        new object[] { "RestartSystemResult", s_restartSystemContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning"}""" },
        new object[] { "ResetSystemResult", s_resetSystemContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning"}""" }
    };

    public static IEnumerable<object[]> DefaultResponseData
    {
        get
        {
            var defaultResponseJson = """{"Message":"Error processing request: Exception."}""";

            return new List<object[]>
            {
                new object[] { SourceGenerationContext.Default.RedeployScriptsResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.GetSystemConfigurationResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.SetSystemConfigurationResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.GetViciOneSuiteVersionsResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.InstallViciOneSuiteVersionResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.GetDHCPLeaseInformationResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.GetNetworkStatusInformationResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.GetNTPFallbackInformationResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.RenewDHCPLeaseResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.ServiceControlResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.UpdateSystemResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.RestartSystemResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.ResetSystemResult, defaultResponseJson }
            };
        }
    }

    public static IEnumerable<object[]> MissingMemberData => new List<object[]>
    {
        new object[] { s_redeployContext, """{"ResultType":"RedeployScriptsResult","Status":"Success"}""" },
        new object[] { s_redeployContext, """{"ResultType":"RedeployScriptsResult","Message":null}""" },

        new object[] { s_loadContext, """{"ResultType":"GetSystemConfigurationResult","Status":"Warning"}""" },
        new object[] { s_loadContext, """{"ResultType":"GetSystemConfigurationResult","Message":"Warning"}""" },
        new object[] { s_loadContext, """{"ResultType":"GetSystemConfigurationResult","Configuration":{}}""" },
        new object[] { s_loadContext, """{"ResultType":"GetSystemConfigurationResult","Status":"Warning","Message":"Warning"}""" },
        new object[] { s_loadContext, """{"ResultType":"GetSystemConfigurationResult","Status":"Warning","Configuration":{}}""" },
        new object[] { s_loadContext, """{"ResultType":"GetSystemConfigurationResult","Message":"Warning","Configuration":{}}""" },

        new object[] { s_applyContext, """{"ResultType":"SetSystemConfigurationResult","Status":"Error"}""" },
        new object[] { s_applyContext, """{"ResultType":"SetSystemConfigurationResult","Message":"Error"}""" },

        new object[] { s_getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Status":"Warning"}""" },
        new object[] { s_getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Message":"Warning"}""" },
        new object[] { s_getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","SuiteVersions":[]}""" },
        new object[] { s_getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Status":"Warning","Message":"Warning"}""" },
        new object[] { s_getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Status":"Warning","SuiteVersions":[]}""" },
        new object[] { s_getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Message":"Warning","SuiteVersions":[]}""" },

        new object[] { s_installSuiteContext, """{"ResultType":"InstallViciOneSuiteVersionResult","Status":"Error"}""" },
        new object[] { s_installSuiteContext, """{"ResultType":"InstallViciOneSuiteVersionResult","Message":"Error"}""" },

        new object[] { s_getDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning"}""" },
        new object[] { s_getDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Message":"Warning"}""" },
        new object[] { s_getDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","DHCPLease":null}""" },
        new object[] { s_getDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","Message":"Warning"}""" },
        new object[] { s_getDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","DHCPLease":null}""" },
        new object[] { s_getDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Message":"Warning","DHCPLease":null}""" },

        new object[] { s_getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning"}""" },
        new object[] { s_getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","Message":"Warning"}""" },
        new object[] { s_getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","NetworkStatusInformation":{}}""" },
        new object[] { s_getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","Message":"Warning"}""" },
        new object[] { s_getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","NetworkStatusInformation":{}}""" },
        new object[] { s_getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","Message":"Warning","NetworkStatusInformation":{}}""" },

        new object[] { s_getNTPFallbackContext, """{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning"}""" },
        new object[] { s_getNTPFallbackContext, """{"ResultType":"GetNTPFallbackInformationResult","Message":"Warning"}""" },
        new object[] { s_getNTPFallbackContext, """{"ResultType":"GetNTPFallbackInformationResult","FallbackNTPServers":[]}""" },
        new object[] { s_getNTPFallbackContext, """{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning","Message":"Warning"}""" },
        new object[] { s_getNTPFallbackContext, """{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning","FallbackNTPServers":[]}""" },
        new object[] { s_getNTPFallbackContext, """{"ResultType":"GetNTPFallbackInformationResult","Message":"Warning","FallbackNTPServers":[]}""" },

        new object[] { s_renewDHCPLeaseContext, """{"ResultType":"RenewDHCPLeaseResult","Status":"Warning"}""" },
        new object[] { s_renewDHCPLeaseContext, """{"ResultType":"RenewDHCPLeaseResult","Message":"Warning"}""" },
        new object[] { s_renewDHCPLeaseContext, """{"ResultType":"RenewDHCPLeaseResult","DHCPLease":null}""" },
        new object[] { s_renewDHCPLeaseContext, """{"ResultType":"RenewDHCPLeaseResult","Status":"Warning","Message":"Warning"}""" },
        new object[] { s_renewDHCPLeaseContext, """{"ResultType":"RenewDHCPLeaseResult","Status":"Warning","DHCPLease":null}""" },
        new object[] { s_renewDHCPLeaseContext, """{"ResultType":"RenewDHCPLeaseResult","Message":"Warning","DHCPLease":null}""" },

        new object[] { s_serviceControlContext, """{"ResultType":"ServiceControlResult","Status":"Warning"}""" },
        new object[] { s_serviceControlContext, """{"ResultType":"ServiceControlResult","Message":"Warning"}""" },

        new object[] { s_updateSystemContext, """{"ResultType":"UpdateSystemResult","Status":"Warning"}""" },
        new object[] { s_updateSystemContext, """{"ResultType":"UpdateSystemResult","Message":"Warning"}""" },

        new object[] { s_restartSystemContext, """{"ResultType":"RestartSystemResult","Status":"Warning"}""" },
        new object[] { s_restartSystemContext, """{"ResultType":"RestartSystemResult","Message":"Warning"}""" },

        new object[] { s_resetSystemContext, """{"ResultType":"ResetSystemResult","Status":"Warning"}""" },
        new object[] { s_resetSystemContext, """{"ResultType":"ResetSystemResult","Message":"Warning"}""" }
    };

    public static IEnumerable<object[]> AdditionalMemberData => new List<object[]>
    {
        new object[] { s_redeployContext, $$"""{"ResultType":"RedeployScriptsResult","Status":"Success","Message":null,"Configuration":{{s_sampleSystemConfigurationJson}}}""" },
        new object[] { s_redeployContext, """{"ResultType":"RedeployScriptsResult","AdditionalMember":"Additional","Status":"Success","Message":null}""" },

        new object[] { s_loadContext, $$"""{"ResultType":"GetSystemConfigurationResult","Status":"Warning","Message":"Warning","Configuration":{{s_sampleSystemConfigurationJson}},"AdditionalMember":"Additional"}""" },
        new object[] { s_loadContext, $$"""{"ResultType":"GetSystemConfigurationResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","Configuration":{{s_sampleSystemConfigurationJson}}}""" },

        new object[] { s_applyContext, """{"ResultType":"SetSystemConfigurationResult","Status":"Error","Message":"Error","AdditionalMember":"Additional"}""" },
        new object[] { s_applyContext, """{"ResultType":"SetSystemConfigurationResult","AdditionalMember":"Additional","Status":"Error","Message":"Error"}""" },

        new object[] { s_getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Status":"Warning","Message":"Warning","SuiteVersions":[],"AdditionalMember":"Additional"}""" },
        new object[] { s_getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","SuiteVersions":[]}""" },

        new object[] { s_installSuiteContext, """{"ResultType":"InstallViciOneSuiteVersionResult","Status":"Error","Message":"Error","AdditionalMember":"Additional"}""" },
        new object[] { s_installSuiteContext, """{"ResultType":"InstallViciOneSuiteVersionResult","AdditionalMember":"Additional","Status":"Error","Message":"Error"}""" },

        new object[] { s_getDHCPLeaseContext, $$"""{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","Message":"Warning","DHCPLease":{{s_sampleDHCPLeaseJson}},"AdditionalMember":"Additional"}""" },
        new object[] { s_getDHCPLeaseContext, $$"""{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","DHCPLease":{{s_sampleDHCPLeaseJson}}}""" },

        new object[] { s_getNetworkContext, $$"""{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","Message":"Warning","NetworkStatusInformation":{{s_sampleNetworkStatusJson}},"AdditionalMember":"Additional"}""" },
        new object[] { s_getNetworkContext, $$"""{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","NetworkStatusInformation":{{s_sampleNetworkStatusJson}}}""" },

        new object[] { s_getNTPFallbackContext, $$"""{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning","Message":"Warning","FallbackNTPServers":{{s_sampleFallbackNTPInformationJson}},"AdditionalMember":"Additional"}""" },
        new object[] { s_getNTPFallbackContext, $$"""{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","FallbackNTPServers":{{s_sampleFallbackNTPInformationJson}}}""" },

        new object[] { s_renewDHCPLeaseContext, $$"""{"ResultType":"RenewDHCPLeaseResult","Status":"Warning","Message":"Warning","DHCPLease":{{s_sampleDHCPLeaseJson}},"AdditionalMember":"Additional"}""" },
        new object[] { s_renewDHCPLeaseContext, $$"""{"ResultType":"RenewDHCPLeaseResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","DHCPLease":{{s_sampleDHCPLeaseJson}}}""" },

        new object[] { s_serviceControlContext, """{"ResultType":"ServiceControlResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional"}""" },
        new object[] { s_serviceControlContext, """{"ResultType":"ServiceControlResult","AdditionalMember":"Additional","Status":"Warning","Message":"Warning"}""" },

        new object[] { s_updateSystemContext, """{"ResultType":"UpdateSystemResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional"}""" },
        new object[] { s_updateSystemContext, """{"ResultType":"UpdateSystemResult","AdditionalMember":"Additional","Status":"Warning","Message":"Warning"}""" },

        new object[] { s_restartSystemContext, """{"ResultType":"RestartSystemResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional"}""" },
        new object[] { s_restartSystemContext, """{"ResultType":"RestartSystemResult","AdditionalMember":"Additional","Status":"Warning","Message":"Warning"}""" },

        new object[] { s_resetSystemContext, """{"ResultType":"ResetSystemResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional"}""" },
        new object[] { s_resetSystemContext, """{"ResultType":"ResetSystemResult","AdditionalMember":"Additional","Status":"Warning","Message":"Warning"}""" }
    };

    public static IEnumerable<object[]> InvalidNullMemberData
    {
        get
        {
            var invalidNullData = "null";

            return new List<object[]>
            {
                new object[] { s_redeployContext, $$"""{"ResultType":{{invalidNullData}},"Status":"Success","Message":""}""" },
                new object[] { s_getSuiteContext, $$"""{"ResultType":"GetViciOneSuiteVersionsResult","Status":"Warning","Message":"Warning","SuiteVersions":{{invalidNullData}}}""" },
                new object[] { s_getNetworkContext, $$"""{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","Message":"Warning","NetworkStatusInformation":{{invalidNullData}}}""" },
                new object[] { s_getNTPFallbackContext, $$"""{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning","Message":"Warning","FallbackNTPServers":{{invalidNullData}}}""" }
            };
        }
    }

    public static IEnumerable<object[]> ValidMemberData => new List<object[]>
    {
        new object[] { s_redeployContext, """{"ResultType":"RedeployScriptsResult","Status":"Success","Message":null}""" },
        new object[] { s_loadContext, $$"""{"ResultType":"GetSystemConfigurationResult","Status":"Warning","Message":"Warning","Configuration":{{s_sampleSystemConfigurationJson}}}""" },
        new object[] { s_applyContext, """{"ResultType":"SetSystemConfigurationResult","Status":"Error","Message":"Error"}""" },
        new object[] { s_getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Status":"Warning","Message":"Warning","SuiteVersions":[]}""" },
        new object[] { s_installSuiteContext, """{"ResultType":"InstallViciOneSuiteVersionResult","Status":"Error","Message":"Error"}""" },

        new object[] { s_getDHCPLeaseContext, $$"""{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","Message":"Warning","DHCPLease":{{s_sampleDHCPLeaseJson}}}""" },
        new object[] { s_getDHCPLeaseContext, """{"ResultType":"GetDHCPLeaseInformationResult","Status":"Warning","Message":"Warning","DHCPLease":null}""" },

        new object[] { s_getNetworkContext, $$"""{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","Message":"Warning","NetworkStatusInformation":{{s_sampleNetworkStatusJson}}}""" },

        new object[] { s_getNTPFallbackContext, $$"""{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning","Message":"Warning","FallbackNTPServers":{{s_sampleFallbackNTPInformationJson}}}""" },
        new object[] { s_getNTPFallbackContext, """{"ResultType":"GetNTPFallbackInformationResult","Status":"Warning","Message":"Warning","FallbackNTPServers":[]}""" },

        new object[] { s_renewDHCPLeaseContext, $$"""{"ResultType":"RenewDHCPLeaseResult","Status":"Warning","Message":"Warning","DHCPLease":{{s_sampleDHCPLeaseJson}}}""" },
        new object[] { s_serviceControlContext, """{"ResultType":"ServiceControlResult","Status":"Success","Message":"Success"}""" },
        new object[] { s_updateSystemContext, """{"ResultType":"UpdateSystemResult","Status":"Success","Message":"Success"}""" },
        new object[] { s_restartSystemContext, """{"ResultType":"RestartSystemResult","Status":"Success","Message":"Success"}""" },
        new object[] { s_resetSystemContext, """{"ResultType":"ResetSystemResult","Status":"Success","Message":"Success"}""" }
    };

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
        public void ShouldReturnGetSystemConfigurationResult_WhenJsonValid<TType>(
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
        public void ShouldSerializeResult_WhenCalled<TType>(
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
