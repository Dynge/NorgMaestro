using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class PrepareRenameHandlerTests
{
    [Fact]
    public void ShouldReturnLinkTextRangeWhenPrepareRenameOnLink()
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

            PrepareRenameHandler handler = new(request);
            Response? response = handler.HandleRequest();

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
}
