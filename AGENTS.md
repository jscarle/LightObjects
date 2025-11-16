# Repository Guidelines for AI Contributors

This repository contains C# source code for **LightObjects**, a lightweight value object framework with a source generator for strongly typed identifiers. The code targets high performance scenarios and is used in paths that handle millions of requests per second. Changes should prioritize runtime efficiency and minimal memory allocations.

## Structure

- `src/LightObjects` – Interfaces and helpers for value objects.
  - `IValueObject` and related interfaces define the contract for value objects.
  - `TypeExtensions` provides helpers to detect value object types at runtime using reflection.
  - `ValueObjectException` is used to throw when creation or parsing fails.
- `src/LightObjects.Generated` – Roslyn source generator that creates strongly typed identifier structs or classes when decorated with `[GeneratedIdentifier<T>]`.
  - `Common/` contains small utility types such as `Declaration`, `SymbolExtensions`, and `EquatableImmutableArray` used by the generator.
  - `GeneratedIdentifierSourceGenerator.cs` implements the source generation logic. The generated code includes factory methods (`Create`, `TryCreate`, `Parse`, `TryParse`), equality, comparisons, type converters and JSON converters.
- `tests/` – Unit tests and fixtures for verifying generated code. Tests rely on xUnit and Verify for snapshot comparisons.

## Building and Testing

This repo normally builds with multiple target frameworks (net6.0–net9.0) and the generator targets `netstandard2.0`. The solution file is `LightObjects.sln`. Build and test scripts use the `dotnet` CLI.

## Performance Notes

- The generated structs are `readonly` and classes are `sealed` to avoid unnecessary allocations or inheritance costs.
- `ValueObjectException.ThrowIfFailed` should be used to validate creation without extra allocations.
- String comparisons in generated code use `StringComparison.Ordinal` where applicable.
- `GetHashCode()` implementations rely on the underlying value to keep hashing inexpensive.
- `EquatableImmutableArray<T>` exposes an efficient, allocation‑free enumerator and implements `IEquatable` to reduce boxing in equality checks.
- `TypeExtensions.IsValueObjectType` uses reflection. If called frequently in hot paths consider caching results in the application layer.
- When editing generator code, favor using `StringBuilder` with `Append`/`AppendLine` carefully to minimize string allocations.
- Hot methods (e.g., `Create`, `TryCreate`, conversions) may benefit from `[MethodImpl(MethodImplOptions.AggressiveInlining)]` if further optimization is needed.

## Contribution Tips

1. Keep the public API stable. The library is consumed by other packages and applications.
2. When modifying or adding generated code templates, ensure the tests under `tests/LightObjects.Generated.Tests` are updated accordingly.
3. Follow existing code style: modern C#, `Nullable` and `ImplicitUsings` enabled, `LangVersion` set to `latest`.
4. Commit generated files when updating generator output so snapshot tests remain consistent.
5. The project uses the MIT license—ensure any new code is compatible with this license.

