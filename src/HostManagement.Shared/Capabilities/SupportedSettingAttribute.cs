namespace HostManagement.Shared.Capabilities;

/// <summary>
/// Represents the attribute to specify that a property is a supported setting.
/// </summary>
/// <remarks>
/// If a property is marked with this attribute, it will be part of the <c>SupportedSettings</c> object
/// and a record will be created for it.
/// If it is applied to a collection property and the collection element type contains supported settings,
/// the generated hierarchy contains an intermediate setting for the collection element type.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class SupportedSettingAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the attribute using the property name as setting name.
    /// </summary>
    public SupportedSettingAttribute() { }

    /// <summary>
    /// Initializes a new instance of the attribute with an explicit setting name.
    /// </summary>
    /// <param name="name">The explicit setting name (display name).</param>
    public SupportedSettingAttribute(string name) => Name = name;

    /// <summary>
    /// The optional display name override. If <see langword="null"/>, the property name is used.
    /// </summary>
    public string? Name { get; }
}
