using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record ExecuteCommandRequest : RpcMessage
{
    [JsonPropertyName("id")]
    public new int? Id { get; init; }

    [JsonPropertyName("params")]
    public new required ExecuteCommandRequestParams Params { get; init; }

    public static ExecuteCommandRequest From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Id = message.Id,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<ExecuteCommandRequestParams>()!
        };
    }
}

public record ExecuteCommandRequestParams
{
    [JsonPropertyName("command")]
    public required string Command { get; init; }

    [JsonPropertyName("arguments")]
    public JsonElement[]? Arguments { get; init; }
}

public record ExecuteCommandOptions
{
    [JsonPropertyName("commands")]
    public required string[] Commands { get; init; }
}
