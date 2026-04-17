using System.Text.Json;
using DataFilter.Core.Models;

namespace DataFilter.Demo.Shared.Services;

/// <summary>
/// JSON helpers for <see cref="FilterPipelineSnapshot"/> (presets / persistence in demos).
/// </summary>
public static class FilterPipelineJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Serialize(FilterPipelineSnapshot snapshot)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
        return JsonSerializer.Serialize(snapshot, Options);
    }

    public static FilterPipelineSnapshot Deserialize(string json)
    {
        if (json == null) throw new ArgumentNullException(nameof(json));
        return JsonSerializer.Deserialize<FilterPipelineSnapshot>(json, Options)
            ?? throw new JsonException("Deserialization returned null.");
    }
}
