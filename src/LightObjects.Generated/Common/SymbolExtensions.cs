using Microsoft.CodeAnalysis;

namespace LightObjects.Generated.Common;

/// <summary>Provides extension methods for working with symbols.</summary>
internal static class SymbolExtensions
{
    /// <summary>Gets a list of declarations representing the hierarchy containing the given symbol.</summary>
    /// <param name="symbol">The <see cref="ISymbol"/> to get the containing declarations for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="EquatableImmutableArray{T}"/> of <see cref="Declaration"/> objects representing the hierarchy.</returns>
    public static EquatableImmutableArray<Declaration> GetContainingDeclarations(this ISymbol symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var declarations = new Stack<Declaration>();

        BuildContainingSymbolHierarchy(symbol, in declarations, cancellationToken);

        return declarations.ToEquatableImmutableArray();
    }

    private static void BuildContainingSymbolHierarchy(ISymbol symbol, in Stack<Declaration> declarations, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        switch (symbol.ContainingSymbol)
        {
            case INamespaceSymbol namespaceSymbol:
                BuildNamespaceHierarchy(namespaceSymbol, declarations, cancellationToken);
                break;
            case INamedTypeSymbol namedTypeSymbol:
                BuildTypeHierarchy(namedTypeSymbol, declarations, cancellationToken);
                break;
        }
    }

    private static void BuildNamespaceHierarchy(INamespaceSymbol symbol, in Stack<Declaration> declarations, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!symbol.IsGlobalNamespace)
        {
            var namespaceDeclaration = new Declaration(DeclarationType.Namespace, symbol.Name, EquatableImmutableArray<string>.Empty);
            declarations.Push(namespaceDeclaration);
        }

        if (symbol.ContainingNamespace is not null && !symbol.ContainingNamespace.IsGlobalNamespace)
            BuildNamespaceHierarchy(symbol.ContainingNamespace, declarations, cancellationToken);
    }

    private static void BuildTypeHierarchy(INamedTypeSymbol symbol, in Stack<Declaration> declarations, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var declarationType = symbol.GetDeclarationType(cancellationToken);
        if (declarationType is null)
            return;

        var genericTypeParameters = symbol.GetGenericTypeParameters(cancellationToken);
        var genericTypeParameterConstraints = symbol.GetGenericTypeParameterConstraints(cancellationToken);
        var accessibility = symbol.DeclaredAccessibility.ToKeyword();

        var typeDeclaration = new Declaration(declarationType.Value, symbol.Name, genericTypeParameters, genericTypeParameterConstraints, accessibility, symbol.IsStatic);
        declarations.Push(typeDeclaration);

        BuildContainingSymbolHierarchy(symbol, declarations, cancellationToken);
    }

    public static EquatableImmutableArray<string> GetGenericTypeParameters(this INamedTypeSymbol symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!symbol.IsGenericType)
            return EquatableImmutableArray<string>.Empty;

        var genericTypeParameters = new List<string>();

        for (var index = 0; index < symbol.TypeParameters.Length; index++)
        {
            var typeParameter = symbol.TypeParameters[index];
            genericTypeParameters.Add(typeParameter.Name);
        }

        return genericTypeParameters.ToEquatableImmutableArray();
    }

    public static string ToKeyword(this Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedAndInternal => "private protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            _ => string.Empty,
        };
    }

    public static EquatableImmutableArray<string> GetGenericTypeParameterConstraints(this INamedTypeSymbol symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!symbol.IsGenericType)
            return EquatableImmutableArray<string>.Empty;

        var genericTypeParameterConstraints = new List<string>();

        for (var index = 0; index < symbol.TypeParameters.Length; index++)
        {
            var typeParameter = symbol.TypeParameters[index];
            var constraints = typeParameter.GetConstraintParts();
            if (constraints.Count == 0)
                continue;

            genericTypeParameterConstraints.Add($"where {typeParameter.Name} : {string.Join(", ", constraints)}");
        }

        return genericTypeParameterConstraints.ToEquatableImmutableArray();
    }

    private static List<string> GetConstraintParts(this ITypeParameterSymbol typeParameter)
    {
        var constraints = new List<string>();

        if (typeParameter.HasUnmanagedTypeConstraint)
            constraints.Add("unmanaged");
        else if (typeParameter.HasValueTypeConstraint)
            constraints.Add("struct");
        else if (typeParameter.HasReferenceTypeConstraint)
            constraints.Add("class");
        else if (typeParameter.HasNotNullConstraint)
            constraints.Add("notnull");

        foreach (var constraintType in typeParameter.ConstraintTypes)
            constraints.Add(constraintType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

        if (typeParameter.HasConstructorConstraint)
            constraints.Add("new()");

        return constraints;
    }

    private static DeclarationType? GetDeclarationType(this ITypeSymbol symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return symbol switch
        {
            { IsReferenceType: true, TypeKind: TypeKind.Interface } => DeclarationType.Interface,
            { IsReferenceType: true, IsRecord: true } => DeclarationType.Record,
            { IsReferenceType: true } => DeclarationType.Class,
            { IsValueType: true, IsRecord: true } => DeclarationType.RecordStruct,
            { IsValueType: true } => DeclarationType.Struct,
            _ => null,
        };
    }
}
