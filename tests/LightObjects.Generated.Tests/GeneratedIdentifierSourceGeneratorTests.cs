using System.Collections.Immutable;
using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
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
