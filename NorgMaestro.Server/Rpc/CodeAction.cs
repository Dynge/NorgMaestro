using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record CodeActionRequest : RpcMessage
{
    [JsonPropertyName("id")]
    public new required int Id { get; init; }

    [JsonPropertyName("params")]
    public new required CodeActionRequestParams Params { get; init; }

    public static CodeActionRequest From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Id = message.Id!.Value,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<CodeActionRequestParams>()!
        };
    }
}

public record CodeActionRequestParams
{
    [JsonPropertyName("textDocument")]
    public required TextDocument TextDocument { get; init; }

    [JsonPropertyName("range")]
    public required TextRange Range { get; init; }

    [JsonPropertyName("context")]
    public required CodeActionContext Context { get; init; }
}

public record CodeActionContext
{
    [JsonPropertyName("diagnostics")]
    public required Diagnostic[] Diagnostics { get; init; }
}

public record CodeAction
{
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("kind")]
    public string? Kind { get; init; }

    [JsonPropertyName("command")]
    public RpcCommand? Command { get; init; }

    [JsonPropertyName("edit")]
    public WorkspaceEdit? Edit { get; init; }

    [JsonPropertyName("isPreferred")]
    public bool? IsPreferred { get; init; }
}

public record RpcCommand
{
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("command")]
    public required string Command { get; init; }

    [JsonPropertyName("arguments")]
    public object[]? Arguments { get; init; }
}

public record CodeActionKind
{
    public const string QuickFix = "quickfix";
    public const string RefactorRewrite = "refactor.rewrite";
}

public record CodeActionOptions
{
    [JsonPropertyName("codeActionKinds")]
    public string[]? CodeActionKinds { get; init; }
}
