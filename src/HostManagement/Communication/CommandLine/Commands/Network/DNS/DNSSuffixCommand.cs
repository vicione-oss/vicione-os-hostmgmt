using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the primary DNS suffix command.
/// </summary>
internal sealed class DNSSuffixCommand : ConfigCommandBase
{
    private static readonly Option<string> s_suffixOption = new(["-s", "--suffix"])
    {
        Description = "The primary DNS suffix.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="DNSSuffixCommand"/>.
    /// </summary>
    public DNSSuffixCommand() : base(
        CommandLineStrings_DNS.DNSSuffixCommandName,
        CommandLineStrings_DNS.DNSSuffixCommandDescription)
    {
        Add(
            CommandLineStrings_Common.EnableCommandName,
            CommandLineStrings_DNS.DNSSuffixEnableCommandDescription,
            () => EnableDisableHandler(enable: true));

        Add(
            CommandLineStrings_Common.DisableCommandName,
            CommandLineStrings_DNS.DNSSuffixDisableCommandDescription,
            () => EnableDisableHandler(enable: false));

        Add(
            CommandLineStrings_Common.StatusCommandName,
            CommandLineStrings_DNS.DNSSuffixStatusCommandDescription,
            StatusHandler);

        Add(
            CommandLineStrings_Common.SetCommandName,
            CommandLineStrings_DNS.DNSSuffixSetCommandDescription,
            s_suffixOption,
            SetHandler);

        Add(
            CommandLineStrings_Common.RemoveCommandName,
            CommandLineStrings_DNS.DNSSuffixRemoveCommandDescription,
            RemoveHandler);
    }

    private static async Task StatusHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (string.IsNullOrWhiteSpace(config.NetworkDNSSettings.DNSSuffix))
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.SuffixNotConfigured);

            return;
        }

        new ListOutput()
            .WithTitle(CommandLineStrings_DNS.Suffix, config.NetworkDNSSettings.DNSSuffixEnabled)
            .With(config.NetworkDNSSettings.DNSSuffix)
            .WriteLine();
    }

    private static async Task EnableDisableHandler(bool enable)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.DNSSuffixEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings_DNS.SuffixAlreadyEnabled
                : CommandLineStrings_DNS.SuffixAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info);

            return;
        }

        if (enable &&
            string.IsNullOrEmpty(config.NetworkDNSSettings.DNSSuffix))
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_DNS.SuffixNotConfigured);

            return;
        }

        config.NetworkDNSSettings.DNSSuffixEnabled = enable;

        if (!await SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings_DNS.SuffixEnabled
            : CommandLineStrings_DNS.SuffixDisabled;

        SimpleOutput.WriteLine(enableMessage);
    }

    private static async Task SetHandler(string dnsSuffix)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.DNSSuffix = dnsSuffix;

        if (!await SetConfiguration(config))
            return;

        var output = new ListOutput();
        output.WithTitle(CommandLineStrings_DNS.SuffixSet);
        output.With(dnsSuffix);

        if (!config.NetworkDNSSettings.DNSSuffixEnabled)
            output.WithWarning(CommandLineStrings_DNS.SuffixCurrentlyDisabled);

        output.WriteLine();
    }

    private static async Task RemoveHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var dnsSuffix = config.NetworkDNSSettings.DNSSuffix;

        var output = new ListOutput();

        if (string.IsNullOrWhiteSpace(dnsSuffix))
        {
            output
                .WithWarning(CommandLineStrings_DNS.SuffixNotConfigured)
                .WriteLine();

            return;
        }

        config.NetworkDNSSettings.DNSSuffix = string.Empty;
        config.NetworkDNSSettings.DNSSuffixEnabled = false;

        output.With(dnsSuffix);

        if (!await SetConfiguration(config))
            return;

        output
            .WithTitle(CommandLineStrings_DNS.SuffixRemoved)
            .WriteLine();
    }
}
