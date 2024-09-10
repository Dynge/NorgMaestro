using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record IncomingCallsRequest : RpcMessage
{
    [JsonPropertyName("id")]
    public new required int Id { get; init; }

    [JsonPropertyName("params")]
    public new required IncomingCallsRequestParams Params { get; init; }

    public static IncomingCallsRequest From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Id = message.Id!.Value,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<IncomingCallsRequestParams>()!
        };
    }
}

public record IncomingCallsRequestParams
{
    [JsonPropertyName("item")]
    public required CallHierarchyItem Item { get; init; }
}

public record IncomingCallsResponseParams
{
    [JsonPropertyName("from")]
    public required CallHierarchyItem From { get; init; }

    [JsonPropertyName("fromRanges")]
    public required TextRange[] FromRanges { get; init; }
}
