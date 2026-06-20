using System.Text;

namespace LightObjects.Generated.Common;

/// <summary>Provides extension methods for working with declarations.</summary>
internal static class DeclarationExtensions
{
    /// <summary>Converts a list of declarations to their corresponding namespace.</summary>
    /// <param name="declarations">The list of declarations to convert.</param>
    /// <returns>The namespace represented by the declarations.</returns>
    public static string ToNamespace(this EquatableImmutableArray<Declaration> declarations)
    {
        var builder = new StringBuilder();

        for (var index = 0; index < declarations.Count; index++)
        {
            var declaration = declarations[index];
            if (declaration.Type != DeclarationType.Namespace)
                continue;

            if (builder.Length > 0)
                builder.Append('.');
            builder.Append(declaration.Name);
        }

        return builder.ToString();
    }

    /// <summary>Converts a list of declarations to their fully qualified name.</summary>
    /// <param name="declarations">The list of declarations to convert.</param>
    /// <returns>The fully qualified name represented by the declarations.</returns>
    public static string ToFullyQualifiedName(this EquatableImmutableArray<Declaration> declarations)
    {
        var builder = new StringBuilder();

        for (var index = 0; index < declarations.Count; index++)
        {
            var declaration = declarations[index];

            if (builder.Length > 0)
                builder.Append('.');
            builder.Append(declaration.Name);

            if (declaration.GenericParameters.Count == 0)
                continue;

            builder.Append('`');
            builder.Append(declaration.GenericParameters.Count);
        }

        return builder.ToString();
    }

    public static string ToGenericParameterList(this EquatableImmutableArray<string> genericParameters)
    {
        return genericParameters.Count == 0 ? string.Empty : $"<{string.Join(", ", genericParameters)}>";
    }

    public static string ToGenericAritySuffix(this EquatableImmutableArray<string> genericParameters)
    {
        return genericParameters.Count == 0 ? string.Empty : $"`{genericParameters.Count}";
    }

    public static string ToGenericConstraintList(this EquatableImmutableArray<string> genericParameterConstraints)
    {
        return genericParameterConstraints.Count == 0 ? string.Empty : $" {string.Join(" ", genericParameterConstraints)}";
    }

    public static string ToIndentedGenericConstraintList(this EquatableImmutableArray<string> genericParameterConstraints)
    {
        return genericParameterConstraints.Count == 0 ? string.Empty : $"\n    {string.Join("\n    ", genericParameterConstraints)}";
    }

    public static string ToUnboundGenericParameterList(this EquatableImmutableArray<string> genericParameters)
    {
        if (genericParameters.Count == 0)
            return string.Empty;

        return genericParameters.Count == 1 ? "<>" : $"<{new string(',', genericParameters.Count - 1)}>";
    }

    public static string ToPartialDeclaration(this Declaration declaration)
    {
        var accessibility = declaration.Accessibility.Length == 0 ? string.Empty : $"{declaration.Accessibility} ";
        var staticModifier = declaration.IsStatic ? "static " : string.Empty;
        var genericParameters = declaration.GenericParameters.ToGenericParameterList();
        var genericParameterConstraints = declaration.GenericParameterConstraints.ToGenericConstraintList();

        return declaration.Type switch
        {
            DeclarationType.Interface => $"{accessibility}partial interface {declaration.Name}{genericParameters}{genericParameterConstraints}",
            DeclarationType.Class => $"{accessibility}{staticModifier}partial class {declaration.Name}{genericParameters}{genericParameterConstraints}",
            DeclarationType.Record => $"{accessibility}partial record {declaration.Name}{genericParameters}{genericParameterConstraints}",
            DeclarationType.Struct => $"{accessibility}partial struct {declaration.Name}{genericParameters}{genericParameterConstraints}",
            DeclarationType.RecordStruct => $"{accessibility}partial record struct {declaration.Name}{genericParameters}{genericParameterConstraints}",
            _ => throw new InvalidOperationException(),
        };
    }
}
