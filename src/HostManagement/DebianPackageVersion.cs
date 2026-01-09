using System.Buffers;
using System.Text;

namespace HostManagement;

/// <summary>
/// Represents a Debian package version including epoch, upstream version and revision.
/// </summary>
/// <remarks>
/// The format is: [epoch:]upstream_version[-debian_revision]
/// </remarks>
internal sealed class DebianPackageVersion :
    IComparable,
    IComparable<DebianPackageVersion?>,
    IEquatable<DebianPackageVersion?>
{
    private const string DefaultEpoch = "0"; // Absence of epoch is equivalent to 0.
    private const string DefaultDebianRevision = "0"; // Absence of revision is equivalent to 0.

    private const char EpochSeparator = ':';
    private const char RevisionSeparator = '-';

    private static readonly SearchValues<char> s_validAsciiNumbers
        = SearchValues.Create("0123456789");

    private static readonly SearchValues<char> s_validUpstreamVersionCharactersWhenRevisionPresent
        = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-+.~");

    private static readonly SearchValues<char> s_validUpstreamVersionCharactersWhenNoRevisionPresent
        = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+.~");

    private static readonly SearchValues<char> s_validRevisionCharacters
        = s_validUpstreamVersionCharactersWhenNoRevisionPresent;

    private readonly string _epoch;
    private readonly string _upstreamVersion;
    private readonly string _debianRevision;

    /// <summary>
    /// Initializes a new <see cref="DebianPackageVersion"/>.
    /// </summary>
    /// <param name="upstreamVersion">The upstream version.</param>
    /// <param name="debianRevision">The debian revision.</param>
    public DebianPackageVersion(string upstreamVersion, string debianRevision = DefaultDebianRevision)
        : this(DefaultEpoch, upstreamVersion, debianRevision) { }

    /// <summary>
    /// Initializes a new <see cref="DebianPackageVersion"/>.
    /// </summary>
    /// <param name="epoch">The epoch.</param>
    /// <param name="upstreamVersion">The upstream version.</param>
    /// <param name="debianRevision">The debian revision.</param>
    public DebianPackageVersion(string epoch, string upstreamVersion, string debianRevision)
        : this(
              string.Concat(
                epoch,
                EpochSeparator,
                upstreamVersion,
                RevisionSeparator,
                debianRevision))
    { }

    /// <summary>
    /// Initializes a new <see cref="DebianPackageVersion"/>.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <remarks>Epoch and debian revision are optional.</remarks>
    public DebianPackageVersion(string version)
    {
        var parsedVersion = Parse(version);

        _epoch = parsedVersion.Epoch;
        _upstreamVersion = parsedVersion.UpstreamVersion;
        _debianRevision = parsedVersion.DebianRevision;
    }

    private DebianPackageVersion(
        ReadOnlySpan<char> epoch,
        ReadOnlySpan<char> upstreamVersion,
        ReadOnlySpan<char> debianRevision)
    {
        _epoch = epoch.ToString();
        _upstreamVersion = upstreamVersion.ToString();
        _debianRevision = debianRevision.ToString();
    }

    /// <summary>
    /// The epoch.
    /// </summary>
    public string Epoch => _epoch;

    /// <summary>
    /// The upstream version.
    /// </summary>
    public string UpstreamVersion => _upstreamVersion;

    /// <summary>
    /// The debian revision.
    /// </summary>
    public string DebianRevision => _debianRevision;

    /// <summary>
    /// Converts the string representation of a debian package version to an equivalent
    /// <see cref="DebianPackageVersion"/> object.
    /// </summary>
    /// <param name="version">A string that contains a debian package version to convert.</param>
    /// <returns>An object that is equivalent to the debian package version specified in the
    /// <paramref name="version"/> parameter.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static DebianPackageVersion Parse(string version)
    {
        ArgumentNullException.ThrowIfNull(version);

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException(
                $"Version '{version}' can not be empty.",
                nameof(version));
        }

        return ParseVersion(version.AsSpan());
    }

    /// <summary>
    /// Converts the specified read-only span of characters that represents a debian package
    /// version to an equivalent <see cref="DebianPackageVersion"/> object.
    /// </summary>
    /// <param name="version">A read-only span of characters that contains a debian package version
    /// to convert.</param>
    /// <returns>An object that is equivalent to the debian package version specified in the
    /// <paramref name="version"/> parameter.</returns>
    public static DebianPackageVersion Parse(ReadOnlySpan<char> version) => ParseVersion(version);

    /// <summary>
    /// Tries to convert the string representation of a debian package version to an equivalent
    /// <see cref="DebianPackageVersion"/> object, and returns a value that indicates whether the
    /// conversion succeeded.
    /// </summary>
    /// <param name="version">A string that contains a debian package version to convert.</param>
    /// <param name="parsedVersion">The <see cref="DebianPackageVersion"/> equivalent of the
    /// <paramref name="version"/>, if the conversion succeeded, otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the conversion succeeded, otherwise <see langword="false"/>.</returns>
    public static bool TryParse(string version, out DebianPackageVersion? parsedVersion)
    {
        try
        {
            parsedVersion = Parse(version);
            return true;
        }
        catch (ArgumentNullException)
        {
            parsedVersion = null;
            return false;
        }
        catch (ArgumentException)
        {
            parsedVersion = null;
            return false;
        }
    }

    public int CompareTo(object? version)
    {
        if (version == null)
            return 1;

        if (version is DebianPackageVersion debianVersion)
            return CompareTo(debianVersion);

        throw new ArgumentException($"Argument must be a {nameof(DebianPackageVersion)}.", nameof(version));
    }

    public int CompareTo(DebianPackageVersion? version)
    {
        if (ReferenceEquals(this, version))
            return 0;

        if (version is null)
            return 1;

        var epochComparison = CompareEpochs(_epoch.AsSpan(), version._epoch.AsSpan());

        if (epochComparison != 0)
            return epochComparison;

        var upstreamComparison =
            CompareVersionStrings(_upstreamVersion.AsSpan(), version._upstreamVersion.AsSpan());

        if (upstreamComparison != 0)
            return upstreamComparison;

        return CompareVersionStrings(_debianRevision.AsSpan(), version._debianRevision.AsSpan());
    }

    public override bool Equals(object? other) => Equals(other as DebianPackageVersion);

    public bool Equals(DebianPackageVersion? other)
    {
        if (ReferenceEquals(this, other))
            return true;

        return
            other is not null &&
            _epoch == other._epoch &&
            _upstreamVersion == other._upstreamVersion &&
            _debianRevision == other._debianRevision;
    }

    public override int GetHashCode()
        => HashCode.Combine(_epoch, _upstreamVersion, _debianRevision);

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        if (_epoch != DefaultEpoch)
        {
            stringBuilder
                .Append(_epoch)
                .Append(EpochSeparator);
        }

        stringBuilder.Append(_upstreamVersion);

        if (_debianRevision != DefaultDebianRevision)
        {
            stringBuilder
                .Append(RevisionSeparator)
                .Append(_debianRevision);
        }

        return stringBuilder.ToString();
    }

    public static bool operator ==(DebianPackageVersion? v1, DebianPackageVersion? v2)
    {
        if (v1 is null)
            return v2 is null;

        return v1.Equals(v2);
    }

    public static bool operator !=(DebianPackageVersion? v1, DebianPackageVersion? v2) => !(v1 == v2);

    public static bool operator <(DebianPackageVersion? v1, DebianPackageVersion? v2)
    {
        if (v1 is null)
            return v2 is not null;

        return v1.CompareTo(v2) < 0;
    }

    public static bool operator >(DebianPackageVersion? v1, DebianPackageVersion? v2) => v2 < v1;

    public static bool operator <=(DebianPackageVersion? v1, DebianPackageVersion? v2)
    {
        if (v1 is null)
            return true;

        return v1.CompareTo(v2) <= 0;
    }

    public static bool operator >=(DebianPackageVersion? v1, DebianPackageVersion? v2) => v2 <= v1;

    private static DebianPackageVersion ParseVersion(ReadOnlySpan<char> version)
    {
        var epoch = GetEpoch(version, out var epochSeparatorIndex);

        if (epoch.IndexOfAnyExcept(s_validAsciiNumbers) >= 0)
        {
            throw new FormatException(
                $"Epoch '{epoch}' of version '{version}' invalid. " +
                $"Epoch must consist of ASCII digits only (0-9).");
        }

        var debianRevision = GetDebianRevision(version, out var revisionSeparatorIndex);

        if (debianRevision.Length == 0 ||
            debianRevision.IndexOfAnyExcept(s_validRevisionCharacters) >= 0)
        {
            throw new FormatException(
                $"Debian revision '{debianRevision}' of version '{version}' invalid. " +
                $"Revision must contain only alphanumeric characters and '+', '.' or '~'.");
        }

        var upstreamVersion = GetUpstreamVersion(version, epochSeparatorIndex, revisionSeparatorIndex);

        if (upstreamVersion.Length == 0)
        {
            throw new FormatException(
                $"Upstream version '{upstreamVersion}' of version '{version}' invalid. " +
                $"Upstream version can not be empty.");
        }

        if (!char.IsAsciiDigit(upstreamVersion[0]))
        {
            // This is a recommendation, not an absolute requirement. However, deviating from this
            // recommendation can lead to issues with version comparison and package management tools.

            throw new FormatException(
                $"Upstream version '{upstreamVersion}' of version '{version}' invalid. " +
                $"Upstream version should start with a digit.");
        }

        if (revisionSeparatorIndex == -1)
        {
            if (upstreamVersion.IndexOfAnyExcept(s_validUpstreamVersionCharactersWhenNoRevisionPresent) >= 0)
            {
                throw new FormatException(
                    $"Upstream version '{upstreamVersion}' of version '{version}' invalid. " +
                    $"Upstream version must contain only alphanumeric characters and '+', '.' or '~'.");
            }
        }
        else
        {
            if (upstreamVersion.IndexOfAnyExcept(s_validUpstreamVersionCharactersWhenRevisionPresent) >= 0)
            {
                throw new FormatException(
                    $"Upstream version '{upstreamVersion}' of version '{version}' invalid. " +
                    $"Upstream version must contain only alphanumeric characters and '-', '+', '.' or '~'.");
            }
        }

        return new DebianPackageVersion(epoch, upstreamVersion, debianRevision);
    }

    private static ReadOnlySpan<char> GetEpoch(ReadOnlySpan<char> version, out int epochSeparatorIndex)
    {
        epochSeparatorIndex = version.IndexOf(EpochSeparator);

        return epochSeparatorIndex == -1
            ? DefaultEpoch.AsSpan()
            : version[..epochSeparatorIndex];
    }

    private static ReadOnlySpan<char> GetDebianRevision(
        ReadOnlySpan<char> version,
        out int revisionSeparatorIndex)
    {
        revisionSeparatorIndex = version.LastIndexOf(RevisionSeparator);

        return revisionSeparatorIndex == -1
            ? DefaultDebianRevision.AsSpan()
            : version[(revisionSeparatorIndex + 1)..];
    }

    private static ReadOnlySpan<char> GetUpstreamVersion(
        ReadOnlySpan<char> version,
        int epochSeparatorIndex,
        int revisionSeparatorIndex)
    {
        return revisionSeparatorIndex == -1
            ? version[(epochSeparatorIndex + 1)..]
            : version[(epochSeparatorIndex + 1)..revisionSeparatorIndex];
    }

    private static int CompareEpochs(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        var leftEpoch = ulong.Parse(left);
        var rightEpoch = ulong.Parse(right);

        return leftEpoch.CompareTo(rightEpoch);
    }

    private static int CompareVersionStrings(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        var indexLeft = 0;
        var indexRight = 0;

        while (indexLeft < left.Length || indexRight < right.Length)
        {
            // Extract and compare non-digit parts
            var nonDigitLeft = GetNextNonDigitPart(left, ref indexLeft);
            var nonDigitRight = GetNextNonDigitPart(right, ref indexRight);
            var nonDigitComparison = CompareNonDigitParts(nonDigitLeft, nonDigitRight);

            if (nonDigitComparison != 0)
                return nonDigitComparison;

            // Extract and compare digit parts
            var digitLeft = GetNextDigitPart(left, ref indexLeft);
            var digitRight = GetNextDigitPart(right, ref indexRight);
            var digitComparison = CompareDigitParts(digitLeft, digitRight);

            if (digitComparison != 0)
                return digitComparison;
        }

        return 0;
    }

    private static ReadOnlySpan<char> GetNextNonDigitPart(ReadOnlySpan<char> version, ref int index)
        => GetNextPart(false, version, ref index);

    private static ReadOnlySpan<char> GetNextDigitPart(ReadOnlySpan<char> version, ref int index)
        => GetNextPart(true, version, ref index);

    private static ReadOnlySpan<char> GetNextPart(
        bool shouldBeDigit,
        ReadOnlySpan<char> version,
        ref int index)
    {
        var start = index;

        while (index < version.Length && char.IsAsciiDigit(version[index]) == shouldBeDigit)
        {
            index++;
        }

        return version[start..index];
    }

    private static int CompareNonDigitParts(ReadOnlySpan<char> nonDigitPartLeft, ReadOnlySpan<char> nonDigitPartRight)
    {
        // Compares non-digit parts using modified ASCII ordering

        int i = 0, j = 0;

        while (true)
        {
            var charLeft = i < nonDigitPartLeft.Length
                ? nonDigitPartLeft[i]
                : '\0';

            var charRight = j < nonDigitPartRight.Length
                ? nonDigitPartRight[j]
                : '\0';

            if (charLeft == charRight)
            {
                if (charLeft == '\0')
                {
                    // Both strings have ended
                    return 0;
                }

                i++;
                j++;

                continue;
            }

            // Tilde '~' sorts before everything
            if (charLeft == '~') return -1;
            if (charRight == '~') return 1;

            // End of string sorts before any character except '~'
            if (charLeft == '\0') return -1;
            if (charRight == '\0') return 1;

            // Letters sort before non-letters
            var charLeftIsLetter = char.IsLetter(charLeft);
            var charRightIsLetter = char.IsLetter(charRight);

            if (charLeftIsLetter && !charRightIsLetter) return -1;
            if (!charLeftIsLetter && charRightIsLetter) return 1;

            // Both letters or both non-letters (excluding '~')
            return charLeft.CompareTo(charRight);
        }
    }

    private static int CompareDigitParts(ReadOnlySpan<char> digitLeft, ReadOnlySpan<char> digitRight)
    {
        // Remove leading zeros; empty string counts as zero
        digitLeft = digitLeft.TrimStart('0');
        digitRight = digitRight.TrimStart('0');

        if (digitLeft.Length != digitRight.Length)
        {
            return digitLeft.Length.CompareTo(digitRight.Length);
        }

        return digitLeft.CompareTo(digitRight, StringComparison.Ordinal);
    }
}
