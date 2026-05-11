using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class ReferencesHandlerTests
{
    [Fact]
    public async Task ShouldReturnReferencesWhenCursorOnTitleMetadata()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-references-title").FullName;
        try
        {
            string targetPath = Path.Combine(tempDir, "202601060101.norg");
            string sourcePath = Path.Combine(tempDir, "202601060102.norg");
            File.WriteAllText(targetPath, "@document.meta\ntitle: Main Note\n@end\n");
            File.WriteAllText(sourcePath, "@document.meta\ntitle: Ref Note\n@end\n\nSee {:202601060101:}[Main Note]");

            Uri targetUri = new(Path.GetFullPath(targetPath));
            Uri sourceUri = new(Path.GetFullPath(sourcePath));

            LanguageServerState state = new();
            _ = await state.UpdateDocument(targetUri);
            _ = await state.UpdateDocument(sourceUri);

            RpcMessage request = new()
            {
                Id = 91,
                JsonRpc = "2.0",
                Method = "textDocument/references",
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new { uri = targetUri.AbsoluteUri },
                        position = new { line = 1, character = 9 },
                        context = new { includeDeclaration = true }
                    }
                )
            };

            ReferencesHandler handler = new(state, request);
            Response? response = await handler.HandleRequest();
            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            Location[]? locations = result.Deserialize<Location[]>();

            locations.Should().NotBeNull();
            Location[] refs = locations!;
            refs.Should().HaveCount(2);
            refs.Select(a => a.Uri).Should().Contain(sourceUri.AbsoluteUri);
            refs.Select(a => a.Uri).Should().Contain(targetUri.AbsoluteUri);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ShouldReturnEmptyArrayWhenNoReferenceTargetFound()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-references-empty").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601060103.norg");
            File.WriteAllText(sourcePath, "plain text without links\n");

            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            LanguageServerState state = new();
            _ = await state.UpdateDocument(sourceUri);

            RpcMessage request = new()
            {
                Id = 95,
                JsonRpc = "2.0",
                Method = "textDocument/references",
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new { uri = sourceUri.AbsoluteUri },
                        position = new { line = 0, character = 2 },
                        context = new { includeDeclaration = false }
                    }
                )
            };

            ReferencesHandler handler = new(state, request);
            Response? response = await handler.HandleRequest();
            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            Location[]? locations = result.Deserialize<Location[]>();

            locations.Should().NotBeNull();
            locations.Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ShouldResolveReferencesFromStateWhenSourceFileRemoved()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-references-state").FullName;
        try
        {
            string targetPath = Path.Combine(tempDir, "202601060104.norg");
            string sourcePath = Path.Combine(tempDir, "202601060105.norg");
            File.WriteAllText(targetPath, "@document.meta\ntitle: Main Note\n@end\n");
            File.WriteAllText(sourcePath, "@document.meta\ntitle: Ref Note\n@end\n\nSee {:202601060104:}[Main Note]");

            Uri targetUri = new(Path.GetFullPath(targetPath));
            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            LanguageServerState state = new();
            _ = await state.UpdateDocument(targetUri);
            _ = await state.UpdateDocument(sourceUri);

            File.Delete(sourcePath);

            RpcMessage request = new()
            {
                Id = 96,
                JsonRpc = "2.0",
                Method = "textDocument/references",
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new { uri = sourceUri.AbsoluteUri },
                        position = new { line = 4, character = 8 },
                        context = new { includeDeclaration = false }
                    }
                )
            };

            ReferencesHandler handler = new(state, request);
            Response? response = await handler.HandleRequest();
            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            Location[]? locations = result.Deserialize<Location[]>();

            locations.Should().NotBeNull();
            locations!.Should().ContainSingle(location => location.Uri == sourceUri.AbsoluteUri);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
