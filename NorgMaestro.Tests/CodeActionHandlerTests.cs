using System.Text.Json;
using FluentAssertions;
using NorgMaestro.Server;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class CodeActionHandlerTests
{
    [Fact]
    public async Task ShouldCreateQuickFixForUnresolvedNoteDiagnostics()
    {
        RpcMessage request = new()
        {
            Id = 93,
            JsonRpc = "2.0",
            Method = "textDocument/codeAction",
            Params = JsonSerializer.SerializeToElement(
                new
                {
                    textDocument = new { uri = "file:///tmp/a.norg" },
                    range = new
                    {
                        start = new { line = 5, character = 1 },
                        end = new { line = 5, character = 10 }
                    },
                    context = new
                    {
                        diagnostics = new[]
                        {
                            new
                            {
                                range = new
                                {
                                    start = new { line = 5, character = 1 },
                                    end = new { line = 5, character = 10 }
                                },
                                severity = 2,
                                source = "norgmaestro",
                                message = "Unresolved note link: /tmp/missing-note.norg"
                            }
                        }
                    }
                }
            )
        };

        CodeActionHandler handler = new(new LanguageServerState(), request);
        Response? response = await handler.HandleRequest();
        JsonElement result = response!.Result ?? throw new Xunit.Sdk.XunitException("Missing result payload");
        CodeAction[]? actions = result.Deserialize<CodeAction[]>();

        actions.Should().NotBeNull();
        CodeAction[] actionList = actions ?? throw new Xunit.Sdk.XunitException("Missing actions payload");
        actionList.Should().NotBeEmpty();
        CodeAction action = actionList
            .FirstOrDefault(a => a.Command?.Command == CodeActionHandler.CreateNoteCommand)
            ?? throw new Xunit.Sdk.XunitException("Missing create-note command action");
        action.Command.Should().NotBeNull();
        action.Command!.Command.Should().Be(CodeActionHandler.CreateNoteCommand);
    }
}
