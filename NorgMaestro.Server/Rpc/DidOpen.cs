using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record DidOpenNotification : RpcMessage
{
    [JsonPropertyName("params")]
    public new required DidOpenRequestParams Params { get; init; }

    public static DidOpenNotification From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<DidOpenRequestParams>()!
        };
    }
}

public record DidOpenRequestParams
{
    [JsonPropertyName("textDocument")]
    public required OpenTextDocument TextDocument { get; init; }
}

public record OpenTextDocument
{
    [JsonPropertyName("uri")]
    public required Uri Uri { get; init; }

    [JsonPropertyName("languageId")]
    public string? LanguageId { get; init; }

    [JsonPropertyName("version")]
    public int? Version { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }
}
