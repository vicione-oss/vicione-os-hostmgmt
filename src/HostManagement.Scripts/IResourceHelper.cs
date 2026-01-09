namespace HostManagement.Scripts;

/// <summary>
/// Represents a resource helper. 
/// </summary>
public interface IResourceHelper
{
    /// <summary>
    /// Loads the specified resource.
    /// </summary>
    /// <param name="resourceName">The resource name.</param>
    /// <returns><see cref="Stream"/></returns>
    Stream GetResourceStream(string resourceName);

    /// <summary>
    /// Gets all the names of all the resources.
    /// </summary>
    /// <returns></returns>
    string[] GetManifestResourceNames();
}
