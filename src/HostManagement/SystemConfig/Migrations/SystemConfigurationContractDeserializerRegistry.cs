using System.Diagnostics.CodeAnalysis;
using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig.Migrations;

/// <summary>
/// Provides lookup functionality for system configuration contract deserializers by contract version.
/// </summary>
/// <param name="deserializers">The available system configuration contract deserializers.</param>
internal sealed class SystemConfigurationContractDeserializerRegistry(
    IEnumerable<ISystemConfigurationContractDeserializer> deserializers)
{
    private readonly IReadOnlyDictionary<int, ISystemConfigurationContractDeserializer> _deserializers =
        CreateDeserializerDictionary(deserializers);

    /// <summary>
    /// Tries to get the deserializer for the specified contract version.
    /// </summary>
    /// <param name="version">The contract version.</param>
    /// <param name="deserializer">The matching deserializer, if one is registered.</param>
    /// <returns><see langword="true"/> when a matching deserializer was found; otherwise <see langword="false"/>.</returns>
    internal bool TryGetDeserializer(
        int version,
        [NotNullWhen(true)] out ISystemConfigurationContractDeserializer? deserializer)
        => _deserializers.TryGetValue(version, out deserializer);

    private static IReadOnlyDictionary<int, ISystemConfigurationContractDeserializer> CreateDeserializerDictionary(
        IEnumerable<ISystemConfigurationContractDeserializer> deserializers)
    {
        var deserializerList = deserializers.ToList();

        ValidateDeserializerVersions(deserializerList);
        ValidateUniqueVersions(deserializerList);

        return deserializerList.ToDictionary(static deserializer => deserializer.Version);
    }

    private static void ValidateDeserializerVersions(IEnumerable<ISystemConfigurationContractDeserializer> deserializers)
    {
        foreach (var deserializer in deserializers)
        {
            if (deserializer.Version < 1)
            {
                throw new InvalidOperationException(
                    $"System configuration deserializer '{deserializer.GetType().Name}' has invalid version " +
                    $"'{deserializer.Version}'.");
            }

            if (deserializer.Version > SystemConfiguration.CurrentVersion)
            {
                throw new InvalidOperationException(
                    $"System configuration deserializer '{deserializer.GetType().Name}' targets version " +
                    $"'{deserializer.Version}', which is newer than the current supported version " +
                    $"'{SystemConfiguration.CurrentVersion}'.");
            }
        }
    }

    private static void ValidateUniqueVersions(IEnumerable<ISystemConfigurationContractDeserializer> deserializers)
    {
        var duplicateVersions = deserializers
            .GroupBy(static deserializer => deserializer.Version)
            .Where(static group => group.Count() > 1)
            .Select(static group => group.Key)
            .ToList();

        if (duplicateVersions.Count > 0)
        {
            throw new InvalidOperationException(
                $"Multiple system configuration deserializers are registered for version " +
                $"'{string.Join(", ", duplicateVersions)}'.");
        }
    }
}
