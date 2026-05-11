using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class HoverHandlerTests
{
    [Fact]
    public async Task ShouldReturnNullHoverForUnresolvedTarget()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-hover-missing").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "source.norg");
            File.WriteAllText(sourcePath, "See {:missing:}[Missing]");

            LanguageServerState state = new();
            _ = await state.UpdateDocument(new Uri(Path.GetFullPath(sourcePath)));
            RpcMessage request = CreateHoverRequest(sourcePath, 0, 8, 1);

            HoverHandler handler = new(state, request);
            Response? response = await handler.HandleRequest();

            response.Should().NotBeNull();
            response!.Result.Should().NotBeNull();
            response.Result!.Value.ValueKind.Should().Be(JsonValueKind.Null);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ShouldReturnHoverPreviewForResolvedTarget()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-hover-hit").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "source.norg");
            string targetPath = Path.Combine(tempDir, "target.norg");
            File.WriteAllText(sourcePath, "See {:target:}[Target]");
            File.WriteAllText(targetPath, "@document.meta\ntitle: Target\n@end\n\nBody line");

            LanguageServerState state = new();
            _ = await state.UpdateDocument(new Uri(Path.GetFullPath(sourcePath)));
            _ = await state.UpdateDocument(new Uri(Path.GetFullPath(targetPath)));
            RpcMessage request = CreateHoverRequest(sourcePath, 0, 8, 2);

            HoverHandler handler = new(state, request);
            Response? response = await handler.HandleRequest();

            response.Should().NotBeNull();
            response!.Result.Should().NotBeNull();
            string value = response.Result!.Value.GetProperty("contents").GetProperty("value").GetString()!;
            value.Should().Contain("title: Target");
            value.Should().Contain("Body line");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static RpcMessage CreateHoverRequest(string path, uint line, uint character, int id)
    {
        return new()
        {
            Id = id,
            JsonRpc = "2.0",
            Method = HandlerFactory.MethodType.Hover,
            Params = JsonSerializer.SerializeToElement(
                new
                {
                    textDocument = new { uri = new Uri(Path.GetFullPath(path)).AbsoluteUri },
                    position = new { line, character },
                }
            ),
        };
    }
}
