using System.Text;

namespace HostManagement.Communication.CommandLine.Output;

/// <summary>
/// Represents the <see cref="NewLineOutput"/> which creates a new line.
/// </summary>
internal sealed class NewLineOutput : CliComplexOutputBase
{
    /// <summary>
    /// Initializes a new <see cref="NewLineOutput"/>.
    /// </summary>
    /// <param name="cliOutputs">A collection of CLI outputs.</param>
    /// <param name="stringBuilder">A <see cref="StringBuilder"/>.</param>
    public NewLineOutput(
        ICollection<CliComplexOutputBase>? cliOutputs = null,
        StringBuilder? stringBuilder = null)
        : base(cliOutputs: cliOutputs, stringBuilder: stringBuilder) { }

    protected internal override int ItemCount => 0;

    private protected override void Format() => AppendLine();
}
