using HostManagement.Configuration;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Service;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the abstract base class for a service command.
/// </summary>
internal abstract class ServiceCommandBase(string name, string description) : ConfigCommandBase(name, description)
{
    private protected static ServiceDetail? GetServiceDetailToConfigure(
        SystemConfiguration config,
        string systemdUnitName)
    {
        var normalizedUnitName = GetSystemdUnitWithUnitType(systemdUnitName);

        return config.Services.FirstOrDefault(service => service.Name.Equals(normalizedUnitName));
    }

    private protected static string GetSystemdUnitWithUnitType(string systemdUnitName)
    {
        // Ensure the provided service name has a unit type
        return SystemdUnits.KnownUnitTypes.Any(systemdUnitName.EndsWith)
            ? systemdUnitName
            : systemdUnitName + ".service";
    }
}
