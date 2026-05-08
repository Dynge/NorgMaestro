using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record DidCloseNotification : RpcMessage
{
    [JsonPropertyName("params")]
    public new required DidCloseRequestParams Params { get; init; }

    public static DidCloseNotification From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<DidCloseRequestParams>()!
        };
    }
}

public record DidCloseRequestParams
{
    [JsonPropertyName("textDocument")]
    public required TextDocument TextDocument { get; init; }
}
