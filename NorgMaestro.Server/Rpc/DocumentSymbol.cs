using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record DocumentSymbolRequest : RpcMessage
{
    [JsonPropertyName("id")]
    public new required int Id { get; init; }

    [JsonPropertyName("params")]
    public new required DocumentSymbolRequestParams Params { get; init; }

    public static DocumentSymbolRequest From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Id = message.Id!.Value,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<DocumentSymbolRequestParams>()!
        };
    }
}

public record DocumentSymbolRequestParams
{
    [JsonPropertyName("textDocument")]
    public required TextDocument TextDocument { get; init; }
}

public record DocumentSymbol
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("kind")]
    public required SymbolKind Kind { get; init; }

    [JsonPropertyName("range")]
    public required TextRange Range { get; init; }

    [JsonPropertyName("selectionRange")]
    public required TextRange SelectionRange { get; init; }

    [JsonPropertyName("children")]
    public DocumentSymbol[]? Children { get; init; }
}
