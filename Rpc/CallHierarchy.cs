using System.Text.Json.Serialization;

namespace CeorgLsp.Rpc
{
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
        public required Range Range { get; init; }

        [JsonPropertyName("selectionRange")]
        public required Range SelectionRange { get; init; }
    }
}
