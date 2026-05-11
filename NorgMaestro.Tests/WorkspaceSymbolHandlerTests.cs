using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class WorkspaceSymbolHandlerTests
{
    [Fact]
    public async Task ShouldFilterByQueryCaseInsensitive()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-ws-symbol").FullName;
        try
        {
            string alphaPath = Path.Combine(tempDir, "alpha.norg");
            string betaPath = Path.Combine(tempDir, "beta.norg");
            File.WriteAllText(alphaPath, "@document.meta\ntitle: Alpha Note\n@end\n");
            File.WriteAllText(betaPath, "@document.meta\ntitle: Beta Note\n@end\n");

            LanguageServerState state = new();
            _ = await state.UpdateDocument(new Uri(Path.GetFullPath(alphaPath)));
            _ = await state.UpdateDocument(new Uri(Path.GetFullPath(betaPath)));

            WorkspaceSymbolHandler handler = new(state, CreateRequest("alpha", 31));
            Response? response = await handler.HandleRequest();
            JsonElement[] items = response!.Result!.Value.Deserialize<JsonElement[]>()!;

            items.Should().HaveCount(1);
            items[0].GetProperty("name").GetString().Should().Contain("Alpha Note");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ShouldSortWorkspaceSymbolsByName()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-ws-order").FullName;
        try
        {
            string zetaPath = Path.Combine(tempDir, "zeta.norg");
            string alphaPath = Path.Combine(tempDir, "alpha.norg");
            File.WriteAllText(zetaPath, "@document.meta\ntitle: Zeta\n@end\n");
            File.WriteAllText(alphaPath, "@document.meta\ntitle: Alpha\n@end\n");

            LanguageServerState state = new();
            _ = await state.UpdateDocument(new Uri(Path.GetFullPath(zetaPath)));
            _ = await state.UpdateDocument(new Uri(Path.GetFullPath(alphaPath)));

            WorkspaceSymbolHandler handler = new(state, CreateRequest("", 32));
            Response? response = await handler.HandleRequest();
            JsonElement[] items = response!.Result!.Value.Deserialize<JsonElement[]>()!;

            items.Should().HaveCount(2);
            items[0].GetProperty("name").GetString().Should().StartWith("Alpha");
            items[1].GetProperty("name").GetString().Should().StartWith("Zeta");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static RpcMessage CreateRequest(string query, int id)
    {
        return new()
        {
            Id = id,
            JsonRpc = "2.0",
            Method = HandlerFactory.MethodType.WorkspaceSymbols,
            Params = JsonSerializer.SerializeToElement(new { query }),
        };
    }
}
