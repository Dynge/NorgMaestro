using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class IncomingCallsHandlerTests
{
    [Fact]
    public void ShouldGroupIncomingCallsBySourceDocument()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-incoming").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "source.norg");
            string targetPath = Path.Combine(tempDir, "target.norg");
            File.WriteAllText(
                sourcePath,
                "@document.meta\ntitle: Source\n@end\n\n- {:target:}[Target]\n- {:target:}[Target]"
            );
            File.WriteAllText(targetPath, "@document.meta\ntitle: Target\n@end\n");

            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            Uri targetUri = new(Path.GetFullPath(targetPath));
            LanguageServerState state = new();
            state.UpdateDocument(sourceUri);
            state.UpdateDocument(targetUri);

            RpcMessage request = new()
            {
                Id = 20,
                JsonRpc = "2.0",
                Method = HandlerFactory.MethodType.IncomingCalls,
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        item = new
                        {
                            name = "Target",
                            kind = SymbolKind.File,
                            uri = targetUri.AbsoluteUri,
                            range = new
                            {
                                start = new { line = 0, character = 0 },
                                end = new { line = 0, character = 0 }
                            },
                            selectionRange = new
                            {
                                start = new { line = 0, character = 0 },
                                end = new { line = 0, character = 0 }
                            }
                        }
                    }
                )
            };

            IncomingCallsHandler handler = new(state, request);
            Response? response = handler.HandleRequest().Result;

            JsonElement[] calls = response!.Result!.Value.Deserialize<JsonElement[]>()!;
            calls.Should().HaveCount(1);
            calls[0].GetProperty("from").GetProperty("uri").GetString().Should().Be(sourceUri.AbsoluteUri);
            calls[0].GetProperty("fromRanges").GetArrayLength().Should().Be(2);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
