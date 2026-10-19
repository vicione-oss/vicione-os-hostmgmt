namespace HostManagement.Communication.CommandLine.Resources;

internal static partial class CliStrings
{
    /// <summary>
    /// Provides strings for service commands.
    /// </summary>
    internal static class Service
    {
        internal const string CommandDescription = "Manage Linux systemd units (services, sockets etc.).";
        internal const string CommandName = "service";

        internal const string DisableCommandDescription = "Disable a systemd unit.";
        internal const string EnableCommandDescription = "Enable a systemd unit.";
        internal const string ListCommandDescription = "List all configurable services.";
        internal const string RestartCommandDescription = "Restart a systemd unit.";
        internal const string RestartCommandName = "restart";
        internal const string StartCommandDescription = "Start a systemd unit.";
        internal const string StartCommandName = "start";
        internal const string StatusCommandDescription = "Show status of a systemd unit.";
        internal const string StopCommandDescription = "Stop a systemd unit.";
        internal const string StopCommandName = "stop";

        internal const string AlreadyDisabled = "Systemd unit '{0}' is already disabled.";
        internal const string AlreadyEnabled = "Systemd unit '{0}' is already enabled.";
        internal const string ConfigurationNotAllowed = "Configuration of systemd unit '{0}' is not allowed.";
        internal const string Controlled = "Systemd unit '{0}' has been {1}ed.";
        internal const string Disabled = "Systemd unit '{0}' has been disabled.";
        internal const string Empty = "Systemd unit can not be empty.";
        internal const string Enabled = "Systemd unit '{0}' has been enabled.";
        internal const string OptionalDelayCanBeSpecified = "An optional delay in seconds can be specified.";
        internal const string SystemdUnits = "Systemd units";
        internal const string WillBeControlled = "Systemd unit '{0}' will be {1}ed in {2} seconds.";
    }
}
