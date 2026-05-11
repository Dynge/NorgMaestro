using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class DocumentLinkHandlerTests
{
    [Fact]
    public async Task ShouldReturnDocumentLinksForNorgFileLinks()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-links").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601040101.norg");
            File.WriteAllText(
                sourcePath,
                "@document.meta\ntitle: Link Note\n@end\n\n- {:202601040102:}[Second]\n- {:202601040103:}[Third]"
            );

            Uri sourceUri = new(Path.GetFullPath(sourcePath));
            LanguageServerState state = new();
            _ = await state.UpdateDocument(sourceUri);

            RpcMessage request = new()
            {
                Id = 11,
                JsonRpc = "2.0",
                Method = "textDocument/documentLink",
                Params = JsonSerializer.SerializeToElement(new { textDocument = new { uri = sourceUri.AbsoluteUri } })
            };

            DocumentLinkHandler handler = new(state, request);
            Response? response = await handler.HandleRequest();

            JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
            DocumentLink[]? links = result.Deserialize<DocumentLink[]>();
            links.Should().NotBeNull();
            DocumentLink[] linkList = links ?? throw new Xunit.Sdk.XunitException("Missing links payload");
            linkList.Should().HaveCount(2);
            DocumentLink second = linkList[0] ?? throw new Xunit.Sdk.XunitException("Missing first link");
            DocumentLink third = linkList[1] ?? throw new Xunit.Sdk.XunitException("Missing second link");
            second.Tooltip.Should().Be("Second");
            third.Tooltip.Should().Be("Third");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
