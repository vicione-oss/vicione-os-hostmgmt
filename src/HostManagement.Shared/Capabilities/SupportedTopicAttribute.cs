namespace HostManagement.Shared.Communication.Capabilities;

/// <summary>
/// Represents the attribute to specify that a field is a supported topic.
/// </summary>
/// <remarks>
/// If a field is marked with this attribute, it will be part of the <c>SupportedTopics</c> object
/// and a record will be created for it.
/// </remarks>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class SupportedTopicAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the attribute using the field name as topic name.
    /// </summary>
    public SupportedTopicAttribute() { }

    /// <summary>
    /// Initializes a new instance of the attribute with an explicit topic name.
    /// </summary>
    /// <param name="name">The explicit topic name (display name).</param>
    public SupportedTopicAttribute(string name) => Name = name;

    /// <summary>
    /// The optional display name override. If <see langword="null"/>, the field name is used.
    /// </summary>
    public string? Name { get; }
}
