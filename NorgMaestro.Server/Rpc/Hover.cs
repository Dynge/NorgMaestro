using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Rpc;

public record HoverRequest : RpcMessage
{
    [JsonPropertyName("id")]
    public new required int Id { get; init; }

    [JsonPropertyName("params")]
    public new required HoverRequestParams Params { get; init; }

    public static HoverRequest From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Id = message.Id!.Value,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<HoverRequestParams>()!
        };
    }
}

public record HoverRequestParams
{
    [JsonPropertyName("textDocument")]
    public required TextDocument TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required Position Postion { get; init; }
}

public record Hover
{
    [JsonPropertyName("contents")]
    public required MarkedString? Contents { get; init; }

    [JsonPropertyName("range")]
    public TextRange? Range { get; init; }
}
