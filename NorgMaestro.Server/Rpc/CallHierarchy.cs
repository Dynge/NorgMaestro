using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record CallHierarchyItem
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("kind")]
    public required SymbolKind Kind { get; init; }

    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("range")]
    public required TextRange Range { get; init; }

    [JsonPropertyName("selectionRange")]
    public required TextRange SelectionRange { get; init; }
}
