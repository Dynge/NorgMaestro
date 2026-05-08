using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record DidChangeNotification : RpcMessage
{
    [JsonPropertyName("params")]
    public new required DidChangeRequestParams Params { get; init; }

    public static DidChangeNotification From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<DidChangeRequestParams>()!
        };
    }
}

public record DidChangeRequestParams
{
    [JsonPropertyName("textDocument")]
    public required VersionedTextDocument TextDocument { get; init; }

    [JsonPropertyName("contentChanges")]
    public required TextDocumentContentChangeEvent[] ContentChanges { get; init; }
}

public record VersionedTextDocument
{
    [JsonPropertyName("uri")]
    public required Uri Uri { get; init; }

    [JsonPropertyName("version")]
    public int? Version { get; init; }
}

public record TextDocumentContentChangeEvent
{
    [JsonPropertyName("range")]
    public TextRange? Range { get; init; }

    [JsonPropertyName("rangeLength")]
    public uint? RangeLength { get; init; }

    [JsonPropertyName("text")]
    public required string Text { get; init; }
}
