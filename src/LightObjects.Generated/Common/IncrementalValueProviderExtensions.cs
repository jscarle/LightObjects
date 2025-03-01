using Microsoft.CodeAnalysis;

namespace LightObjects.Generated.Common;

internal static class IncrementalValueProviderExtensions
{
    public static IncrementalValuesProvider<TSource> WhereNotNull<TSource>(this IncrementalValuesProvider<TSource?> source)
        where TSource : struct
    {
        return source.Where(x => x is not null)
            .Select((x, _) => x!.Value);
    }
}
