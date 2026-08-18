# Production C# Instructions

These instructions apply when creating or modifying production C# code.

## Code

- Code shall adhere to the rules in [`.editorconfig`] if not otherwise stated.
- Class names, method names etc. MUST be written in **English**.
- Code MUST adhere to the following ordering scheme according to [StyleCop Ordering Rules].

  | Identifier | Rule description                                           |
  |------------|------------------------------------------------------------|
  | [SA1201]   | Elements must appear in the correct order                  |
  | [SA1202]   | Elements must be ordered by access                         |
  | [SA1203]   | Constants must appear before fields                        |
  | [SA1204]   | Static elements must appear before instance elements       |
  | [SA1206]   | Declaration keywords must follow order                     |
  | [SA1214]   | Readonly elements must appear before non-readonly elements |

- When multiple StyleCop ordering rules apply, use StyleCop's precedence order.
- In particular, access modifier ordering ([SA1202]) takes precedence over static/instance ordering ([SA1204]) and constant/field ordering ([SA1203]).

## Documentation

- XML comments MUST be written in **English**.
- XML comments are REQUIRED for public and internal types and members.
- Check that XML comments match the code and logic (for example parameter descriptions and return values).
- XML comments are OPTIONAL for private types and members (when present, they MUST be accurate and up-to-date).
- XML comments that are already defined in an interface are NOT REQUIRED in the implementing type.
- Use `<inheritdoc/>` on the implementing class instead and ensure the interface has all required XML comments.
- Additional comments SHOULD be added if needed (for example exceptions), by using `<inheritdoc/>` on the method.
- Non-XML comments (inline code comments) MAY be in **German** to explain complex technical concepts.

[`.editorconfig`]: ../.editorconfig
[SA1201]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1201.md
[SA1202]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1202.md
[SA1203]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1203.md
[SA1204]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1204.md
[SA1206]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1206.md
[SA1214]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1214.md
[StyleCop Ordering Rules]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/OrderingRules.md
