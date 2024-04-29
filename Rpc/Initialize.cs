using System.Text.Json;
using System.Text.Json.Serialization;

namespace CeorgLsp.Rpc
{
    public record InitializeRequest : Request
    {
        [JsonPropertyName("params")]
        public new InitializeRequestParams? Params { get; init; }

        public static InitializeRequest From(RpcMessage message)
        {
            return new()
            {
                JsonRpc = message.JsonRpc,
                Id = message!.Id,
                Method = message.Method,
                Params = message.Params?.Deserialize<InitializeRequestParams>()
            };
        }
    }

    public record InitializeRequestParams
    {
        [JsonPropertyName("capabilities")]
        public JsonElement? Capabilities { get; init; }

        [JsonPropertyName("rootPath")]
        public string? RootPath { get; init; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; init; }
    }

    public record CompletionOptions
    {
        [JsonPropertyName("resolveProvider")]
        public bool? ResolveProvider { get; init; }

        [JsonPropertyName("triggerCharacters")]
        public char[]? TriggerCharacters { get; init; }
    }

    public record ServerCapabilities
    {
        [JsonPropertyName("completionProvider")]
        public CompletionOptions? CompletionProvider { get; init; }
    }

    public record InitializeResultParams
    {
        [JsonPropertyName("capabilities")]
        public ServerCapabilities? Capabilities { get; init; }
    }
}
