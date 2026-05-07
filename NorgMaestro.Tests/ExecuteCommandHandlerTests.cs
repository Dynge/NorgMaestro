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

    private sealed class BufferingWriter : IRpcWriter
    {
        public int WriteCount { get; private set; }

        public void EncodeAndWrite(object o)
        {
            WriteCount++;
        }
    }
}
