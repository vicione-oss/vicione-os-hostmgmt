namespace HostManagement.Shared;

/// <summary>
/// Represents the attribute to specify that a topic requires content passed along with a named pipe request.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class RequiresContentAttribute : Attribute;
