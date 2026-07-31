namespace HostManagement.SystemConfig.Contracts;

internal sealed record VLANSettings
{
    public bool? Enabled { get; set; }
    public int? ID { get; set; }
}
