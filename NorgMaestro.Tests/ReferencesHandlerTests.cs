using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class ReferencesHandlerTests
{
    [Fact]
    public void ShouldReturnReferencesWhenCursorOnTitleMetadata()
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
            state.UpdateDocument(targetUri);
            state.UpdateDocument(sourceUri);

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
            Response? response = handler.HandleRequest();
            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            Location[]? locations = result.Deserialize<Location[]>();

            locations.Should().NotBeNull();
            locations!.Should().HaveCount(2);
            locations.Select(a => a.Uri).Should().Contain(sourceUri.AbsoluteUri);
            locations.Select(a => a.Uri).Should().Contain(targetUri.AbsoluteUri);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
