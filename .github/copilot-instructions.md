# Copilot Custom Instructions

These are the global instructions for the `HostManagement` project.

Additional instructions are located in the `.github\instructions` folder. Use [Unit Testing Instructions] when applicable and look for potential user defined instructions (`user-defined.instructions.md`).

## Project and Application

The project `HostManagement` is an AOT compiled .NET C# application and runs only on Linux (Debian 12). It provides a uniform interface to configure the underlying system and enables communication with higher-level applications via named pipe.

## Code

- Code shall adhere to the rules in [`.editorconfig`] if not otherwise stated
- Class names, method names etc. MUST be written in **English**
- Code MUST adhere to the following ordering scheme according to [StyleCop Ordering Rules]

  | Identifier | Rule description                                           |
  |------------|------------------------------------------------------------|
  | [SA1201]   | Elements must appear in the correct order                  |
  | [SA1202]   | Elements must be ordered by access                         |
  | [SA1203]   | Constants must appear before fields                        |
  | [SA1204]   | Static elements must appear before instance elements       |
  | [SA1206]   | Declaration keywords must follow order                     |
  | [SA1214]   | Readonly elements must appear before non-readonly elements |

- When multiple StyleCop ordering rules apply, use StyleCop's precedence order. In particular, access modifier ordering (`SA1202`) takes precedence over static/instance ordering (`SA1204`) and constant/field ordering (`SA1203`). For example, a `public` instance member may appear before a `private static` member of the same element kind.

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
- XML comments are NOT REQUIRED for unit tests
- Non-XML comments (inline code comments) MAY be in **German** to explain complex technical concepts.

[`.editorconfig`]: ../.editorconfig
[SA1201]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1201.md
[SA1202]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1202.md
[SA1203]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1203.md
[SA1204]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1204.md
[SA1206]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1206.md
[SA1214]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1214.md
[StyleCop Ordering Rules]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/OrderingRules.md
[Unit Testing Instructions]: instructions/testing.instructions.md
