using System.Text.Json.Serialization;

namespace NorgMaestro.Rpc;

public record TextDocument
{
    [JsonPropertyName("uri")]
    public required Uri Uri { get; init; }
}

public record ReferenceLocation
{
    public required string Line { get; init; }
    public required Location Location { get; init; }
}

public record Location
{
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("range")]
    public required TextRange Range { get; init; }
}

public record TextRange
{
    [JsonPropertyName("start")]
    public required Position Start { get; init; }

    [JsonPropertyName("end")]
    public required Position End { get; init; }
}

public record Position
{
    [JsonPropertyName("line")]
    public required uint Line { get; init; }

    [JsonPropertyName("character")]
    public required uint Character { get; init; }
}

public record MarkedString
{
    [JsonPropertyName("language")]
    public required string Language { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }
}

public record MarkupContent
{
    [JsonPropertyName("kind")]
    public required string MarkupKind { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }
}

public record MarkupKind
{
    public const string PlainText = "plaintext";
    public const string Markdown = "markdown";
}
