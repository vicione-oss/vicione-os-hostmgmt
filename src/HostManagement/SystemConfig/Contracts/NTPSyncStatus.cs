using System.Net;
using System.Text.Json.Serialization;

namespace HostManagement.SystemConfig.Contracts;

internal sealed record NTPSyncStatus
{
    [JsonIgnore]
    public static NTPSyncStatus Unknown { get; } = new NTPSyncStatus()
    {
        ServerAddress = IPAddress.None,
        ServerName = string.Empty,
        Synchronized = false,
        Stratum = 16,
        StratumQuality = "Unknown",
        Offset = 0f,
        OffsetQuality = "Unknown",
        Jitter = 0f,
        JitterQuality = "Unknown",
        Message = string.Empty
    };

    [JsonConverter(typeof(IPAddressConverter))]
    public required IPAddress ServerAddress { get; set; }
    public required string ServerName { get; set; }
    public required bool Synchronized { get; set; }
    public required byte Stratum { get; set; }
    public required string StratumQuality { get; set; }
    public required float Offset { get; set; }
    public required string OffsetQuality { get; set; }
    public required float Jitter { get; set; }
    public required string JitterQuality { get; set; }
    public string? Message { get; set; }
}
