using System.Text.Json;
using System.Text.Json.Serialization;

namespace CeorgLsp.Rpc
{
    public record CompletionRequest : Request
    {
        [JsonPropertyName("params")]
        public new CompletionRequestParams Params { get; init; }

        public static CompletionRequest From(RpcMessage message)
        {
            return new()
            {
                JsonRpc = message.JsonRpc,
                Id = message.Id!.Value,
                Method = message.Method,
                Params = message.Params!.Value.Deserialize<CompletionRequestParams>()
            };
        }
    }

    public readonly record struct CompletionRequestParams
    {
        [JsonPropertyName("textDocument")]
        public required TextDocument TextDocument { get; init; }

        [JsonPropertyName("position")]
        public required Position Postion { get; init; }

        [JsonPropertyName("completionContext")]
        public JsonElement? CompletionContext { get; init; }
    }

    public readonly record struct TextDocument
    {
        [JsonPropertyName("uri")]
        public required Uri Uri { get; init; }
    }

    public readonly record struct Position
    {
        [JsonPropertyName("line")]
        public required uint Line { get; init; }

        [JsonPropertyName("character")]
        public required uint Character { get; init; }
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
