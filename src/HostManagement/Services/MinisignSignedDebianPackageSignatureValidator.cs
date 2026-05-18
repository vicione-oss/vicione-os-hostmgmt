using HostManagement.Shared.Contracts.System;

namespace HostManagement.Services;

/// <summary>
/// Signature validator based on the Minisign library.
/// </summary>
internal sealed class MinisignSignedDebianPackageSignatureValidator : ISignedDebianPackageSignatureValidator
{
    /// <inheritdoc/>
    public bool IsSignatureValid(SignedDebianPackage signedDebianPackage, IReadOnlyCollection<string> publicKeys)
    {
        var signature = Minisign.Core.LoadSignatureFromFile(signedDebianPackage.SignatureFilePath);

        foreach (var key in publicKeys)
        {
            var publicKey = Minisign.Core.LoadPublicKeyFromString(key);

            if (Minisign.Core.ValidateSignature(signedDebianPackage.FilePath, signature, publicKey))
                return true;
        }

        return false;
    }
}
