using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class DidChangeHandlerTests
{
    [Fact]
    public void ShouldUpdateStateFromInMemoryTextWithoutSave()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-didchange").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "source.norg");
            File.WriteAllText(sourcePath, "@document.meta\ntitle: Old\n@end\n");
            Uri sourceUri = new(Path.GetFullPath(sourcePath));

            LanguageServerState state = new();
            state.UpdateDocument(sourceUri);
            BufferingWriter writer = new();
            RpcMessage request = new()
            {
                JsonRpc = "2.0",
                Method = HandlerFactory.MethodType.DidChange,
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new { uri = sourceUri.AbsoluteUri, version = 2 },
                        contentChanges = new[]
                        {
                            new { text = "@document.meta\ntitle: New\n@end\n\nSee {:missing:}[Missing]" }
                        }
                    }
                )
            };

            DidChangeHandler handler = new(state, writer, request);
            _ = handler.HandleRequest();

            state.Documents[sourceUri].Metadata.Title!.Name.Should().Be("New");
            state.GetDiagnostics().Should().ContainKey(sourceUri);
            writer.WriteCount.Should().BeGreaterThan(0);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ShouldApplyRangeBasedIncrementalChanges()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-didchange-range").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "source.norg");
            File.WriteAllText(sourcePath, "@document.meta\ntitle: Old\n@end\n");
            Uri sourceUri = new(Path.GetFullPath(sourcePath));

            LanguageServerState state = new();
            state.UpdateDocument(sourceUri);
            BufferingWriter writer = new();
            RpcMessage request = new()
            {
                JsonRpc = "2.0",
                Method = HandlerFactory.MethodType.DidChange,
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new { uri = sourceUri.AbsoluteUri, version = 2 },
                        contentChanges = new object[]
                        {
                            new
                            {
                                range = new
                                {
                                    start = new { line = 1, character = 7 },
                                    end = new { line = 1, character = 10 }
                                },
                                rangeLength = 3,
                                text = "New"
                            }
                        }
                    }
                )
            };

            DidChangeHandler handler = new(state, writer, request);
            _ = handler.HandleRequest();

            state.Documents[sourceUri].Metadata.Title!.Name.Should().Be("New");
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
