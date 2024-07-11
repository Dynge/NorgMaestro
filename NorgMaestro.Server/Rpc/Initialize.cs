using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Rpc;

public record InitializeRequest : RpcMessage
{
    [JsonPropertyName("id")]
    public new required int Id { get; init; }

    [JsonPropertyName("params")]
    public new InitializeRequestParams Params { get; init; }

    public static InitializeRequest From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Id = message.Id!.Value,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<InitializeRequestParams>()
        };
    }
}

public readonly record struct InitializeRequestParams
{
    [JsonPropertyName("capabilities")]
    public JsonElement? Capabilities { get; init; }

    [JsonPropertyName("rootUri")]
    public Uri RootUri { get; init; }

    [JsonPropertyName("processId")]
    public int? ProcessId { get; init; }
}

public readonly record struct CompletionOptions
{
    [JsonPropertyName("resolveProvider")]
    public bool? ResolveProvider { get; init; }

    [JsonPropertyName("triggerCharacters")]
    public char[]? TriggerCharacters { get; init; }
}

public readonly record struct ServerCapabilities
{
    [JsonPropertyName("completionProvider")]
    public CompletionOptions? CompletionProvider { get; init; }

    [JsonPropertyName("workspaceSymbolProvider")]
    public bool? WorkspaceSymbolProvider { get; init; }

    [JsonPropertyName("referencesProvider")]
    public bool? ReferencesProvider { get; init; }

    [JsonPropertyName("callHierarchyProvider")]
    public bool? CallHierarchyProvider { get; init; }

    [JsonPropertyName("hoverProvider")]
    public bool? HoverProvider { get; init; }

    [JsonPropertyName("renameProvider")]
    public bool? RenameProvider { get; init; }
}

public readonly record struct InitializeResultParams
{
    [JsonPropertyName("capabilities")]
    public ServerCapabilities? Capabilities { get; init; }
}
