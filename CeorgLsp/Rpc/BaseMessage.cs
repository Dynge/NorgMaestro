using System.Text.Json;
using System.Text.Json.Serialization;

namespace CeorgLsp.Rpc
{
    public record Message
    {
        [JsonPropertyName("jsonrpc")]
        public required string JsonRpc { get; init; }
    }

    public record MessageWithId : Message
    {
        [JsonPropertyName("id")]
        public required int Id { get; init; }
    }

    public record Request : MessageWithId
    {
        [JsonPropertyName("method")]
        public required string Method { get; init; }

        [JsonPropertyName("params")]
        public JsonElement? Params { get; init; }
    }

    public record Response : MessageWithId
    {
        [JsonPropertyName("result")]
        public JsonElement? Result { get; init; }

        [JsonPropertyName("error")]
        public JsonElement? Error { get; init; }

        public static Response OfSuccess(int id, object res)
        {
            return new()
            {
                JsonRpc = "2.0",
                Id = id,
                Result = JsonSerializer.SerializeToElement(res),
            };
        }

        public static Response OfError(int id, object err)
        {
            return new()
            {
                JsonRpc = "2.0",
                Id = id,
                Error = JsonSerializer.SerializeToElement(err)
            };
        }
    }

    public record RpcMessage
    {
        [JsonPropertyName("jsonrpc")]
        public required string JsonRpc { get; init; }

        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("method")]
        public required string Method { get; init; }

        [JsonPropertyName("params")]
        public JsonElement? Params { get; init; }
    }
}
