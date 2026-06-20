using Microsoft.CodeAnalysis.CSharp;

namespace LightObjects.Generated.Common;

internal static class IdentifierExtensions
{
    public static string ToEscapedIdentifier(this string identifier)
    {
        return SyntaxFacts.GetKeywordKind(identifier) == SyntaxKind.None ? identifier : $"@{identifier}";
    }

    public static string ToGeneratedIdentifierPart(this string identifier)
    {
        return identifier.StartsWith("@", StringComparison.Ordinal) ? identifier.Substring(1) : identifier;
    }
}
