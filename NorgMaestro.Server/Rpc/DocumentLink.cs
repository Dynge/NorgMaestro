using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record DocumentLinkRequest : RpcMessage
{
    [JsonPropertyName("id")]
    public new required int Id { get; init; }

    [JsonPropertyName("params")]
    public new required DocumentLinkRequestParams Params { get; init; }

    public static DocumentLinkRequest From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Id = message.Id!.Value,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<DocumentLinkRequestParams>()!
        };
    }
}

public record DocumentLinkRequestParams
{
    [JsonPropertyName("textDocument")]
    public required TextDocument TextDocument { get; init; }
}

public record DocumentLink
{
    [JsonPropertyName("range")]
    public required TextRange Range { get; init; }

    [JsonPropertyName("target")]
    public required string Target { get; init; }

    [JsonPropertyName("tooltip")]
    public string? Tooltip { get; init; }
}

public record DocumentLinkOptions
{
    [JsonPropertyName("resolveProvider")]
    public bool? ResolveProvider { get; init; }
}
