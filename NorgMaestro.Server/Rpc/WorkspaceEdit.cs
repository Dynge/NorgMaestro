using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record WorkspaceEdit
{
    [JsonPropertyName("changes")]
    public required Dictionary<string, TextEdit[]> Changes { get; init; }
}

public record TextEdit
{
    [JsonPropertyName("range")]
    public required TextRange Range { get; init; }

    [JsonPropertyName("newText")]
    public required string NewText { get; init; }
}
