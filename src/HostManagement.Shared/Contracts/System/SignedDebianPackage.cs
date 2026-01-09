namespace HostManagement.Shared.Contracts.System;

/// <summary>
/// Represents a Debian package with signature.
/// </summary>
public sealed record SignedDebianPackage
{
    /// <summary>
    /// The package file path.
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// The signature file path.
    /// </summary>
    public required string SignatureFilePath { get; set; }
}
