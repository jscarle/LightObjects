using System.Collections.Immutable;
using System.Text.RegularExpressions;
using LightResults;
using Microsoft.CodeAnalysis;
using Shouldly;
using SourceGeneratorTestHelpers;
using SourceGeneratorTestHelpers.XUnit;

namespace LightObjects.Generated.Tests;

public sealed class GeneratedIdentifierSourceGeneratorTests
{
    static GeneratedIdentifierSourceGeneratorTests()
    {
        ModuleInitializer.Initialize();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GenerateGuidIdentifier(bool withNamespace)
    {
        var sources = GetSources("""
                                 /// <summary>Represents an identifier.</summary>
                                 [GeneratedIdentifier<Guid>]
                                 public partial struct TestGuidId;
                                 """, withNamespace
        );

        var (_, result, _) = RunGenerator(sources);
        await result.VerifyAsync("TestGuidId.g.cs");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GenerateIntIdentifier(bool withNamespace)
    {
        var sources = GetSources("""
                                 /// <summary>Represents an identifier.</summary>
                                 [GeneratedIdentifier<int>]
                                 public partial struct TestIntId;
                                 """, withNamespace
        );

        var (_, result, _) = RunGenerator(sources);
        await result.VerifyAsync("TestIntId.g.cs");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GenerateLongIdentifier(bool withNamespace)
    {
        var sources = GetSources("""
                                 /// <summary>Represents an identifier.</summary>
                                 [GeneratedIdentifier<long>]
                                 public partial struct TestLongId;
                                 """, withNamespace
        );

        var (_, result, _) = RunGenerator(sources);
        await result.VerifyAsync("TestLongId.g.cs");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GenerateShortIdentifier(bool withNamespace)
    {
        var sources = GetSources("""
                                 /// <summary>Represents an identifier.</summary>
                                 [GeneratedIdentifier<short>]
                                 public partial struct TestShortId;
                                 """, withNamespace
        );

        var (_, result, _) = RunGenerator(sources);
        await result.VerifyAsync("TestShortId.g.cs");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GenerateStringIdentifier(bool withNamespace)
    {
        var sources = GetSources("""
                                 /// <summary>Represents an identifier.</summary>
                                 [GeneratedIdentifier<string>]
                                 public partial class TestStringId;
                                 """, withNamespace
        );

        var (_, result, _) = RunGenerator(sources);
        await result.VerifyAsync("TestStringId.g.cs");
    }

    [Fact]
    public void GenerateNestedIdentifier_ShouldPreserveContainingType()
    {
        var sources = GetSources("""
                                 public sealed partial class Container
                                 {
                                     /// <summary>Represents an identifier.</summary>
                                     [GeneratedIdentifier<int>]
                                     public readonly partial struct TestNestedId;
                                 }
                                 """
        );

        var (_, result, _) = RunGenerator(sources);
        var generatedSource = result.GeneratedTrees
            .Single(tree => tree.FilePath.EndsWith("Container.TestNestedId.g.cs", StringComparison.Ordinal))
            .GetText(TestContext.Current.CancellationToken)
            .ToString();

        generatedSource.ShouldContain("public partial class Container");
        generatedSource.ShouldContain("public readonly partial struct TestNestedId");
        generatedSource.ShouldContain("global::LightObjects.ICreatableValueObject<int, TestNestedId>");
        generatedSource.ShouldNotContain("public readonly partial struct Container.TestNestedId");
    }

    [Fact]
    public void GenerateGenericIdentifier_ShouldPreserveGenericParameters()
    {
        var sources = GetSources("""
                                 /// <summary>Represents an identifier.</summary>
                                 [GeneratedIdentifier<int>]
                                 public readonly partial struct TestGenericId<T>
                                     where T : class;
                                 """
        );

        var (_, result, _) = RunGenerator(sources);
        var generatedSource = result.GeneratedTrees
            .Single(tree => tree.FilePath.EndsWith("TestGenericId`1.g.cs", StringComparison.Ordinal))
            .GetText(TestContext.Current.CancellationToken)
            .ToString();

        AssertPublicGeneratedMethodsHaveXmlDocs(generatedSource);
        generatedSource.ShouldContain("public readonly partial struct TestGenericId<T>");
        generatedSource.ShouldContain("global::LightObjects.ICreatableValueObject<int, TestGenericId<T>>");
        generatedSource.ShouldContain("where T : class");
        generatedSource.ShouldContain("public static TestGenericId<T> Create(int value)");
        generatedSource.ShouldContain("""/// <summary>Creates a <see cref="TestGenericId{T}" /> from the specified value.</summary>""");
        generatedSource.ShouldContain("""/// <summary>Compares this <see cref="TestGenericId{T}" /> value with another value of the same type.</summary>""");
        generatedSource.ShouldContain(
            """/// <exception cref="System.ArgumentException"><paramref name="obj" /> is not <c>null</c> and is not a <see cref="TestGenericId{T}" />.</exception>"""
        );
        generatedSource.ShouldContain("""<see cref="TestGenericId{T}" />""");
        generatedSource.ShouldNotContain("""<see cref="TestGenericId" />""");
        generatedSource.ShouldContain(
            "[global::System.Text.Json.Serialization.JsonConverter(typeof(global::MyProject.Identifiers.LightObjectsGenerated_TestGenericId1JsonConverterFactory))]"
        );
        generatedSource.ShouldContain("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]");
        generatedSource.ShouldContain("internal sealed class LightObjectsGenerated_TestGenericId1JsonConverterFactory");
        generatedSource.ShouldContain("""/// <summary>Provides JSON converters for generated identifier values.</summary>""");
        generatedSource.ShouldContain("""/// <summary>Provides JSON conversion for <typeparamref name="TIdentifier" /> values.</summary>""");
        generatedSource.ShouldContain("""/// <typeparam name="TIdentifier">The identifier type to convert.</typeparam>""");
        generatedSource.ShouldContain("typeToConvert.GetGenericTypeDefinition() == typeof(global::MyProject.Identifiers.TestGenericId<>)");
        generatedSource.ShouldNotContain("TypeConverter(typeof(TestGenericIdTypeConverter<>))");
    }

    [Fact]
    public void GenerateGenericStringIdentifier_ShouldDocumentGenericTypeConverter()
    {
        var sources = GetSources("""
                                 /// <summary>Represents an identifier.</summary>
                                 [GeneratedIdentifier<string>]
                                 public sealed partial class TestGenericStringId<T>
                                     where T : class;
                                 """
        );

        var (_, result, _) = RunGenerator(sources);
        var generatedSource = result.GeneratedTrees
            .Single(tree => tree.FilePath.EndsWith("TestGenericStringId`1.g.cs", StringComparison.Ordinal))
            .GetText(TestContext.Current.CancellationToken)
            .ToString();

        AssertPublicGeneratedMethodsHaveXmlDocs(generatedSource);
        generatedSource.ShouldContain("public sealed partial class TestGenericStringId<T>");
        generatedSource.ShouldContain(
            "[global::System.ComponentModel.TypeDescriptionProvider(typeof(global::MyProject.Identifiers.LightObjectsGenerated_TestGenericStringId1TypeDescriptionProvider))]"
        );
        generatedSource.ShouldContain("private sealed class IdentifierTypeConverter : global::System.ComponentModel.TypeConverter");
        generatedSource.ShouldContain("""/// <summary>Provides type conversion for generated identifier values.</summary>""");
        generatedSource.ShouldContain("""/// <summary>Provides JSON converters for generated identifier values.</summary>""");
        generatedSource.ShouldContain("""/// <summary>Provides JSON conversion for <typeparamref name="TIdentifier" /> values.</summary>""");
    }

    [Fact]
    public void GenerateGenericIdentifier_WithNullableReferenceConstraint_ShouldPreserveConstraint()
    {
        var sources = GetSources("""
                                 /// <summary>Represents an identifier.</summary>
                                 [GeneratedIdentifier<int>]
                                 public readonly partial struct TestGenericId<T>
                                     where T : class?;
                                 """
        );

        var (_, result, _) = RunGenerator(sources);
        var generatedSource = result.GeneratedTrees
            .Single(tree => tree.FilePath.EndsWith("TestGenericId`1.g.cs", StringComparison.Ordinal))
            .GetText(TestContext.Current.CancellationToken)
            .ToString();

        generatedSource.ShouldContain("public readonly partial struct TestGenericId<T>");
        generatedSource.ShouldContain("where T : class?");
        generatedSource.ShouldNotContain("where T : class\n");
    }

    [Fact]
    public void GenerateIdentifier_WithEscapedKeywordName_ShouldEscapeGeneratedReferences()
    {
        var sources = GetSources("""
                                 /// <summary>Represents an identifier.</summary>
                                 [GeneratedIdentifier<int>]
                                 public readonly partial struct @event;
                                 """
        );

        var (_, result, diagnostics) = RunGenerator(sources);
        diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ShouldBeEmpty();
        result.Diagnostics
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ShouldBeEmpty();

        var generatedSource = result.GeneratedTrees
            .Select(tree => tree.GetText(TestContext.Current.CancellationToken).ToString()
            )
            .Single(source => source.Contains("public readonly partial struct @event", StringComparison.Ordinal));

        generatedSource.ShouldContain("global::LightObjects.ICreatableValueObject<int, @event>");
        generatedSource.ShouldContain("private @event(int value, bool skipValidation = false)");
        generatedSource.ShouldContain("public static @event Create(int value)");
        generatedSource.ShouldContain("public sealed class @eventTypeConverter");
        generatedSource.ShouldContain("global::System.Text.Json.Serialization.JsonConverter<@event>");
        generatedSource.ShouldNotContain("partial struct event");
        generatedSource.ShouldNotContain("JsonConverter<event>");
    }

    [Fact]
    public void GenerateRecordIdentifier_ShouldReportDiagnosticAndNotGenerateIdentifier()
    {
        var sources = GetSources("""
                                 /// <summary>Represents an identifier.</summary>
                                 [GeneratedIdentifier<int>]
                                 public readonly partial record struct RecordId;
                                 """
        );

        var (_, result, diagnostics) = RunGenerator(sources);

        var diagnostic = diagnostics.Single(diagnostic => diagnostic.Id == "LO0003");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.GetMessage()
            .ShouldBe("Record identifiers are not supported. Declare 'MyProject.Identifiers.RecordId' as a partial class or struct instead.");
        result.GeneratedTrees
            .Any(tree => tree.FilePath.EndsWith("RecordId.g.cs", StringComparison.Ordinal))
            .ShouldBeFalse();
    }

    [Fact]
    public void GenerateRefStructIdentifier_ShouldReportDiagnosticAndNotGenerateIdentifier()
    {
        var sources = GetSources("""
                                 /// <summary>Represents an identifier.</summary>
                                 [GeneratedIdentifier<int>]
                                 public readonly ref partial struct RefId;
                                 """
        );

        var (_, result, diagnostics) = RunGenerator(sources);

        var diagnostic = diagnostics.Single(diagnostic => diagnostic.Id == "LO0004");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.GetMessage()
            .ShouldBe("Ref struct identifiers are not supported because generated identifiers implement interfaces and use generic result/converter types");
        result.GeneratedTrees
            .Any(tree => tree.FilePath.EndsWith("RefId.g.cs", StringComparison.Ordinal))
            .ShouldBeFalse();
    }

    [Fact]
    public void GenerateFileLocalIdentifier_ShouldReportDiagnosticAndNotGenerateIdentifier()
    {
        var sources = GetSources("""
                                 /// <summary>Represents an identifier.</summary>
                                 [GeneratedIdentifier<int>]
                                 file readonly partial struct FileId;
                                 """
        );

        var (_, result, diagnostics) = RunGenerator(sources);

        var diagnostic = diagnostics.Single(diagnostic => diagnostic.Id == "LO0005");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.GetMessage()
            .ShouldBe("File-local identifiers are not supported because generated partial declarations are emitted into a separate file");
        result.GeneratedTrees
            .Any(tree => tree.FilePath.EndsWith("FileId.g.cs", StringComparison.Ordinal))
            .ShouldBeFalse();
    }

    [Fact]
    public void GenerateStringIdentifier_ForStruct_ShouldWarnAndNotGenerateIdentifier()
    {
        var sources = GetSources("""
                                 /// <summary>Represents an identifier.</summary>
                                 [GeneratedIdentifier<string>]
                                 public readonly partial struct TestStringId;
                                 """
        );

        var (_, result, diagnostics) = RunGenerator(sources);

        var diagnostic = diagnostics.Single(diagnostic => diagnostic.Id == "LO0002");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diagnostic.GetMessage()
            .ShouldBe("String identifiers must be declared as classes because default string-backed structs can contain a null value");
        result.GeneratedTrees
            .Any(tree => tree.FilePath.EndsWith("TestStringId.g.cs", StringComparison.Ordinal))
            .ShouldBeFalse();
    }

    [Fact]
    public void GenerateIntIdentifier_WithCustomValidation_ShouldNotGenerateDefaultValidation()
    {
        var sources = GetSources("""
                                 /// <summary>Represents an identifier.</summary>
                                 [GeneratedIdentifier<int>]
                                 public partial struct TestIntId
                                 {
                                     private static LightResults.Result Validate(int value)
                                     {
                                         return value > 0
                                             ? LightResults.Result.Success()
                                             : LightResults.Result.Failure("The value must be greater than zero.");
                                     }
                                 }
                                 """
        );

        var (_, result, _) = RunGenerator(sources);
        var generatedSource = result.GeneratedTrees
            .Single(tree => tree.FilePath.EndsWith("TestIntId.g.cs", StringComparison.Ordinal))
            .GetText(TestContext.Current.CancellationToken)
            .ToString();

        generatedSource.ShouldContain("var validation = Validate(value);");
        generatedSource.ShouldNotContain("private static Result Validate(int value)");
    }

    [Fact]
    public void GenerateStringIdentifier_WithCustomValidation_ShouldNotGenerateDefaultValidation()
    {
        var sources = GetSources("""
                                 /// <summary>Represents an identifier.</summary>
                                 [GeneratedIdentifier<string>]
                                 public partial class TestStringId
                                 {
                                     private static LightResults.Result Validate(string value)
                                     {
                                         return LightResults.Result.Success();
                                     }
                                 }
                                 """
        );

        var (_, result, _) = RunGenerator(sources);
        var generatedSource = result.GeneratedTrees
            .Single(tree => tree.FilePath.EndsWith("TestStringId.g.cs", StringComparison.Ordinal))
            .GetText(TestContext.Current.CancellationToken)
            .ToString();

        generatedSource.ShouldContain("var validation = Validate(value);");
        generatedSource.ShouldContain("return TestStringId.Create(value!);");
        generatedSource.ShouldNotContain("private static Result Validate(string value)");
        generatedSource.ShouldNotContain("string.IsNullOrWhiteSpace(value)");
    }

    public static IEnumerable<object[]> MismatchedCustomValidationSources()
    {
        yield return
        [
            """
            private static LightResults.Result validate(int value)
            {
                return LightResults.Result.Success();
            }
            """,
        ];
        yield return
        [
            """
            public static LightResults.Result Validate(int value)
            {
                return LightResults.Result.Success();
            }
            """,
        ];
        yield return
        [
            """
            private LightResults.Result Validate(int value)
            {
                return LightResults.Result.Success();
            }
            """,
        ];
        yield return
        [
            """
            private static bool Validate(int value)
            {
                return true;
            }
            """,
        ];
        yield return
        [
            """
            private static LightResults.Result Validate(long value)
            {
                return LightResults.Result.Success();
            }
            """,
        ];
        yield return
        [
            """
            private static LightResults.Result Validate(int value, int other)
            {
                return LightResults.Result.Success();
            }
            """,
        ];
    }

    [Theory]
    [MemberData(nameof(MismatchedCustomValidationSources))]
    public void GenerateIntIdentifier_WithMismatchedCustomValidation_ShouldGenerateDefaultValidation(string validationMethod)
    {
        var sources = GetSources($$"""
                                   /// <summary>Represents an identifier.</summary>
                                   [GeneratedIdentifier<int>]
                                   public partial struct TestIntId
                                   {
                                       {{validationMethod}}
                                   }
                                   """
        );

        var (_, result, _) = RunGenerator(sources);
        var generatedSource = result.GeneratedTrees
            .Single(tree => tree.FilePath.EndsWith("TestIntId.g.cs", StringComparison.Ordinal))
            .GetText(TestContext.Current.CancellationToken)
            .ToString();

        generatedSource.ShouldContain("private static global::LightResults.Result Validate(int value)");
    }

    private static IEnumerable<string> GetSources(string source, bool withNamespace = true)
    {
        const string usingStatements = """
                                       using System;
                                       using LightObjects.Generated;
                                       """;

        if (withNamespace)
            yield return $"""
                          {usingStatements}

                          namespace MyProject.Identifiers;

                          {source}
                          """;
        else
            yield return $"""
                          {usingStatements}

                          {source}
                          """;
    }

    // ReSharper disable once UnusedTupleComponentInReturnValue
    private static (GeneratedIdentifierSourceGenerator Generator, GeneratorDriverRunResult Result, ImmutableArray<Diagnostic> Diagnostics) RunGenerator(
        IEnumerable<string> sources
    )
    {
        var metadataReferences = GetTargetFrameworkReferences()
            .Append(MetadataReference.CreateFromFile(typeof(IValueObject<,>).Assembly.Location))
            .Append(MetadataReference.CreateFromFile(typeof(Result).Assembly.Location));

        return IncrementalGenerator.RunWithDiagnostics<GeneratedIdentifierSourceGenerator>(sources, metadataReferences: metadataReferences);
    }

    private static IEnumerable<MetadataReference> GetTargetFrameworkReferences()
    {
#if NET10_0
        const string targetFramework = "net10.0";
        const string versionPrefix = "10.";
#elif NET9_0
        const string targetFramework = "net9.0";
        const string versionPrefix = "9.";
#elif NET8_0
        const string targetFramework = "net8.0";
        const string versionPrefix = "8.";
#else
#error Unsupported test target framework.
#endif

        var runtimeDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var dotnetRoot = Path.GetFullPath(Path.Combine(runtimeDirectory, "..", "..", ".."));
        var referencePackDirectory = Path.Combine(dotnetRoot, "packs", "Microsoft.NETCore.App.Ref");
        var referencePackVersionDirectory = Directory.EnumerateDirectories(referencePackDirectory)
            .Where(path => Path.GetFileName(path)
                .StartsWith(versionPrefix, StringComparison.Ordinal)
            )
            .OrderByDescending(path => Version.Parse(Path.GetFileName(path)))
            .First();

        var referenceDirectory = Path.Combine(referencePackVersionDirectory, "ref", targetFramework);
        return Directory.EnumerateFiles(referenceDirectory, "*.dll")
            .Select(path => MetadataReference.CreateFromFile(path));
    }

    private static void AssertPublicGeneratedMethodsHaveXmlDocs(string generatedSource)
    {
        const string publicMethodPattern = @"(?m)^\s*public\s+(?:(?:static|override)\s+)?(?:[^\r\n(]+)\([^;\r\n]*\)\s*$";
        var matches = Regex.Matches(generatedSource, publicMethodPattern);

        foreach (Match match in matches)
        {
            var previousLineEnd = generatedSource.LastIndexOf('\n', match.Index - 1);
            if (previousLineEnd < 0)
                continue;

            var previousLineStart = generatedSource.LastIndexOf('\n', previousLineEnd - 1);
            var previousLine = generatedSource.Substring(previousLineStart + 1, previousLineEnd - previousLineStart - 1)
                .Trim();

            previousLine.ShouldStartWith("///");
        }
    }
}
