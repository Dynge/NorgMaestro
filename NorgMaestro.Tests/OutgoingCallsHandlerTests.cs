using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class OutgoingCallsHandlerTests
{
    [Fact]
    public void ShouldReturnOutgoingCallsFromCurrentDocumentLinks()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-outgoing").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601070101.norg");
            string targetPath = Path.Combine(tempDir, "202601070102.norg");
            File.WriteAllText(sourcePath, "@document.meta\ntitle: Source\n@end\n\nSee {:202601070102:}[Target]");
            File.WriteAllText(targetPath, "@document.meta\ntitle: Target\n@end\n");

            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            Uri targetUri = new(Path.GetFullPath(targetPath));
            LanguageServerState state = new();
            state.UpdateDocument(sourceUri);
            state.UpdateDocument(targetUri);

            RpcMessage request = new()
            {
                Id = 92,
                JsonRpc = "2.0",
                Method = "callHierarchy/outgoingCalls",
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        item = new
                        {
                            name = "Source",
                            kind = (int)SymbolKind.File,
                            uri = sourceUri.AbsoluteUri,
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

            OutgoingCallsHandler handler = new(state, request);
            Response? response = handler.HandleRequest();
            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            OutgoingCallsResponseParams[]? outgoing = result.Deserialize<OutgoingCallsResponseParams[]>();

            outgoing.Should().NotBeNull();
            OutgoingCallsResponseParams call = outgoing!.Should().ContainSingle().Subject;
            call.To.Uri.Should().Be(targetUri.AbsoluteUri);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
