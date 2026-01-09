namespace HostManagement.Scripts;

/// <inheritdoc cref="IResourceHelper"/>
internal sealed class ResourceHelper : IResourceHelper
{
    public Stream GetResourceStream(string resourceName)
        => typeof(ResourceHelper).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("Resource stream was null.");

    public string[] GetManifestResourceNames()
        => typeof(ResourceHelper).Assembly.GetManifestResourceNames();
}
