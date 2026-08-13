namespace HostManagement.Communication.CommandLine.Resources;

/// <summary>
/// Provides command line strings for HostManagement CLI.
/// </summary>
internal static partial class CliStrings
{
    /// <summary>
    /// Provides command line strings shared by multiple commands.
    /// </summary>
    internal static class Common
    {
        internal const string RootCommandDescription = "ViciOne Host Management CLI";

        internal const string AddCommandName = "add";
        internal const string ClearCommandName = "clear";
        internal const string DisableCommandName = "disable";
        internal const string EnableCommandName = "enable";
        internal const string ListCommandName = "list";
        internal const string ModifyCommandName = "modify";
        internal const string RemoveCommandName = "remove";
        internal const string SetCommandName = "set";
        internal const string StatusCommandName = "status";

        internal const string DisabledState = "disabled";
        internal const string EnabledLabel = "Enabled";
        internal const string EnabledState = "enabled";
        internal const string CliHelpExtractorHostManagementDescription =
            """
            # Host Management CLI Documentation - %HM_VERSION%

            `HostManagement` provides a standardized interface to read and change system settings on Debian version 12 and later.

            Currently the following features are supported:

            * Change network settings
            * Install signed Debian packages (including `ViciOne Suite`)
            * Configure systemd units
            * Execute system wide commands

            ## Usage

            1. Execute `HostManagement` in daemon mode, for example as a Linux service (`HostManagement --daemon`).
            1. Execute `HostManagement` CLI application that communicates via named pipe with previously started service (`HostManagement [command] [options]`).

            **Hint:** `HostManagement` needs privileged execution rights to be able to change system settings (e.g. change network settings).
            """;
    }
}
