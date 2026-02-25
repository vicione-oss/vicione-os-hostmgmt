using HostManagement.Shared.Contracts.System;

namespace HostManagement.Services;

/// <summary>
/// Validates the signature of a Debian package against one or multiple public keys.
/// </summary>
internal interface ISignedDebianPackageSignatureValidator
{
    /// <summary>
    /// Validates the signature of a signed Debian package against a list of public keys.
    /// </summary>
    /// <param name="signedDebianPackage">The signed Debian package to validate.</param>
    /// <param name="publicKeys">A list of public keys used to verify the package signature.</param>
    /// <returns>
    /// <see langword="true"/> if the signature is valid for at least one key; otherwise <see langword="false"/>.
    /// </returns>
    bool IsSignatureValid(SignedDebianPackage signedDebianPackage, List<string> publicKeys);
}
