using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class DidOpenHandlerTests
{
    [Fact]
    public void ShouldIndexOpenedDocumentAndPublishDiagnostics()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-didopen").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "source.norg");
            File.WriteAllText(sourcePath, "@document.meta\ntitle: Source\n@end\n\nNo links here");

            LanguageServerState state = new();
            BufferingWriter writer = new();
            RpcMessage request = new()
            {
                JsonRpc = "2.0",
                Method = HandlerFactory.MethodType.DidOpen,
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new
                        {
                            uri = new Uri(Path.GetFullPath(sourcePath)).AbsoluteUri,
                            languageId = "norg",
                            version = 1,
                            text = File.ReadAllText(sourcePath)
                        }
                    }
                )
            };

            DidOpenHandler handler = new(state, writer, request);
            _ = handler.HandleRequest();

            state.Documents.Should().ContainKey(new Uri(Path.GetFullPath(sourcePath)));
            writer.WriteCount.Should().BeGreaterThan(0);
            state.GetDiagnostics().Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ShouldIndexDocumentFromDidOpenTextWhenFileMissing()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-didopen-inline").FullName;
        try
        {
            string missingPath = Path.Combine(tempDir, "untitled.norg");

            LanguageServerState state = new();
            BufferingWriter writer = new();
            RpcMessage request = new()
            {
                JsonRpc = "2.0",
                Method = HandlerFactory.MethodType.DidOpen,
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new
                        {
                            uri = new Uri(Path.GetFullPath(missingPath)).AbsoluteUri,
                            languageId = "norg",
                            version = 1,
                            text = "@document.meta\ntitle: Inline\n@end\n"
                        }
                    }
                )
            };

            DidOpenHandler handler = new(state, writer, request);
            _ = handler.HandleRequest();

            Uri uri = new(Path.GetFullPath(missingPath));
            state.Documents.Should().ContainKey(uri);
            state.Documents[uri].Metadata.Title!.Name.Should().Be("Inline");
            writer.WriteCount.Should().BeGreaterThan(0);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ShouldPreferDidOpenTextOverFileContent()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-didopen-prefer").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "source.norg");
            File.WriteAllText(sourcePath, "@document.meta\ntitle: Disk\n@end\n");

            LanguageServerState state = new();
            BufferingWriter writer = new();
            RpcMessage request = new()
            {
                JsonRpc = "2.0",
                Method = HandlerFactory.MethodType.DidOpen,
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new
                        {
                            uri = new Uri(Path.GetFullPath(sourcePath)).AbsoluteUri,
                            languageId = "norg",
                            version = 1,
                            text = "@document.meta\ntitle: Buffer\n@end\n"
                        }
                    }
                )
            };

            DidOpenHandler handler = new(state, writer, request);
            _ = handler.HandleRequest();

            Uri uri = new(Path.GetFullPath(sourcePath));
            state.Documents[uri].Metadata.Title!.Name.Should().Be("Buffer");
            writer.WriteCount.Should().BeGreaterThan(0);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ShouldFallbackToDiskWhenDidOpenTextMissing()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-didopen-fallback").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "source.norg");
            File.WriteAllText(sourcePath, "@document.meta\ntitle: DiskOnly\n@end\n");

            LanguageServerState state = new();
            BufferingWriter writer = new();
            RpcMessage request = new()
            {
                JsonRpc = "2.0",
                Method = HandlerFactory.MethodType.DidOpen,
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        textDocument = new
                        {
                            uri = new Uri(Path.GetFullPath(sourcePath)).AbsoluteUri,
                            languageId = "norg",
                            version = 1,
                        }
                    }
                )
            };

            DidOpenHandler handler = new(state, writer, request);
            _ = handler.HandleRequest();

            Uri uri = new(Path.GetFullPath(sourcePath));
            state.Documents[uri].Metadata.Title!.Name.Should().Be("DiskOnly");
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

        public Task EncodeAndWrite(object o)
        {
            WriteCount++;
            return Task.CompletedTask;
        }
    }
}
