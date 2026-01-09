using Microsoft.Extensions.Configuration;

namespace HostManagement.Configuration;

/// <summary>
/// Provides application configuration from environment variables.
/// </summary>
public static class AppConfiguration
{
    /// <summary>
    /// The set of key/value application configuration properties.
    /// </summary>
    /// <remarks>
    /// See <see cref="EnvironmentVariables"/> for the application environment variables.
    /// </remarks>
    public static IConfigurationRoot Configuration { get; } = ReadConfiguration();

    private static IConfigurationRoot ReadConfiguration()
    {
        try
        {
            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables();

            return builder.Build();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("App configuration could not be created.", ex);
        }
    }
}
