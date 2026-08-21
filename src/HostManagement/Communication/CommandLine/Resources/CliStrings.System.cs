namespace HostManagement.Communication.CommandLine.Resources;

internal static partial class CliStrings
{
    /// <summary>
    /// Provides strings for system commands.
    /// </summary>
    internal static class System
    {
        internal const string CommandDescription = "Execute system wide commands.";
        internal const string CommandName = "system";

        internal const string InstallPackageCommandDescription = "Installs a signed Debian package.";
        internal const string ResetCommandDescription = "Reset the system.";
        internal const string ResetCommandName = "reset";
        internal const string RestartCommandDescription = "Restart the system.";
        internal const string RestartCommandName = "restart";
        internal const string ShutdownCommandDescription = "Shutdown the system.";
        internal const string ShutdownCommandName = "shutdown";
        internal const string UpdateCommandDescription = "Update the system.";
        internal const string UpdateCommandName = "update";

        internal const string DoYouWantToShutdownTheSystem = "Do you want to shutdown the system? [y/N] ";
        internal const string InstallPackageCommandName = "install";
        internal const string InstallPackageSuccessful = "Signed Debian package installed successfully ('{0}').";
        internal const string OptionalDelayCanBeSpecified = "An optional delay in seconds can be specified.";
        internal const string ResetFailed = "System could not be reset.";
        internal const string SystemCanNotRestartRemotely = "The system can not be restarted remotely, manual power cycling is necessary!";
        internal const string UpdateFailed = "System could not be updated.";
    }
}
