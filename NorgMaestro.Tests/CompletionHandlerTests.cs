using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class CompletionHandlerTests
{
    [Fact]
    public void ShouldCompleteLinksInsideBracesAndReplaceWholeLink()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-completion-link").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601090101.norg");
            string targetPath = Path.Combine(tempDir, "202601090102.norg");

            File.WriteAllText(sourcePath, "- {:t:}[Writing]\n");
            File.WriteAllText(targetPath, "@document.meta\ntitle: Target Note\n@end\n");

            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            LanguageServerState state = new();
            state.UpdateDocument(sourceUri);
            state.UpdateDocument(new Uri(Path.GetFullPath(targetPath)));

            RpcMessage request = new()
            {
                Id = 96,
                JsonRpc = "2.0",
                Method = "textDocument/completion",
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new { uri = sourceUri.AbsoluteUri },
                        position = new { line = 0, character = 5 },
                        context = new { triggerKind = 1 }
                    }
                )
            };

            CompletionHandler handler = new(state, request);
            Response? response = handler.HandleRequest().Result;

            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            CompletionItem[]? items = result.Deserialize<CompletionItem[]>();

            items.Should().NotBeNull();
            CompletionItem item = items!.First(i => i.Label == "Target Note");
            item.TextEdit.Should().NotBeNull();
            item.TextEdit!.NewText.Should().Be("{:202601090102:}[Target Note]");
            item.TextEdit.Range.Start.Character.Should().Be(2);
            item.TextEdit.Range.End.Character.Should().Be(16);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ShouldReturnCategoryCompletionsOutsideLinkBraces()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-completion-category").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601090201.norg");
            string targetPath = Path.Combine(tempDir, "202601090202.norg");

            File.WriteAllText(sourcePath, "- normal text\n");
            File.WriteAllText(targetPath, "@document.meta\ncategories: [\n  zettel\n]\n@end\n");

            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            LanguageServerState state = new();
            state.UpdateDocument(sourceUri);
            state.UpdateDocument(new Uri(Path.GetFullPath(targetPath)));

            RpcMessage request = new()
            {
                Id = 97,
                JsonRpc = "2.0",
                Method = "textDocument/completion",
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new { uri = sourceUri.AbsoluteUri },
                        position = new { line = 0, character = 3 },
                        context = new { triggerKind = 1 }
                    }
                )
            };

            CompletionHandler handler = new(state, request);
            Response? response = handler.HandleRequest().Result;

            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            CompletionItem[]? items = result.Deserialize<CompletionItem[]>();

            items.Should().NotBeNull();
            items!.Select(i => i.Label.Trim()).Should().Contain("zettel");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
