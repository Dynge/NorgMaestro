using System.Text.Json.Serialization;

namespace NorgMaestro.Server.Rpc;

public record PublishDiagnosticsParams
{
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("diagnostics")]
    public required Diagnostic[] Diagnostics { get; init; }
}

public record Diagnostic
{
    [JsonPropertyName("range")]
    public required TextRange Range { get; init; }

    [JsonPropertyName("severity")]
    public required DiagnosticSeverity Severity { get; init; }

    [JsonPropertyName("source")]
    public string? Source { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}

public enum DiagnosticSeverity
{
    Error = 1,
    Warning = 2,
    Information = 3,
    Hint = 4,
}
