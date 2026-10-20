using System.Net;
using System.Text.Json.Serialization;

namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the synchronization status of the system clock with an NTP server.
/// </summary>
public sealed record NTPSyncStatus
{
    /// <summary>
    /// The IP address of the used NTP server.
    /// </summary>
    [JsonConverter(typeof(IPAddressConverter))]
    public required IPAddress ServerAddress { get; init; }

    /// <summary>
    /// The name of the used NTP server.
    /// </summary>
    /// <remarks>
    /// If no name is available, contains the string representation of the <see cref="ServerAddress"/>.
    /// </remarks>
    public required string ServerName { get; init; }

    /// <summary>
    /// Indicates whether the system clock has been synchronized.
    /// </summary>
    public required bool Synchronized { get; init; }

    /// <summary>
    /// Represents the 16-level hierarchy, that indicates how many synchronization "hops" a clock
    /// is away from a primary reference clock.
    /// </summary>
    /// <remarks>
    /// The lower the number, the closer is the reference clock and the higher is the accuracy.
    /// <list type="bullet">
    /// <item>
    /// 0: Actual reference source, such as an atomic clock or GPS receiver.
    /// [Accuracy: ~1 ns - 1 µs] 
    /// </item>
    /// <item>
    /// 1: A primary NTP server, directly synchronized to a stratum-0 reference clock.
    /// [Accuracy: ~1 µs - 10 µs]
    /// </item>
    /// <item>
    /// 2: A secondary NTP server, synchronized to one or more stratum-1 servers.
    /// [Accuracy: ~100 µs - 1 ms]
    /// </item>
    /// <item>
    /// 3: Enterprise NTP servers, Active Directory etc., synchronized via stratum-2.
    /// [Accuracy: ~1 ms - 10 ms]
    /// </item>
    /// <item>
    /// 4: Routers, workstations, IoT etc., synchronized via stratum-3.
    /// [Accuracy: ~5 ms - 20 ms]
    /// </item>
    /// <item>
    /// 5-15: Very large networks
    /// [Accuracy: > 20 ms]
    /// </item>
    /// <item>
    /// 16: Unsynchronized
    /// </item>
    /// </list>
    /// </remarks>
    public required byte Stratum { get; init; }

    /// <summary>
    /// A simplified quality classification of the <see cref="Stratum"/> value.
    /// <list type="bullet">
    /// <item>Stratum 0: "Unknown"</item>
    /// <item>Stratum 1, 2: "High"</item>
    /// <item>Stratum 3, 4: "Medium"</item>
    /// <item>Stratum >5: "Low"</item>
    /// </list>
    /// </summary>
    public required string StratumQuality { get; init; }

    /// <summary>
    /// The absolute time difference (in ms) between the local system clock and the NTP server.
    /// </summary>
    public required float Offset { get; init; }

    /// <summary>
    /// A simplified quality classification of the <see cref="Offset"/> value.
    /// <list type="bullet">
    /// <item>&lt; 1 ms: "Excellent"</item>
    /// <item>&lt; 10 ms: "Good"</item>
    /// <item>&lt; 100 ms: "Fair"</item>
    /// <item>&gt; 100 ms: "Poor"</item>
    /// </list>
    /// </summary>
    public required string OffsetQuality { get; init; }

    /// <summary>
    /// The statistical variance of the network latency (in ms) over time.
    /// High values indicate an unstable network connection.
    /// </summary>
    public required float Jitter { get; init; }

    /// <summary>
    /// A simplified quality classification of the <see cref="Jitter"/> value.
    /// <list type="bullet">
    /// <item>&lt; 1 ms: "Excellent"</item>
    /// <item>&lt; 5 ms: "Good"</item>
    /// <item>&lt; 20 ms: "Fair"</item>
    /// <item>&gt; 20 ms: "Poor"</item>
    /// </list>
    /// </summary>
    public required string JitterQuality { get; init; }

    /// <summary>
    /// Contains a message, if synchronization failed.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
