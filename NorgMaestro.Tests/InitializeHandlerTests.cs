using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class InitializeHandlerTests
{
    [Fact]
    public void ShouldUseRootPathWhenRootUriAndWorkspaceFoldersMissing()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-initialize-rootpath").FullName;
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "index.norg"), "@document.meta\ntitle: RootPath\n@end\n");
            LanguageServerState state = new();
            RpcMessage request = new()
            {
                JsonRpc = "2.0",
                Id = 1,
                Method = HandlerFactory.MethodType.Initialize,
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        capabilities = new { },
                        rootPath = tempDir,
                    }
                )
            };

            InitializeHandler handler = new(state, new BufferingWriter(), request);
            Response? response = handler.HandleRequest();

            response.Should().NotBeNull();
            state.WorkspaceRoot.Should().NotBeNull();
            state.WorkspaceRoot!.LocalPath.Should().Be(Path.GetFullPath(tempDir));
            state.Documents.Should().ContainKey(new Uri(Path.GetFullPath(Path.Combine(tempDir, "index.norg"))));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ShouldFallbackToAppBaseDirectoryWhenRootFieldsMissing()
    {
        LanguageServerState state = new();
        RpcMessage request = new()
        {
            JsonRpc = "2.0",
            Id = 2,
            Method = HandlerFactory.MethodType.Initialize,
            Params = JsonSerializer.SerializeToElement(
                new
                {
                    capabilities = new { },
                }
            )
        };

        InitializeHandler handler = new(state, new BufferingWriter(), request);
        Response? response = handler.HandleRequest();

        response.Should().NotBeNull();
        state.WorkspaceRoot.Should().NotBeNull();
        state.WorkspaceRoot!.LocalPath.Should().Be(Path.GetFullPath(AppContext.BaseDirectory));
    }

    private sealed class BufferingWriter : IRpcWriter
    {
        public void EncodeAndWrite(object o) { }
    }
}
