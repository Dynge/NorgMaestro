using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record CompletionRequest : RpcMessage
{
    [JsonPropertyName("id")]
    public new required int Id { get; init; }

    [JsonPropertyName("params")]
    public new required CompletionRequestParams Params { get; init; }

    public static CompletionRequest From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Id = message.Id!.Value,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<CompletionRequestParams>()!,
        };
    }
}

public record CompletionRequestParams
{
    [JsonPropertyName("textDocument")]
    public required TextDocument TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required Position Position { get; init; }

    [JsonPropertyName("context")]
    public CompletionContext? CompletionContext { get; init; }
}

public record CompletionContext
{
    [JsonPropertyName("triggerKind")]
    public required CompletionTriggerKind TriggerKind { get; init; }

    [JsonPropertyName("triggerCharacter")]
    public char? TriggerCharacter { get; init; }
}

public enum CompletionTriggerKind
{
    Invoked = 1,
    TriggerCharacter = 2,
    TriggerForIncompleteCompletions = 3,
}

public record CompletionItem
{
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    [JsonPropertyName("documentation")]
    public MarkupContent? LabelDetails { get; init; }

    [JsonPropertyName("textEdit")]
    public TextEdit? TextEdit { get; init; }

    [JsonPropertyName("kind")]
    public CompletionKind? Kind { get; init; }
}

public enum CompletionKind
{
    Text = 1,
    Method = 2,
    Function = 3,
    Constructor = 4,
    Field = 5,
    Variable = 6,
    Class = 7,
    Interface = 8,
    Module = 9,
    Property = 10,
    Unit = 11,
    Value = 12,
    Enum = 13,
    Keyword = 14,
    Snippet = 15,
    Color = 16,
    File = 17,
    Reference = 18,
    Folder = 19,
    EnumMember = 20,
    Constant = 21,
    Struct = 22,
    Event = 23,
    Operator = 24,
    TypeParameter = 25,
}
