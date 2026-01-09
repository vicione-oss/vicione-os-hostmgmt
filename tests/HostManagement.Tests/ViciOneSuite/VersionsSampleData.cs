namespace HostManagement.Tests.ViciOneSuite;

internal static class VersionsSampleData
{
    public const string HostManagementVersion = "0.6.0";

    public const string VersionsCsvContent =
        """
        HostManagement Version,Suite Debian Package Version
        0.5.0,vicione-suite_0.13.0_arm64.deb
        0.5.0,vicione-suite_0.13.0_amd64.deb
        0.5.0,vicione-suite_0.14.0~1000001_arm64.deb
        0.5.0,vicione-suite_0.14.0~1000001_amd64.deb
        0.5.0,vicione-suite_0.14.0_arm64.deb
        0.5.0,vicione-suite_0.14.0_amd64.deb
        0.6.0,vicione-suite_0.15.0~1000033_arm64.deb
        0.6.0,vicione-suite_0.15.0~1000033_amd64.deb
        0.6.0,vicione-suite_0.16.0~1000099_arm64.deb
        0.6.0,vicione-suite_0.16.0~1000101_amd64.deb
        0.6.0,vicione-suite_0.16.0_arm64.deb
        0.6.0,vicione-suite_0.16.0_amd64.deb
        """;

    public static readonly Dictionary<string, IList<string>> VersionMapping = new()
    {
        {
            "0.5.0",
            new List<string>
            {
                "0.13.0_arm64.deb",
                "0.13.0_amd64.deb",
                "0.14.0~1000001_arm64.deb",
                "0.14.0~1000001_amd64.deb",
                "0.14.0_arm64.deb",
                "0.14.0_amd64.deb"
            }
        },
        {
            "0.6.0",
            new List<string>
            {
                "0.15.0~1000033_arm64.deb",
                "0.15.0~1000033_amd64.deb",
                "0.16.0~1000099_arm64.deb",
                "0.16.0~1000101_amd64.deb",
                "0.16.0_arm64.deb",
                "0.16.0_amd64.deb"
            }
        }
    };

    public static readonly IEnumerable<string> CompatibleVersions =
    [
        "0.15.0~1000033_arm64.deb",
        "0.15.0~1000033_amd64.deb",
        "0.16.0~1000099_arm64.deb",
        "0.16.0~1000101_amd64.deb",
        "0.16.0_arm64.deb",
        "0.16.0_amd64.deb"
    ];

    public static readonly IEnumerable<string> CompatibleArm64Versions =
    [
        "0.16.0~1000099",
        "0.16.0"
    ];

    public static readonly IEnumerable<string> CompatibleArm64ReleaseVersions =
    [
        "0.16.0"
    ];

    public static readonly IEnumerable<string> CompatibleArm64CIVersions =
    [
        "0.16.0~1000099"
    ];
}
