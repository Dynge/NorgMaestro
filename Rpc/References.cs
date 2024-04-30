using System.Text.Json;
using System.Text.Json.Serialization;

namespace CeorgLsp.Rpc
{
    public record ReferencesRequest : RpcMessage
    {
        [JsonPropertyName("id")]
        public new required int Id { get; init; }

        [JsonPropertyName("params")]
        public new required ReferencesRequestParams Params { get; init; }

        public static ReferencesRequest From(RpcMessage message)
        {
            return new()
            {
                JsonRpc = message.JsonRpc,
                Id = message.Id!.Value,
                Method = message.Method,
                Params = message.Params!.Value.Deserialize<ReferencesRequestParams>()!
            };
        }
    }

    public record ReferencesRequestParams
    {
        [JsonPropertyName("textDocument")]
        public required TextDocument TextDocument { get; init; }

        [JsonPropertyName("position")]
        public required Position Position { get; init; }

        [JsonPropertyName("context")]
        public required ReferenceContext Context { get; init; }
    }

    public record ReferenceContext
    {
        [JsonPropertyName("includeDeclaration")]
        public required bool IncludeDeclaration { get; init; }
    }
}
