namespace LightObjects.Generated.Common;

/// <summary>Represents a declaration.</summary>
internal sealed record Declaration
{
    /// <summary>Initializes a new instance of the <see cref="Declaration"/> class with the specified type, name, and generic parameters.</summary>
    /// <param name="type">The type of declaration.</param>
    /// <param name="name">The name of the declaration.</param>
    /// <param name="genericParameters">A read-only list of generic parameter names, or an empty list if not generic.</param>
    /// <param name="genericParameterConstraints">A read-only list of generic parameter constraint clauses, or an empty list if there are no constraints.</param>
    /// <param name="accessibility">The declaration accessibility keyword, or an empty string if not applicable.</param>
    /// <param name="isStatic">A value indicating whether the declaration is static.</param>
    internal Declaration(
        DeclarationType type,
        string name,
        EquatableImmutableArray<string> genericParameters,
        EquatableImmutableArray<string> genericParameterConstraints = default,
        string accessibility = "",
        bool isStatic = false
    )
    {
        Type = type;
        Name = name;
        GenericParameters = genericParameters;
        GenericParameterConstraints = genericParameterConstraints;
        Accessibility = accessibility;
        IsStatic = isStatic;
    }

    /// <summary>Gets the type of declaration.</summary>
    public DeclarationType Type { get; }

    /// <summary>Gets the name of the declaration.</summary>
    public string Name { get; }

    /// <summary>Gets a read-only list of generic parameter names for generic declarations, or an empty list otherwise.</summary>
    public EquatableImmutableArray<string> GenericParameters { get; }

    /// <summary>Gets a read-only list of generic parameter constraint clauses, or an empty list if there are no constraints.</summary>
    public EquatableImmutableArray<string> GenericParameterConstraints { get; }

    /// <summary>Gets the declaration accessibility keyword, or an empty string if not applicable.</summary>
    public string Accessibility { get; }

    /// <summary>Gets a value indicating whether the declaration is static.</summary>
    public bool IsStatic { get; }

    /// <summary>Returns a string representation of the declaration in the appropriate format for its type.</summary>
    /// <returns>A string representation of the declaration.</returns>
    public override string ToString()
    {
        switch (Type)
        {
            case DeclarationType.Namespace:
                return $"namespace {Name}";
            case DeclarationType.Interface:
            case DeclarationType.Class:
            case DeclarationType.Record:
            case DeclarationType.Struct:
            case DeclarationType.RecordStruct:
                var keyword = ToKeyword(Type);
                var genericParameters = GenericParameters.Count == 0 ? string.Empty : $"<{string.Join(", ", GenericParameters)}>";
                var genericParameterConstraints = GenericParameterConstraints.ToGenericConstraintList();
                var staticModifier = IsStatic ? "static " : string.Empty;
                var accessibility = Accessibility.Length == 0 ? string.Empty : $"{Accessibility} ";
                return $"{accessibility}{staticModifier}{keyword} {Name}{genericParameters}{genericParameterConstraints}";
            default:
                return base.ToString();
        }
    }

    private static string ToKeyword(DeclarationType declarationType)
    {
        return declarationType switch
        {
            DeclarationType.Namespace => "namespace",
            DeclarationType.Interface => "interface",
            DeclarationType.Class => "class",
            DeclarationType.Record => "record",
            DeclarationType.Struct => "struct",
            DeclarationType.RecordStruct => "record struct",
            _ => throw new InvalidOperationException(),
        };
    }
}
