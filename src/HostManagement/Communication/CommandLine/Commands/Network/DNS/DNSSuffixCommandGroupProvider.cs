using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the <see cref="DNSSuffixCommandGroupProvider"/>.
/// </summary>
internal static class DNSSuffixCommandGroupProvider
{
    private static readonly Option<string> s_suffixOption = new(["-s", "--suffix"])
    {
        Description = "The primary DNS suffix.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Command s_statusCommand = CommandHelper.ConstructCommand(
        "status",
        "Show primary DNS suffix status.",
        StatusHandler);

    private static readonly Command s_enableCommand = CommandHelper.ConstructCommand(
        "enable",
        "Enable the primary DNS Suffix.",
        () => EnableDisableHandler(enable: true));

    private static readonly Command s_disableCommand = CommandHelper.ConstructCommand(
        "disable",
        "Disable the primary DNS Suffix.",
        () => EnableDisableHandler(enable: false));

    private static readonly Command s_setCommand = CommandHelper.ConstructCommand(
        "set",
        "Remove the existing primary DNS suffix and set a new one.",
        s_suffixOption,
        SetHandler);

    private static readonly Command s_removeCommand = CommandHelper.ConstructCommand(
        "remove",
        "Remove the primary DNS suffix.",
        RemoveHandler);

    /// <summary>
    /// Adds the command group to configure the primary DNS suffix.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddDNSSuffixCommandGroup(this Command parentCommand)
    {
        var dnsSuffixCommand = parentCommand.AddCommand(
            "dns-suffix",
            "Configure the primary DNS suffix.");

        dnsSuffixCommand.AddCommand(s_statusCommand);
        dnsSuffixCommand.AddCommand(s_enableCommand);
        dnsSuffixCommand.AddCommand(s_disableCommand);
        dnsSuffixCommand.AddCommand(s_setCommand);
        dnsSuffixCommand.AddCommand(s_removeCommand);
    }

    private static async Task StatusHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (string.IsNullOrWhiteSpace(config.NetworkDNSSettings.DNSSuffix))
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings.DNSSuffixNotConfigured);

            return;
        }

        new ListOutput()
            .WithTitle(CommandLineStrings.DNSSuffix, config.NetworkDNSSettings.DNSSuffixEnabled)
            .With(config.NetworkDNSSettings.DNSSuffix)
            .WriteLine();
    }

    private static async Task EnableDisableHandler(bool enable)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.DNSSuffixEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings.DNSSuffixAlreadyEnabled
                : CommandLineStrings.DNSSuffixAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info);

            return;
        }

        if (enable &&
            string.IsNullOrEmpty(config.NetworkDNSSettings.DNSSuffix))
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.DNSSuffixNotConfigured);

            return;
        }

        config.NetworkDNSSettings.DNSSuffixEnabled = enable;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings.DNSSuffixEnabled
            : CommandLineStrings.DNSSuffixDisabled;

        SimpleOutput.WriteLine(enableMessage);
    }

    private static async Task SetHandler(string dnsSuffix)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.DNSSuffix = dnsSuffix;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        var output = new ListOutput();
        output.WithTitle(CommandLineStrings.DNSSuffixSet);
        output.With(dnsSuffix);

        if (!config.NetworkDNSSettings.DNSSuffixEnabled)
            output.WithWarning(CommandLineStrings.DNSSuffixCurrentlyDisabled);

        output.WriteLine();
    }

    private static async Task RemoveHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var dnsSuffix = config.NetworkDNSSettings.DNSSuffix;

        var output = new ListOutput();

        if (string.IsNullOrWhiteSpace(dnsSuffix))
        {
            output
                .WithWarning(CommandLineStrings.DNSSuffixNotConfigured)
                .WriteLine();

            return;
        }

        config.NetworkDNSSettings.DNSSuffix = string.Empty;
        output.With(dnsSuffix);

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        output
            .WithTitle(CommandLineStrings.DNSSuffixRemoved)
            .WriteLine();
    }
}
