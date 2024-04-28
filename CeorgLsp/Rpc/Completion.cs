using System.Text.Json;
using System.Text.Json.Serialization;

namespace CeorgLsp.Rpc
{
    public readonly record struct CompletionRequestParams
    {
        [JsonPropertyName("textDocument")]
        public required Uri TextDocument { get; init; }

        [JsonPropertyName("postion")]
        public required Position Postion { get; init; }

        [JsonPropertyName("completionContext")]
        public JsonElement? CompletionContext { get; init; }
    }

    public readonly record struct Position
    {
        [JsonPropertyName("line")]
        public required uint Line { get; init; }

        [JsonPropertyName("character")]
        public required uint Character { get; init; }
    }

    public readonly record struct CompletionResult
    {
        [JsonPropertyName("result")]
        public required CompletionItem[] Result { get; init; }
    }

    public readonly record struct CompletionItem
    {
        [JsonPropertyName("label")]
        public required string Label { get; init; }

        [JsonPropertyName("documentation")]
        public MarkupContent? LabelDetails { get; init; }
    }

    public readonly record struct MarkupContent
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
