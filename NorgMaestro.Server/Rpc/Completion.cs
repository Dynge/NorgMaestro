using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Rpc;

public record CompletionRequest : RpcMessage
{
    [JsonPropertyName("id")]
    public new required int Id { get; init; }

    [JsonPropertyName("params")]
    public new required CompletionRequestParams Params { get; init; }

    public static CompletionRequest From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Id = message.Id!.Value,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<CompletionRequestParams>()!
        };
    }
}

public record CompletionRequestParams
{
    [JsonPropertyName("textDocument")]
    public required TextDocument TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required Position Postion { get; init; }

    [JsonPropertyName("completionContext")]
    public JsonElement? CompletionContext { get; init; }
}

public record CompletionItem
{
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    [JsonPropertyName("documentation")]
    public MarkupContent? LabelDetails { get; init; }
}
