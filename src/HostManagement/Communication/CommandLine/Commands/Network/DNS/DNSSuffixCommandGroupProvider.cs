using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the <see cref="DNSSuffixCommandGroupProvider"/>.
/// </summary>
internal static class DNSSuffixCommandGroupProvider
{
    private static readonly Option<string> s_dnsSuffixOption = new(["-s", "--set"])
    {
        Description = "Sets the primary DNS suffix (overwriting the existing one).",
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Command s_enableCommand = CommandHelper.ConstructCommand(
        "enable",
        "Enable the primary DNS Suffix.",
        () => EnableDisableHandler(enable: true));

    private static readonly Command s_disableCommand = CommandHelper.ConstructCommand(
        "disable",
        "Disable the primary DNS Suffix.",
        () => EnableDisableHandler(enable: false));

    private static readonly Command s_dnsSuffixCommand = CommandHelper.ConstructCommand(
        "dns-suffix",
        "Configure the primary DNS suffix.",
        s_dnsSuffixOption,
        Handler);

    /// <summary>
    /// Adds the command group to configure the primary DNS suffix.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddDNSSuffixCommandGroup(this Command parentCommand)
    {
        s_dnsSuffixCommand.AddCommand(s_enableCommand);
        s_dnsSuffixCommand.AddCommand(s_disableCommand);

        parentCommand.AddCommand(s_dnsSuffixCommand);
    }

    private static async Task EnableDisableHandler(bool enable)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.DNSSuffixEnabled = enable;

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            enable
            ? CommandLineStrings.DNSSuffixEnabled
            : CommandLineStrings.DNSSuffixDisabled);
    }

    private static async Task Handler(string dnsSuffix)
    {
        if (string.IsNullOrWhiteSpace(dnsSuffix))
        {
            if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
                return;

            if (string.IsNullOrWhiteSpace(config.NetworkDNSSettings.DNSSuffix))
                Console.WriteLine(CommandLineStrings.DNSSuffixNotSet);
            else
                Console.WriteLine(config.NetworkDNSSettings.DNSSuffix);
        }
        else
        {
            if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
                return;

            config.NetworkDNSSettings.DNSSuffix = dnsSuffix;

            if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
                return;

            Console.WriteLine(CommandLineStrings.DNSSuffixSet, dnsSuffix);
        }
    }
}
