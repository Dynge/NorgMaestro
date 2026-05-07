using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record PrepareRenameRequest : RpcMessage
{
    [JsonPropertyName("id")]
    public new required int Id { get; init; }

    [JsonPropertyName("params")]
    public new required PrepareRenameRequestParams Params { get; init; }

    public static PrepareRenameRequest From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Id = message.Id!.Value,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<PrepareRenameRequestParams>()!
        };
    }
}

public record PrepareRenameRequestParams
{
    [JsonPropertyName("textDocument")]
    public required TextDocument TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required Position Position { get; init; }
}

public record RenameOptions
{
    [JsonPropertyName("prepareProvider")]
    public bool? PrepareProvider { get; init; }
}
