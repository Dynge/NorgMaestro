using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record OutgoingCallsRequest : RpcMessage
{
    [JsonPropertyName("id")]
    public new required int Id { get; init; }

    [JsonPropertyName("params")]
    public new required OutgoingCallsRequestParams Params { get; init; }

    public static OutgoingCallsRequest From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Id = message.Id!.Value,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<OutgoingCallsRequestParams>()!
        };
    }
}

public record OutgoingCallsRequestParams
{
    [JsonPropertyName("item")]
    public required CallHierarchyItem Item { get; init; }
}

public record OutgoingCallsResponseParams
{
    [JsonPropertyName("to")]
    public required CallHierarchyItem To { get; init; }

    [JsonPropertyName("fromRanges")]
    public required TextRange[] FromRanges { get; init; }
}
