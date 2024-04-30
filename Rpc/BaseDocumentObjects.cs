using System.Text.Json.Serialization;

namespace CeorgLsp.Rpc
{
    public record TextDocument
    {
        [JsonPropertyName("uri")]
        public required Uri Uri { get; init; }
    }

    public record Location
    {
        [JsonPropertyName("uri")]
        public required string Uri { get; init; }

        [JsonPropertyName("range")]
        public required Range Range { get; init; }
    }

    public record Range
    {
        [JsonPropertyName("start")]
        public required Position Start { get; init; }

        [JsonPropertyName("end")]
        public required Position End { get; init; }
    }

    public record Position
    {
        [JsonPropertyName("line")]
        public required uint Line { get; init; }

        [JsonPropertyName("character")]
        public required uint Character { get; init; }
    }

    public record MarkupContent
    {
        [JsonPropertyName("kind")]
        public required string MarkupKind { get; init; }

        [JsonPropertyName("value")]
        public required string Value { get; init; }
    }

    public enum MarkupKind
    {
        plaintext,
        markdown
    }
}
