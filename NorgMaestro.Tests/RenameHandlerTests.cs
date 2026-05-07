using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class RenameHandlerTests
{
    [Fact]
    public void ShouldUpdateStateTitleAfterRenameRequest()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-rename-state").FullName;
        try
        {
            string targetPath = Path.Combine(tempDir, "202601100101.norg");
            string sourcePath = Path.Combine(tempDir, "202601100102.norg");

            File.WriteAllText(targetPath, "@document.meta\ntitle: test\n@end\n");
            File.WriteAllText(sourcePath, "@document.meta\ntitle: source\n@end\n\nSee {:202601100101:}[test]\n");

            Uri targetUri = new(Path.GetFullPath(targetPath));
            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            LanguageServerState state = new();
            state.UpdateDocument(targetUri);
            state.UpdateDocument(sourceUri);

            RpcMessage request = new()
            {
                Id = 98,
                JsonRpc = "2.0",
                Method = "textDocument/rename",
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new { uri = sourceUri.AbsoluteUri },
                        position = new { line = 4, character = 22 },
                        newName = "testA"
                    }
                )
            };

            RenameHandler handler = new(state, request);
            _ = handler.HandleRequest();

            state.Documents[targetUri].Metadata.Title.Should().NotBeNull();
            state.Documents[targetUri].Metadata.Title!.Name.Should().Be("testA");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
