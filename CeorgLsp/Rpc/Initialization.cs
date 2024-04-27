using System.Text.Json;
using System.Text.Json.Serialization;

namespace CeorgLsp.Rpc
{
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

    public record InitializeResult
    {
        [JsonPropertyName("capabilities")]
        public ServerCapabilities? Capabilities { get; init; }
    }

    public record InitializeResultParams
    {
        [JsonPropertyName("result")]
        public InitializeResult? Result { get; init; }
    }
}
