namespace HostManagement.Scripts;

/// <inheritdoc cref="IResourceHelper"/>
internal sealed class ResourceHelper : IResourceHelper
{
    public Stream GetResourceStream(string resourceName)
        => typeof(ResourceHelper).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("Resource stream was null.");

    public string[] GetManifestResourceNames(Func<string, bool>? predicate = null)
    {
        var resourceNames = typeof(ResourceHelper).Assembly.GetManifestResourceNames();

        if (predicate is not null)
            resourceNames = [.. resourceNames.Where(predicate)];

        return resourceNames;
    }
}
