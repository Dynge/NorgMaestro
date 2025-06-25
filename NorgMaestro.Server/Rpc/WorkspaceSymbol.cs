using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record WorkspaceSymbolRequest : RpcMessage
{
    [JsonPropertyName("id")]
    public new required int Id { get; init; }

    [JsonPropertyName("params")]
    public new required WorkspaceSymbolRequestParams Params { get; init; }

    public static WorkspaceSymbolRequest From(RpcMessage message)
    {
        return new()
        {
            JsonRpc = message.JsonRpc,
            Id = message.Id!.Value,
            Method = message.Method,
            Params = message.Params!.Value.Deserialize<WorkspaceSymbolRequestParams>()!,
        };
    }
}

public record WorkspaceSymbolRequestParams
{
    [JsonPropertyName("query")]
    public required string Query { get; init; }
}

public enum SymbolKind
{
    File = 1,
    Module = 2,
    Namespace = 3,
    Package = 4,
    Class = 5,
    Method = 6,
    Property = 7,
    Field = 8,
    Constructor = 9,
    Enum = 10,
    Interface = 11,
    Function = 12,
    Variable = 13,
    Constant = 14,
    StringKind = 15,
    Number = 16,
    Boolean = 17,
    Array = 18,
    ObjectKind = 19,
    Key = 20,
    Null = 21,
    EnumMember = 22,
    Struct = 23,
    Event = 24,
    Operator = 25,
    TypeParameter = 26,
}

public record WorkspaceSymbol
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("kind")]
    public required SymbolKind Kind { get; init; }

    [JsonPropertyName("containerName")]
    public string? ContainerName { get; init; }

    [JsonPropertyName("location")]
    public required Location Location { get; init; }
}
