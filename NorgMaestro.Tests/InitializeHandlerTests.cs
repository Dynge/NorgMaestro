using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class InitializeHandlerTests
{
    [Fact]
    public async Task ShouldUseRootPathWhenRootUriAndWorkspaceFoldersMissing()
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
            Response? response = await handler.HandleRequest();

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
    public async Task ShouldFallbackToAppBaseDirectoryWhenRootFieldsMissing()
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
        Response? response = await handler.HandleRequest();

        response.Should().NotBeNull();
        state.WorkspaceRoot.Should().NotBeNull();
        state.WorkspaceRoot!.LocalPath.Should().Be(Path.GetFullPath(AppContext.BaseDirectory));
    }

    [Fact]
    public void ShouldApplyInitializationOptionsForUnresolvedLinkSeverity()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-initialize-severity").FullName;
        try
        {
            LanguageServerState state = new();
            RpcMessage request = new()
            {
                JsonRpc = "2.0",
                Id = 3,
                Method = HandlerFactory.MethodType.Initialize,
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        capabilities = new { },
                        rootPath = tempDir,
                        initializationOptions = new
                        {
                            diagnostics = new
                            {
                                unresolvedLinkSeverity = "error"
                            }
                        }
                    }
                )
            };

            InitializeHandler handler = new(state, new BufferingWriter(), request);
            _ = handler.HandleRequest();

            state.UnresolvedLinkSeverity.Should().Be(DiagnosticSeverity.Error);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private sealed class BufferingWriter : IRpcWriter
    {
        public Task EncodeAndWrite(object o) => Task.CompletedTask;
    }
}
