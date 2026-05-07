using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class DefinitionHandlerTests
{
    [Fact]
    public void ShouldReturnDefinitionLocationForFileLink()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-definition").FullName;
        try
        {
            string targetPath = Path.Combine(tempDir, "202601010101.norg");
            string sourcePath = Path.Combine(tempDir, "202601010102.norg");

            File.WriteAllText(
                targetPath,
                "@document.meta\ntitle: Target Note\n@end\n\nBody"
            );
            File.WriteAllText(
                sourcePath,
                "@document.meta\ntitle: Source Note\n@end\n\nSee {:202601010101:}[Target Note]"
            );

            Uri targetUri = new(Path.GetFullPath(targetPath));
            Uri sourceUri = new(Path.GetFullPath(sourcePath));

            LanguageServerState state = new();
            state.UpdateDocument(targetUri);
            state.UpdateDocument(sourceUri);

            RpcMessage request = new()
            {
                Id = 42,
                JsonRpc = "2.0",
                Method = "textDocument/definition",
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new { uri = sourceUri.AbsoluteUri },
                        position = new { line = 4, character = 10 }
                    }
                )
            };

            DefinitionHandler handler = new(state, request);
            Response? response = handler.HandleRequest();

            response.Should().NotBeNull();
            response!.Result.Should().NotBeNull();

            JsonElement result = response.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            Location[]? locations = result.Deserialize<Location[]>();
            locations.Should().NotBeNull();
            locations!.Should().ContainSingle();
            Location location = locations[0] ?? throw new Xunit.Sdk.XunitException("Missing location");
            location.Uri.Should().Be(targetUri.AbsoluteUri);
            location.Range.Start.Line.Should().Be(1);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
