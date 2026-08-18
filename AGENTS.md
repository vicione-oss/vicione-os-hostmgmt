# Agent Instructions

These are the global instructions for the `HostManagement` project.

Additional instructions are located in the `src` and `tests` folder. Use scoped instructions only when applicable.

## Project and Application

The project `HostManagement` is an AOT compiled .NET C# application and runs only on Linux (Debian 12). It provides a uniform interface to configure the underlying system and enables communication with higher-level applications via named pipe.

## Instruction Scope

- Keep this file minimal and generic.
- Put detailed, domain-specific, or file-type-specific rules into directory-scoped `AGENTS.md` files in `src` and `tests`.
- Apply [Unit Testing Instructions] when creating or changing unit tests.
- Apply [Production C# Instructions] when creating or changing production C# code.

[Unit Testing Instructions]: tests/AGENTS.md
[Production C# Instructions]: src/AGENTS.md
