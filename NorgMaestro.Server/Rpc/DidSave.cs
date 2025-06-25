using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record DidSaveNotification : RpcMessage
{
    [JsonPropertyName("params")]
    public new required DidSaveRequestParams Params { get; init; }

    public static DidSaveNotification From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<DidSaveRequestParams>()!,
        };
    }
}

public record DidSaveRequestParams
{
    [JsonPropertyName("textDocument")]
    public required TextDocument TextDocument { get; init; }
}
