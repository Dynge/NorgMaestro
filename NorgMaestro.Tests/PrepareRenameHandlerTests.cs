using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class PrepareRenameHandlerTests
{
    [Fact]
    public async Task ShouldReturnLinkTextRangeWhenPrepareRenameOnLink()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-prepare-rename").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601050101.norg");
            File.WriteAllText(
                sourcePath,
                "@document.meta\ntitle: Source\n@end\n\nSee {:202601050102:}[Old Title]"
            );

            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            RpcMessage request = new()
            {
                Id = 88,
                JsonRpc = "2.0",
                Method = "textDocument/prepareRename",
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new { uri = sourceUri.AbsoluteUri },
                        position = new { line = 4, character = 25 }
                    }
                )
            };

            PrepareRenameHandler handler = new(new LanguageServerState(), request);
            Response? response = await handler.HandleRequest();

            response.Should().NotBeNull();
            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            TextRange? range = result.Deserialize<TextRange>();

            range.Should().NotBeNull();
            range!.Start.Line.Should().Be(4);
            range.Start.Character.Should().BeGreaterThan(0);
            range.End.Character.Should().BeGreaterThan(range.Start.Character);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ShouldReturnTitleRangeWhenPrepareRenameOnMetadataTitle()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-prepare-rename-title").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601050102.norg");
            File.WriteAllText(sourcePath, "@document.meta\ntitle: My Title\n@end\n");

            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            LanguageServerState state = new();
            _ = await state.UpdateDocument(sourceUri);

            RpcMessage request = new()
            {
                Id = 89,
                JsonRpc = "2.0",
                Method = "textDocument/prepareRename",
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new { uri = sourceUri.AbsoluteUri },
                        position = new { line = 1, character = 9 }
                    }
                )
            };

            PrepareRenameHandler handler = new(state, request);
            Response? response = await handler.HandleRequest();

            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            TextRange? range = result.Deserialize<TextRange>();
            range.Should().NotBeNull();
            range!.Start.Line.Should().Be(1);
            range.Start.Character.Should().Be(7);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ShouldReturnFileRangeWhenCursorInsideLinkTarget()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-prepare-rename-file-range").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601050103.norg");
            File.WriteAllText(sourcePath, "See {:202601010101:}[Target]\n");

            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            LanguageServerState state = new();
            _ = await state.UpdateDocument(sourceUri);

            RpcMessage request = new()
            {
                Id = 90,
                JsonRpc = "2.0",
                Method = "textDocument/prepareRename",
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new { uri = sourceUri.AbsoluteUri },
                        position = new { line = 0, character = 7 }
                    }
                )
            };

            PrepareRenameHandler handler = new(state, request);
            Response? response = await handler.HandleRequest();

            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            TextRange? range = result.Deserialize<TextRange>();
            range.Should().NotBeNull();
            range!.Start.Line.Should().Be(0);
            range.Start.Character.Should().Be(6);
            range.End.Character.Should().Be(18);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
