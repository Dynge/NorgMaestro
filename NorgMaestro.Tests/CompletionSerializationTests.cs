using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class CompletionSerializationTests
{
    [Fact]
    public void ShouldOmitNullTextEditFromCompletionItem()
    {
        CompletionItem item = new() { Label = "Category" };

        JsonElement result = JsonSerializer.SerializeToElement(item, JsonOptions.Default);
        result.TryGetProperty("textEdit", out _).Should().BeFalse();
    }

    [Fact]
    public void ShouldSerializeTextEditWhenPresent()
    {
        CompletionItem item = new()
        {
            Label = "Link",
            Kind = CompletionKind.File,
            TextEdit = new()
            {
                NewText = "{:target:}[Target]",
                Range = new()
                {
                    Start = new() { Line = 2, Character = 4 },
                    End = new() { Line = 2, Character = 10 },
                }
            }
        };

        JsonElement result = JsonSerializer.SerializeToElement(item, JsonOptions.Default);
        result.TryGetProperty("textEdit", out JsonElement textEdit).Should().BeTrue();
        textEdit.ValueKind.Should().Be(JsonValueKind.Object);
    }
}
