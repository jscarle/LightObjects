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
- `tests/` – Unit tests and fixtures for verifying generated code. Tests rely on xUnit and SourceGeneratorTestHelpers for snapshot comparisons.

## Building and Testing

This repo normally builds with multiple target frameworks (net8.0–net11.0) and the generator targets `netstandard2.0`. The solution file is `LightObjects.sln`. Build and test scripts use the .NET 11 SDK and run tests across all supported target frameworks.

`global.json` selects Microsoft.Testing.Platform for `dotnet test`, matching xUnit's current runner. Snapshot files use LF line endings. SourceGeneratorTestHelpers provides snapshot comparisons without Verify packages.

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
3. When adding or changing analyzer diagnostics, record them in `src/LightObjects.Generated/AnalyzerReleases.Shipped.md`; do not leave new analyzer entries in `AnalyzerReleases.Unshipped.md`.
4. Follow existing code style: modern C#, `Nullable` and `ImplicitUsings` enabled, and the default language version for each runtime target. The `netstandard2.0` generator uses `LangVersion` set to `latest`.
5. Commit generated files when updating generator output so snapshot tests remain consistent.
6. The project uses the MIT license—ensure any new code is compatible with this license.

## Release Intent, Versioning, and Workflow

Treat explicit release requests—such as “Mint a new release,” “Release this,” “Publish this as a new release,” “Create a new release,” or any clear equivalent—as authorization to carry out LightObjects' full release workflow, including committing, pushing, tagging, and publishing as needed.

If the user has not clearly requested a release, or if the appropriate change category cannot be determined confidently, ask for clarification **before** committing, pushing, creating a tag, or publishing a release.

Only change LightObjects' package, assembly, and file versions when the user has clearly requested that a release be published. Do not bump versions during ordinary implementation, refactoring, bug-fixing, dependency updates, testing, commits, pushes, or pull-request preparation unless those actions are part of an explicitly requested release.

Choose the next version using these repository-specific rules:

- Minor, backward-compatible changes increment the patch component. Example: `10.0.2` → `10.0.3`.
- Moderate or breaking changes increment the middle component and reset the patch. Example: `10.0.2` → `10.1.0`.
- Increment the major component and reset the other components only when the newest supported .NET version changes. Example: `10.0.2` → `11.0.0`.
- Breaking changes alone do not justify a major-version increment in this repository. If multiple categories apply, use the highest applicable category.

LightObjects versions `src/LightObjects/LightObjects.csproj` and `src/LightObjects.Generated/LightObjects.Generated.csproj` in lockstep. Update each project's three-component `Version` and matching four-component `AssemblyVersion` and `FileVersion`, even though only `LightObjects` is currently packable. Use a three-component lightweight tag such as `v10.0.3`; some historical tags are annotated, but new release tags must be lightweight.

A clear release request authorizes this complete workflow:

1. Inspect the worktree, current branch, `origin`, `develop`, both project versions, recent commits, pull requests, and releases before editing release metadata.
2. Determine the next version, update both project files consistently, and verify that the tag and release do not already exist.
3. Validate the release-scoped changes using the repository's normal restore, Release build, source-generator snapshot tests, and pack commands across all supported target frameworks.
4. Commit all release-scoped changes on the reusable `feature/work` branch using a concise past-tense sentence ending in a period, then push `feature/work`.
5. Open a ready pull request from `feature/work` to `develop` using recent title and body conventions, enable squash auto-merge, and monitor checks and review state.
6. Fix and push any issue that prevents the pull request from merging, then wait until it is merged.
7. Fast-forward local `develop` from `origin/develop` and reset local `feature/work` to that latest `develop` commit. The remote feature branch may be deleted automatically after merge.
8. Create the lightweight `vX.Y.Z` tag on the latest `develop` commit and push that tag to `origin`.
9. Publish a non-draft GitHub release named `vX.Y.Z - Description`, using concise release-note bullets consistent with recent LightObjects releases.
10. Monitor the release-triggered `Publish` workflow until the LightObjects NuGet package is published successfully, fixing in-scope failures when possible.

Do not require separate confirmation for each Git or GitHub step after an unambiguous release command. Report the pull request, merge commit, tag, GitHub release URL, and NuGet publishing result when complete.

