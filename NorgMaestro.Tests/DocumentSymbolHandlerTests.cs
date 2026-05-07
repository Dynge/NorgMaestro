using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class DocumentSymbolHandlerTests
{
    [Fact]
    public void ShouldReturnHeadingsAsDocumentSymbols()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-symbols").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601020101.norg");
            File.WriteAllText(
                sourcePath,
                "@document.meta\ntitle: Symbols Note\n@end\n\n* Root\n** Child\nText"
            );

            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            LanguageServerState state = new();
            state.UpdateDocument(sourceUri);

            RpcMessage request = new()
            {
                Id = 7,
                JsonRpc = "2.0",
                Method = "textDocument/documentSymbol",
                Params = JsonSerializer.SerializeToElement(new { textDocument = new { uri = sourceUri.AbsoluteUri } })
            };

            DocumentSymbolHandler handler = new(state, request);
            Response? response = handler.HandleRequest();

            response.Should().NotBeNull();
            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            DocumentSymbol[]? symbols = result.Deserialize<DocumentSymbol[]>();

            symbols.Should().NotBeNull();
            symbols!.Should().HaveCount(2);
            symbols[0]!.Name.Should().Be("Root");
            symbols[1]!.Name.Should().Be("Child");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
