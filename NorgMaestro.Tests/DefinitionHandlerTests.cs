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

    [Fact]
    public void ShouldResolveWorkspaceRelativeLinks()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-definition-root").FullName;
        try
        {
            string notesDir = Path.Combine(tempDir, "notes");
            Directory.CreateDirectory(notesDir);

            string targetPath = Path.Combine(notesDir, "202601010201.norg");
            string sourcePath = Path.Combine(notesDir, "202601010202.norg");

            File.WriteAllText(targetPath, "@document.meta\ntitle: Root Target\n@end\n");
            File.WriteAllText(sourcePath, "@document.meta\ntitle: Root Source\n@end\n\nSee {:$/notes/202601010201:}[Root Target]");

            Uri rootUri = new(Path.GetFullPath(tempDir));
            Uri sourceUri = new(Path.GetFullPath(sourcePath));

            LanguageServerState state = new();
            state.Initialize(rootUri);

            RpcMessage request = new()
            {
                Id = 43,
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
            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            Location[]? locations = result.Deserialize<Location[]>();

            locations.Should().NotBeNull();
            locations!.Should().ContainSingle();
            Location location = locations[0] ?? throw new Xunit.Sdk.XunitException("Missing location");
            location.Uri.Should().Contain("202601010201.norg");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ShouldResolveNamedWorkspaceLinks()
    {
        string rootDir = Directory.CreateTempSubdirectory("norgmaestro-root").FullName;
        string gtdDir = Directory.CreateTempSubdirectory("norgmaestro-gtd").FullName;
        try
        {
            string sourcePath = Path.Combine(rootDir, "202601010301.norg");
            string targetPath = Path.Combine(gtdDir, "202601010302.norg");

            File.WriteAllText(targetPath, "@document.meta\ntitle: Gtd Target\n@end\n");
            File.WriteAllText(sourcePath, "@document.meta\ntitle: Source\n@end\n\nSee {:$gtd/202601010302:}[Gtd Target]");

            Uri rootUri = new(Path.GetFullPath(rootDir));
            Uri sourceUri = new(Path.GetFullPath(sourcePath));

            LanguageServerState state = new();
            state.Initialize(
                rootUri,
                [
                    new WorkspaceFolder() { Name = "main", Uri = rootUri },
                    new WorkspaceFolder() { Name = "gtd", Uri = new Uri(Path.GetFullPath(gtdDir)) }
                ]
            );
            state.UpdateDocument(new Uri(Path.GetFullPath(targetPath)));

            RpcMessage request = new()
            {
                Id = 44,
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
            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            Location[]? locations = result.Deserialize<Location[]>();
            locations.Should().NotBeNull();
            locations!.Should().ContainSingle();
            Location location = locations[0] ?? throw new Xunit.Sdk.XunitException("Missing location");
            location.Uri.Should().Contain("202601010302.norg");
        }
        finally
        {
            Directory.Delete(rootDir, true);
            Directory.Delete(gtdDir, true);
        }
    }
}
