using System.Text.Json;
using HostManagement.Services;
using HostManagement.Shared.Capabilities;
using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Communication.CommandLine;

internal class SupportedCapabilitiesTemplate
{
    private static TextWriter s_output = Console.Out;

    /// <summary>
    /// Sets the <see cref="TextWriter"/>. The default is console output.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/>.</param>
    internal static void SetOutput(TextWriter writer) => s_output = writer;

    /// <summary>
    /// Creates and outputs a template of the <see cref="SupportedCapabilities"/> with all settings and topics disabled.
    /// </summary>
    internal static void CreateAndOutput()
    {
        var supportedCapabilitiesJson = JsonSerializer.Serialize(
            new SupportedCapabilities(),
            SourceGenerationContext.Default.SupportedCapabilities);

        using var document = JsonDocument.Parse(supportedCapabilitiesJson);

        var iniContent = SupportedCapabilitiesIniConverter.ToTemplateIni(document);

        s_output.WriteLine(iniContent);
    }
}
