using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Rpc;

public record PrepareCallHierarchyRequest : RpcMessage
{
    [JsonPropertyName("id")]
    public new required int Id { get; init; }

    [JsonPropertyName("params")]
    public new required PrepareCallHierarchyRequestParams Params { get; init; }

    public static PrepareCallHierarchyRequest From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Id = message.Id!.Value,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<PrepareCallHierarchyRequestParams>()!
        };
    }
}

public record PrepareCallHierarchyRequestParams
{
    [JsonPropertyName("textDocument")]
    public required TextDocument TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required Position Position { get; init; }
}
