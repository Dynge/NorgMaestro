using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class DidCloseHandlerTests
{
    [Fact]
    public void ShouldRemoveClosedDocumentFromState()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-didclose").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "source.norg");
            string targetPath = Path.Combine(tempDir, "target.norg");
            File.WriteAllText(sourcePath, "See {:target:}[Target]");
            File.WriteAllText(targetPath, "@document.meta\ntitle: Target\n@end\n");
            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            Uri targetUri = new(Path.GetFullPath(targetPath));

            LanguageServerState state = new();
            state.UpdateDocument(sourceUri);
            state.UpdateDocument(targetUri);
            BufferingWriter writer = new();

            RpcMessage request = new()
            {
                JsonRpc = "2.0",
                Method = HandlerFactory.MethodType.DidClose,
                Params = JsonSerializer.SerializeToElement(
                    new { textDocument = new { uri = sourceUri.AbsoluteUri } }
                )
            };

            DidCloseHandler handler = new(state, writer, request);
            _ = handler.HandleRequest();

            state.Documents.Should().NotContainKey(sourceUri);
            state.References.GetValueOrDefault(targetUri, []).Should().BeEmpty();
            writer.WriteCount.Should().BeGreaterThan(0);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private sealed class BufferingWriter : IRpcWriter
    {
        public int WriteCount { get; private set; }

        public void EncodeAndWrite(object o)
        {
            WriteCount++;
        }
    }
}
