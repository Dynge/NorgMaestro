using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record InitializeRequest : RpcMessage
{
    [JsonPropertyName("id")]
    public new required int Id { get; init; }

    [JsonPropertyName("params")]
    public new required InitializeRequestParams Params { get; init; }

    public static InitializeRequest From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Id = message.Id!.Value,
            Method = message.Method,
            Params =
                message.Params!.Value.Deserialize<InitializeRequestParams>()
                ?? throw new ArgumentException($"Invalid params object: {message.Params}"),
        };
    }
}

public record InitializeRequestParams
{
    [JsonPropertyName("capabilities")]
    public JsonElement? Capabilities { get; init; }

    [JsonPropertyName("rootUri")]
    public Uri? RootUri { get; init; } = null;

    [JsonPropertyName("rootPath")]
    public string? RootPath { get; init; } = null;

    [JsonPropertyName("workspaceFolders")]
    public IEnumerable<WorkspaceFolder>? WorkspaceFolders { get; init; } = null;

    [JsonPropertyName("processId")]
    public int? ProcessId { get; init; }

    [JsonPropertyName("initializationOptions")]
    public InitializationOptions? InitializationOptions { get; init; }
}

public record InitializationOptions
{
    [JsonPropertyName("diagnostics")]
    public DiagnosticsInitializationOptions? Diagnostics { get; init; }
}

public record DiagnosticsInitializationOptions
{
    [JsonPropertyName("unresolvedLinkSeverity")]
    public string? UnresolvedLinkSeverity { get; init; }
}

public readonly record struct WorkspaceFolder
{
    [JsonPropertyName("uri")]
    public Uri Uri { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }
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
    public RenameOptions? RenameProvider { get; init; }

    [JsonPropertyName("definitionProvider")]
    public bool? DefinitionProvider { get; init; }

    [JsonPropertyName("documentSymbolProvider")]
    public bool? DocumentSymbolProvider { get; init; }

    [JsonPropertyName("documentLinkProvider")]
    public DocumentLinkOptions? DocumentLinkProvider { get; init; }

    [JsonPropertyName("codeActionProvider")]
    public CodeActionOptions? CodeActionProvider { get; init; }

    [JsonPropertyName("executeCommandProvider")]
    public ExecuteCommandOptions? ExecuteCommandProvider { get; init; }
}

public readonly record struct InitializeResultParams
{
    [JsonPropertyName("capabilities")]
    public ServerCapabilities? Capabilities { get; init; }
}
