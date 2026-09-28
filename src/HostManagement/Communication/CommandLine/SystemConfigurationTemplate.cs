using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Enums;
using HostManagement.Shared.Validation;
using HostManagement.SystemConfig;

namespace HostManagement.Communication.CommandLine;

internal static class SystemConfigurationTemplate
{
    /// <summary>
    /// Creates and outputs a template of the <see cref="SystemConfiguration"/> with sample data.
    /// </summary>
    internal static void CreateAndOutput() => CreateAndOutput(Console.Out);

    /// <summary>
    /// Creates and outputs a template of the <see cref="SystemConfiguration"/> with sample data.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/>.</param>
    internal static void CreateAndOutput(TextWriter writer)
    {
        var internalConfigTemplateJson = JsonSerializer
            .Serialize(
                CreateSharedConfigTemplate().ToInternalContracts(),
                SystemConfigurationPersistenceContext.Default.SystemConfiguration);

        writer.WriteLine(internalConfigTemplateJson);
    }

    /// <summary>
    /// Validates a (modified) template of the <see cref="SystemConfiguration"/>, specified in the given file path
    /// and outputs the validation result.
    /// </summary>
    /// <param name="filePath">The file path to the template file to validate.</param>
    internal static async Task Validate(string filePath)
    {
        await using var stream = new FileStream(filePath, new FileStreamOptions()
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            Share = FileShare.Read,
        });

        await Validate(stream);
    }

    /// <summary>
    /// Validates a (modified) template of the <see cref="SystemConfiguration"/>, specified in the given stream
    /// and outputs the validation result.
    /// </summary>
    /// <param name="stream">The template stream to validate.</param>
    internal static Task Validate(Stream stream) => Validate(stream, Console.Out);

    /// <summary>
    /// Validates a (modified) template of the <see cref="SystemConfiguration"/>, specified in the given stream,
    /// using the given <see cref="TextWriter"/> and outputs the validation result.
    /// </summary>
    /// <param name="stream">The template stream to validate.</param>
    /// <param name="writer">The <see cref="TextWriter"/>.</param>
    internal static async Task Validate(Stream stream, TextWriter writer)
    {
        var internalConfig = await LoadInternalConfigTemplate(stream);
        var validator = new SystemConfigurationValidator();
        var result = await validator.ValidateAsync(internalConfig.ToSharedContracts());

        await writer.WriteLineAsync(result.EntriesString);
    }

    private static SystemConfiguration CreateSharedConfigTemplate()
    {
        var ipv4Template1 = IPAddress.Parse("0.0.0.1");
        var ipv4Template2 = IPAddress.Parse("0.0.0.2");

        return new SystemConfiguration
        {
            Version = 2,
            NetworkInterfaces =
            [
                new()
                {
                    CommonInformation = new()
                    {
                        Enabled = true,
                        Name = "interface1",
                        UserDefinedMACAddress = new()
                        {
                            Enabled = true,
                            Address = PhysicalAddress.Parse("00:00:00:00:00:01")
                        }
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
                        UserDefinedMACAddress = new()
                        {
                            Enabled = true,
                            Address = PhysicalAddress.Parse("00:00:00:00:00:02")
                        }
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
            ],
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
                PrimaryDNSSuffix = new()
                {
                    Suffix = "myCompany.com"
                },
                SearchDomains = new()
                {
                    Domains = ["local", "myCompany.com"]
                }
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

    private static async Task<SystemConfig.Contracts.SystemConfiguration> LoadInternalConfigTemplate(Stream stream)
    {
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
