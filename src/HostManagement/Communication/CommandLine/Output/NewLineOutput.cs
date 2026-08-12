using System.Text;

namespace HostManagement.Communication.CommandLine.Output;

/// <summary>
/// Represents the <see cref="NewLineOutput"/> which creates a new line.
/// </summary>
/// <param name="cliOutputs">A collection of CLI outputs.</param>
/// <param name="stringBuilder">A <see cref="StringBuilder"/>.</param>
internal sealed class NewLineOutput(
    ICollection<CliComplexOutputBase>? cliOutputs = null,
    StringBuilder? stringBuilder = null)
        : CliComplexOutputBase(cliOutputs: cliOutputs, stringBuilder: stringBuilder)
{
    protected internal override int ItemCount => 0;

    private protected override void Format() => AppendLine();
}
