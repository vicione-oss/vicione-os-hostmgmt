using System.Net;
using System.Text.Json;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Enums;
using HostManagement.Shared.Validation;
using HostManagement.SystemConfig;

namespace HostManagement.Communication.CommandLine;

internal static class SystemConfigurationTemplate
{
    private static TextWriter s_output = Console.Out;

    /// <summary>
    /// Sets the <see cref="TextWriter"/>. The default is console output.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/>.</param>
    internal static void SetOutput(TextWriter writer) => s_output = writer;

    /// <summary>
    /// Creates and outputs a template of the <see cref="SystemConfiguration"/> with sample data.
    /// </summary>
    internal static void CreateAndOutput()
    {
        var internalConfigTemplateJson = JsonSerializer
            .Serialize(
                CreateSharedConfigTemplate().ToInternalContracts(),
                SystemConfigurationPersistenceContext.Default.SystemConfiguration);

        s_output.WriteLine(internalConfigTemplateJson);
    }

    /// <summary>
    /// Validates a (modified) template of the <see cref="SystemConfiguration"/>, specified in the given file path
    /// and outputs the validation result.
    /// </summary>
    /// <param name="filePath">The file path to the template file to validate.</param>
    internal static async Task Validate(string filePath)
    {
        var internalConfig = await LoadInternalConfigTemplate(filePath);
        var validator = new SystemConfigurationValidator();
        var result = await validator.ValidateAsync(internalConfig.ToSharedContracts());

        await s_output.WriteLineAsync(result.EntriesString);
    }

    private static SystemConfiguration CreateSharedConfigTemplate()
    {
        var ipv4Template1 = IPAddress.Parse("0.0.0.1");
        var ipv4Template2 = IPAddress.Parse("0.0.0.2");

        return new SystemConfiguration
        {
            Version = 2,
            NetworkInterfacesSettings = new()
            {
                NetworkInterfaces =
                [
                    new()
                    {
                        CommonInformation = new()
                        {
                            Enabled = true,
                            Name = "interface1",
                            PhysicalAddress = "00:00:00:00:00:01"
                        },
                        IPv4 = new()
                        {
                            DHCPEnabled = true
                        }
                    },
                    new()
                    {
                        CommonInformation = new()
                        {
                            Enabled = true,
                            Name = "interface2",
                            PhysicalAddress = "00:00:00:00:00:02"
                        },
                        IPv4 = new()
                        {
                            IPv4Details =
                            [
                                new() { IPAddress = ipv4Template1, Netmask = ipv4Template1 },
                                new() { IPAddress = ipv4Template2, Netmask = ipv4Template2 }
                            ]
                        }
                    }
                ]
            },
            NetworkDNSSettings = new()
            {
                Hostname = "hostname",
                StaticHosts = new()
                {
                    Hosts =
                    [
                        new() { Hostname = "staticHost1", IPAddress = ipv4Template1 },
                        new() { Hostname = "staticHost2", IPAddress = ipv4Template2 }
                    ]
                },
                NameServers = new()
                {
                    Addresses = [ipv4Template1, ipv4Template2]
                },
                DNSSuffix = "myCompany.com",
                SearchDomains = ["local", "myCompany.com"]
            },
            NetworkProxySettings = new()
            {
                HTTP = new()
                {
                    Server = "http://proxy.example.com",
                    Port = 80
                },
                HTTPS = new()
                {
                    Server = "https://proxy.example.com",
                    Port = 443,
                    Username = "user",
                    Password = "pass"
                },
                FTP = new()
                {
                    Server = "ftp://proxy.example.com",
                    Port = 21,
                    Username = "user",
                    Password = "pass"
                },
                SFTP = new()
                {
                    Server = "sftp://proxy.example.com",
                    Port = 22
                },
                SOCKS = new()
                {
                    Server = "socks://proxy.example.com",
                    Port = 1080
                },
                NoProxy = new()
                {
                    Entries = ["company.com", "192.168.178.42"]
                }
            },
            NetworkNTPSettings = new()
            {
                Servers = ["ptbtime1.ptb.de", "ptbtime2.ptb.de"]
            },
            Services =
            [
                new() { Name = "service1.service", State = ServiceState.Disabled },
                new() { Name = "service2.service", State = ServiceState.Disabled }
            ]
        };
    }

    private static async Task<SystemConfig.Contracts.SystemConfiguration> LoadInternalConfigTemplate(string filePath)
    {
        await using var stream = new FileStream(filePath, new FileStreamOptions()
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            Share = FileShare.Read,
        });

        return await JsonSerializer
            .DeserializeAsync(
                stream,
                SystemConfigurationPersistenceContext.Default.SystemConfiguration,
                CancellationToken.None)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                $"UTF-8 JSON stream could not be deserialized to '{nameof(SystemConfiguration)}'.");
    }
}
