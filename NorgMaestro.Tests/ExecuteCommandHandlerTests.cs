using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class ExecuteCommandHandlerTests
{
    [Fact]
    public void ShouldCreateMissingNoteFromExecuteCommand()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-execute-command").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601080100.norg");
            string targetPath = Path.Combine(tempDir, "202601080101.norg");
            File.WriteAllText(sourcePath, "See {:202601080101:}[Missing]");

            LanguageServerState state = new();
            state.UpdateDocument(new Uri(Path.GetFullPath(sourcePath)));

            RpcMessage request = new()
            {
                Id = 94,
                JsonRpc = "2.0",
                Method = "workspace/executeCommand",
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        command = CodeActionHandler.CreateNoteCommand,
                        arguments = new[] { targetPath }
                    }
                )
            };

            BufferingWriter writer = new();
            ExecuteCommandHandler handler = new(state, writer, request);
            _ = handler.HandleRequest();

            File.Exists(targetPath).Should().BeTrue();
            string content = File.ReadAllText(targetPath);
            content.Should().Contain("@document.meta");
            writer.WriteCount.Should().BeGreaterThan(0);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ShouldCreateNoteFromLinkTextAndRewriteLink()
    {
        string tempDir = Directory.CreateTempSubdirectory("norgmaestro-linktext").FullName;
        try
        {
            string sourcePath = Path.Combine(tempDir, "202601080200.norg");
            File.WriteAllText(sourcePath, "{}[Title of A new note]");

            LanguageServerState state = new();
            state.Initialize(new Uri(Path.GetFullPath(tempDir)));
            state.UpdateDocument(new Uri(Path.GetFullPath(sourcePath)));

            RpcMessage request = new()
            {
                Id = 95,
                JsonRpc = "2.0",
                Method = "workspace/executeCommand",
                Params = JsonSerializer.SerializeToElement(
                    new
                    {
                        command = CodeActionHandler.CreateNoteFromLinkTextCommand,
                        arguments = new object[]
                        {
                            sourcePath,
                            0,
                            0,
                            22,
                            "Title of A new note"
                        }
                    }
                )
            };

            BufferingWriter writer = new();
            ExecuteCommandHandler handler = new(state, writer, request);
            _ = handler.HandleRequest();

            string rewritten = File.ReadAllText(sourcePath);
            rewritten.Should().Contain("{:20");
            rewritten.Should().Contain("[Title of A new note]");
            Directory.GetFiles(tempDir, "*.norg").Length.Should().BeGreaterThan(1);
            writer.WriteCount.Should().BeGreaterThan(0);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ShouldReturnErrorForUnknownExecuteCommand()
    {
        LanguageServerState state = new();
        BufferingWriter writer = new();
        RpcMessage request = new()
        {
            Id = 96,
            JsonRpc = "2.0",
            Method = "workspace/executeCommand",
            Params = JsonSerializer.SerializeToElement(
                new
                {
                    command = "norgmaestro.command.unknown",
                    arguments = Array.Empty<object>()
                }
            )
        };

        ExecuteCommandHandler handler = new(state, writer, request);
        Response? response = handler.HandleRequest().Result;

        response.Should().NotBeNull();
        response!.Error.Should().NotBeNull();
        response.Result.Should().BeNull();
        JsonElement error = response.Error ?? throw new Xunit.Sdk.XunitException("Missing error payload");
        error.GetProperty("code").GetInt32().Should().Be(-32601);
        error.GetProperty("message").GetString().Should().Contain("Unknown command");
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
