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
    /// Gets all the names of all the resources. Can be filtered by a predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>An array of resource names</returns>
    string[] GetManifestResourceNames(Func<string, bool>? predicate = null);
}
