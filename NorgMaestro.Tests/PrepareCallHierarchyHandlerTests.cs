using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class PrepareCallHierarchyHandlerTests
{
    [Fact]
    public void ShouldReturnFileUriAsAbsoluteUri()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-prepare-call").FullName;
        try
        {
            string notePath = Path.Combine(tempDir, "note.norg");
            File.WriteAllText(notePath, "@document.meta\ntitle: Note\n@end\n");
            Uri noteUri = new(Path.GetFullPath(notePath));

            LanguageServerState state = new();
            state.UpdateDocument(noteUri);

            RpcMessage request = new()
            {
                Id = 61,
                JsonRpc = "2.0",
                Method = HandlerFactory.MethodType.PrepareCallHierarchy,
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new { uri = noteUri.AbsoluteUri },
                        position = new { line = 0, character = 0 }
                    }
                )
            };

            PrepareCallHierarchyHandler handler = new(state, request);
            Response? response = handler.HandleRequest();
            JsonElement[] items = response!.Result!.Value.Deserialize<JsonElement[]>()!;

            items.Should().HaveCount(1);
            items[0].GetProperty("uri").GetString().Should().Be(noteUri.AbsoluteUri);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
