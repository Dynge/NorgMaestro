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
            symbolList.Should().HaveCount(1);
            DocumentSymbol root = symbolList[0] ?? throw new Xunit.Sdk.XunitException("Missing root symbol");
            root.Name.Should().Be("Root");
            root.Children.Should().NotBeNull();
            root.Children.Should().HaveCount(1);
            root.Children[0].Name.Should().Be("Child");
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

    [Fact]
    public void ShouldBuildNestedHierarchyByHeadingDepth()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-symbols-tree").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601020103.norg");
            File.WriteAllText(sourcePath, "* Root\n** Child A\n*** Leaf\n** Child B\n* Root 2\n");

            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            LanguageServerState state = new();
            state.UpdateDocument(sourceUri);

            RpcMessage request = new()
            {
                Id = 9,
                JsonRpc = "2.0",
                Method = "textDocument/documentSymbol",
                Params = JsonSerializer.SerializeToElement(new { textDocument = new { uri = sourceUri.AbsoluteUri } })
            };

            DocumentSymbolHandler handler = new(state, request);
            Response? response = handler.HandleRequest();

            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            DocumentSymbol[] symbols = result.Deserialize<DocumentSymbol[]>() ?? [];

            symbols.Should().HaveCount(2);
            symbols[0].Name.Should().Be("Root");
            symbols[0].Children.Should().HaveCount(2);
            symbols[0].Children[0].Name.Should().Be("Child A");
            symbols[0].Children[0].Children.Should().HaveCount(1);
            symbols[0].Children[0].Children[0].Name.Should().Be("Leaf");
            symbols[0].Children[1].Name.Should().Be("Child B");
            symbols[1].Name.Should().Be("Root 2");
            symbols[1].Children.Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
