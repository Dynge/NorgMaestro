using System.Text.Json;
using System.Text.Json.Serialization;

namespace CeorgLsp.Rpc
{
    public record Message
    {
        [JsonPropertyName("jsonrpc")]
        public const string JsonRpc = "2.0";
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

        public static Response From(int id, object? res, object? err)
        {
            return new()
            {
                Id = id,
                Result = JsonSerializer.SerializeToElement(res),
                Error = JsonSerializer.SerializeToElement(err)
            };
        }
    }
}
