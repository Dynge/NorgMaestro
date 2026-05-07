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
            DocumentSymbol[] symbolList = symbols ?? throw new Xunit.Sdk.XunitException("Missing symbols payload");
            symbolList.Should().HaveCount(2);
            DocumentSymbol root = symbolList[0] ?? throw new Xunit.Sdk.XunitException("Missing root symbol");
            DocumentSymbol child = symbolList[1] ?? throw new Xunit.Sdk.XunitException("Missing child symbol");
            root.Name.Should().Be("Root");
            child.Name.Should().Be("Child");
            root.Children.Should().NotBeNull();
            root.Children.Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ShouldReturnEmptyArrayForDocumentWithoutHeadings()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-symbols-empty").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601020102.norg");
            File.WriteAllText(sourcePath, "@document.meta\ntitle: Symbols Note\n@end\n\nno headings here");

            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            LanguageServerState state = new();
            state.UpdateDocument(sourceUri);

            RpcMessage request = new()
            {
                Id = 8,
                JsonRpc = "2.0",
                Method = "textDocument/documentSymbol",
                Params = JsonSerializer.SerializeToElement(new { textDocument = new { uri = sourceUri.AbsoluteUri } })
            };

            DocumentSymbolHandler handler = new(state, request);
            Response? response = handler.HandleRequest();

            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            DocumentSymbol[]? symbols = result.Deserialize<DocumentSymbol[]>();

            symbols.Should().NotBeNull();
            symbols.Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
