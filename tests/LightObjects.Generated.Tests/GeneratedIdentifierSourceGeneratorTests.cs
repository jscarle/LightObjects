using System.Collections.Immutable;
using Basic.Reference.Assemblies;
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

        var result = RunGenerator(sources);
        await result.VerifyAsync("TestGuidId.g.cs")
            .UseMethodName($"{nameof(GenerateGuidIdentifier)}_With{(withNamespace ? "" : "out")}Namespace");
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

        var result = RunGenerator(sources);
        await result.VerifyAsync("TestIntId.g.cs")
            .UseMethodName($"{nameof(GenerateIntIdentifier)}_With{(withNamespace ? "" : "out")}Namespace");
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

        var result = RunGenerator(sources);
        await result.VerifyAsync("TestLongId.g.cs")
            .UseMethodName($"{nameof(GenerateLongIdentifier)}_With{(withNamespace ? "" : "out")}Namespace");
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

        var result = RunGenerator(sources);
        await result.VerifyAsync("TestShortId.g.cs")
            .UseMethodName($"{nameof(GenerateShortIdentifier)}_With{(withNamespace ? "" : "out")}Namespace");
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

        var result = RunGenerator(sources);
        await result.VerifyAsync("TestStringId.g.cs")
            .UseMethodName($"{nameof(GenerateStringIdentifier)}_With{(withNamespace ? "" : "out")}Namespace");
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

        var result = RunGenerator(sources);
        var generatedSource = result.Result.GeneratedTrees
            .Single(tree => tree.FilePath.EndsWith("Container.TestNestedId.g.cs", StringComparison.Ordinal))
            .GetText()
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

        var result = RunGenerator(sources);
        var generatedSource = result.Result.GeneratedTrees
            .Single(tree => tree.FilePath.EndsWith("TestGenericId`1.g.cs", StringComparison.Ordinal))
            .GetText()
            .ToString();

        generatedSource.ShouldContain("public readonly partial struct TestGenericId<T>");
        generatedSource.ShouldContain("global::LightObjects.ICreatableValueObject<int, TestGenericId<T>>");
        generatedSource.ShouldContain("where T : class");
        generatedSource.ShouldContain("public static TestGenericId<T> Create(int value)");
        generatedSource.ShouldContain("[global::System.Text.Json.Serialization.JsonConverter(typeof(global::MyProject.Identifiers.LightObjectsGenerated_TestGenericId1JsonConverterFactory))]");
        generatedSource.ShouldContain("internal sealed class LightObjectsGenerated_TestGenericId1JsonConverterFactory");
        generatedSource.ShouldContain("typeToConvert.GetGenericTypeDefinition() == typeof(global::MyProject.Identifiers.TestGenericId<>)");
        generatedSource.ShouldNotContain("TypeConverter(typeof(TestGenericIdTypeConverter<>))");
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

        var result = RunGenerator(sources);
        var generatedSource = result.Result.GeneratedTrees
            .Single(tree => tree.FilePath.EndsWith("TestGenericId`1.g.cs", StringComparison.Ordinal))
            .GetText()
            .ToString();

        generatedSource.ShouldContain("public readonly partial struct TestGenericId<T>");
        generatedSource.ShouldContain("where T : class?");
        generatedSource.ShouldNotContain("where T : class\n");
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

        var result = RunGenerator(sources);

        var diagnostic = result.Result.Diagnostics.Single(diagnostic => diagnostic.Id == "LO0002");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diagnostic.GetMessage().ShouldBe("String identifiers must be declared as classes because default string-backed structs can contain a null value");
        result.Result.GeneratedTrees.Any(tree => tree.FilePath.EndsWith("TestStringId.g.cs", StringComparison.Ordinal)).ShouldBeFalse();
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
        ).Append("""
                 namespace LightResults;

                 public readonly struct Result
                 {
                     public static Result Success()
                     {
                         return default;
                     }

                     public static Result Failure(string message)
                     {
                         return default;
                     }
                 }
                 """);

        var result = RunGenerator(sources);
        var generatedSource = result.Result.GeneratedTrees
            .Single(tree => tree.FilePath.EndsWith("TestIntId.g.cs", StringComparison.Ordinal))
            .GetText()
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
        ).Append("""
                 namespace LightResults;

                 public readonly struct Result
                 {
                     public static Result Success()
                     {
                         return default;
                     }
                 }
                 """);

        var result = RunGenerator(sources);
        var generatedSource = result.Result.GeneratedTrees
            .Single(tree => tree.FilePath.EndsWith("TestStringId.g.cs", StringComparison.Ordinal))
            .GetText()
            .ToString();

        generatedSource.ShouldContain("var validation = Validate(value);");
        generatedSource.ShouldContain("return TestStringId.Create(value!);");
        generatedSource.ShouldNotContain("private static Result Validate(string value)");
        generatedSource.ShouldNotContain("string.IsNullOrWhiteSpace(value)");
    }

    public static IEnumerable<object[]> MismatchedCustomValidationSources()
    {
        yield return new object[]
        {
            """
            private static LightResults.Result validate(int value)
            {
                return LightResults.Result.Success();
            }
            """
        };
        yield return new object[]
        {
            """
            public static LightResults.Result Validate(int value)
            {
                return LightResults.Result.Success();
            }
            """
        };
        yield return new object[]
        {
            """
            private LightResults.Result Validate(int value)
            {
                return LightResults.Result.Success();
            }
            """
        };
        yield return new object[]
        {
            """
            private static bool Validate(int value)
            {
                return true;
            }
            """
        };
        yield return new object[]
        {
            """
            private static LightResults.Result Validate(long value)
            {
                return LightResults.Result.Success();
            }
            """
        };
        yield return new object[]
        {
            """
            private static LightResults.Result Validate(int value, int other)
            {
                return LightResults.Result.Success();
            }
            """
        };
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
        ).Append("""
                 namespace LightResults;

                 public readonly struct Result
                 {
                     public static Result Success()
                     {
                         return default;
                     }
                 }
                 """);

        var result = RunGenerator(sources);
        var generatedSource = result.Result.GeneratedTrees
            .Single(tree => tree.FilePath.EndsWith("TestIntId.g.cs", StringComparison.Ordinal))
            .GetText()
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
        yield return """
                     using System;

                     namespace LightObjects.Generated;

                     [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
                     internal sealed class GeneratedIdentifierAttribute<TIdentifier> : Attribute;
                     """;
    }

    private static (ImmutableArray<Diagnostic> Diagnostics, GeneratorDriverRunResult Result) RunGenerator(IEnumerable<string> sources)
    {
        return IncrementalGenerator.RunWithDiagnostics<GeneratedIdentifierSourceGenerator>(sources, metadataReferences: ReferenceAssemblies.Net80);
    }
}
