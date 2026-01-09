namespace HostManagement;

/// <summary>
/// The main entry point for the application.
/// </summary>
internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">An array of strings containing command-line arguments.</param>
    /// <returns>Exit code of the application.</returns>
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 1)
        {
            if (args[0].Equals("--prepare-system-configuration"))
            {
                return await ServiceRunner.PrepareConfiguration();
            }
            else if (args[0].Equals("--daemon"))
            {
                return await ServiceRunner.StartService();
            }
        }

        return await CommandLineRunner.StartCommandLine(args);
    }
}
