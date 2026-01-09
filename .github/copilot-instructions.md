# Copilot Custom Instructions

These are the global instructions for the `HostManagement` project.

## Project and Application

The project `HostManagement` is an AOT compiled .NET C# application and runs only on Linux (Debian 12). It provides a uniform interface to configure the underlying system and enables communication with higher-level applications via named pipe.

## Code

- Code shall adhere to the rules in [`.editorconfig`] if not otherwise stated
- Class names, method names etc. MUST be written in **English**

## Documentation

- XML comments MUST be written in **English**
- XML comments are REQUIRED for public and internal types and members
  - Check if they match the code and logic, e.g. parameter descriptions, return types etc.
- XML Comments that are already defined in an interface, when implementing this interface, are NOT REQUIRED
  - Use the `<inheritdoc/>` tag on the class instead (also check that the interface has all required XML comments)

    **Example:**

    ```csharp
    /// <summary>
    /// Represents the script handler.
    /// </summary>
    internal interface IScriptHandler
    {
        /// <summary>
        /// Executes the script to apply the netplan configuration.
        /// </summary>
        /// <returns><see cref="OperationStatus"/></returns>
        Task<OperationStatus> ExecuteApplyNetplanConfigurationScript();
    }
    ```

    ```csharp
    /// <inheritdoc cref="IScriptHandler"/>
    /// <param name="result">The result of the operation.</param>
    internal sealed class ScriptHandler(Result result) : IScriptHandler
    {
        public async Task<OperationStatus> ExecuteApplyNetplanConfigurationScript()
        {
            // Content of method here...
        }
    }
    ```

  - Additional Comments SHOULD be added, if needed (e.g. for exceptions), by using the `<inheritdoc/>` tag on the method itself

    **Example:**

    ```csharp
    /// <inheritdoc/>
    /// <exception cref="IOException">Thrown when the operation fails.</exception>
    public async Task DoSomething(CancellationToken cancellationToken = default)
    {
        // Content of method here...
    }
    ```

- XML comments are OPTIONAL for private types and members (when present, they MUST be accurate and up-to-date)
- Non-XML comments (inline code comments) MAY be in **German** to explain complex technical concepts.

[`.editorconfig`]: ../.editorconfig
