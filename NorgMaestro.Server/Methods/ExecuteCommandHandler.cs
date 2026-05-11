using NorgMaestro.Server.Rpc;
using System.Text.Json;

namespace NorgMaestro.Server.Methods;

public class ExecuteCommandHandler(LanguageServerState state, IRpcWriter writer, RpcMessage request)
    : IMessageHandler
{
    private readonly DiagnosticsPublisher _diagnosticsPublisher = new(state, writer);
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;
    private readonly IRpcWriter _writer = writer;

    public async Task<Response?> HandleRequest()
    {
        ExecuteCommandRequest executeRequest = ExecuteCommandRequest.From(_request);
        Func<JsonElement[]?, Task>? handleCommand = ResolveCommandHandler(executeRequest.Params.Command);
        if (handleCommand is null)
        {
            return
                executeRequest.Id is int id
                    ? Response.OfError(
                        id,
                        new
                        {
                            code = -32601,
                            message = $"Unknown command: {executeRequest.Params.Command}"
                        }
                    )
                    : null;
        }

        await handleCommand(executeRequest.Params.Arguments);
        await _diagnosticsPublisher.PublishAsync();

        return executeRequest.Id is int requestId ? Response.OfSuccess(requestId) : null;
    }

    private Func<JsonElement[]?, Task>? ResolveCommandHandler(string command)
    {
        return command switch
        {
            CodeActionHandler.CreateNoteCommand => CreateMissingNoteCommand,
            CodeActionHandler.CreateNoteAndOpenCommand => CreateMissingNoteAndOpenCommand,
            CodeActionHandler.CreateBacklinkSectionCommand => CreateBacklinkSection,
            CodeActionHandler.ExtractSelectionToNoteCommand => ExtractSelectionToNewNote,
            CodeActionHandler.MoveNoteToWorkspaceCommand => MoveNoteToWorkspace,
            CodeActionHandler.CreateNoteFromLinkTextCommand => CreateNoteFromLinkText,
            _ => null,
        };
    }

    private async Task CreateMissingNoteCommand(JsonElement[]? arguments)
    {
        _ = await CreateMissingNote(arguments);
    }

    private async Task CreateMissingNoteAndOpenCommand(JsonElement[]? arguments)
    {
        string? created = await CreateMissingNote(arguments);
        if (string.IsNullOrWhiteSpace(created) is false)
        {
            await ShowDocument(created!);
        }
    }

    private async Task<string?> CreateMissingNote(JsonElement[]? arguments)
    {
        string? targetPath = arguments?.FirstOrDefault().GetString();
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            return null;
        }

        if (File.Exists(targetPath))
        {
            return targetPath;
        }

        string? directory = Path.GetDirectoryName(targetPath);
        if (string.IsNullOrWhiteSpace(directory) is false)
        {
            Directory.CreateDirectory(directory);
        }

        string noteId = Path.GetFileNameWithoutExtension(targetPath);
        File.WriteAllText(targetPath, $"@document.meta\ntitle: {noteId}\n@end\n");
        _ = await _state.UpdateDocument(new Uri(Path.GetFullPath(targetPath)));
        return targetPath;
    }

    private async Task ShowDocument(string path)
    {
        await _writer.EncodeAndWrite(
            new RpcNotification<ShowDocumentParams>
            {
                JsonRpc = "2.0",
                Method = "window/showDocument",
                Params = new() { Uri = new Uri(Path.GetFullPath(path)).AbsoluteUri, TakeFocus = true },
            }
        );
    }

    private async Task CreateBacklinkSection(JsonElement[]? arguments)
    {
        string? sourcePath = arguments?.ElementAtOrDefault(0).GetString();
        string? targetPath = arguments?.ElementAtOrDefault(1).GetString();
        if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(targetPath))
        {
            return;
        }

        if (File.Exists(targetPath) is false || File.Exists(sourcePath) is false)
        {
            return;
        }

        string sourceId = Path.GetFileNameWithoutExtension(sourcePath);
        string sourceTitle = _state.Documents.TryGetValue(new Uri(Path.GetFullPath(sourcePath)), out Document? source)
            ? source.Metadata.Title?.Name ?? sourceId
            : sourceId;

        string backlink = $"\n* Backlinks\n- {{:{sourceId}:}}[{sourceTitle}]\n";
        string content = File.ReadAllText(targetPath);
        if (content.Contains($"{{:{sourceId}:}}[", StringComparison.Ordinal))
        {
            return;
        }

        File.AppendAllText(targetPath, backlink);
        _ = await _state.UpdateDocument(new Uri(Path.GetFullPath(targetPath)));
    }

    private async Task ExtractSelectionToNewNote(JsonElement[]? arguments)
    {
        string? sourcePath = arguments?.ElementAtOrDefault(0).GetString();
        uint startLine = arguments?.ElementAtOrDefault(1).GetUInt32() ?? 0;
        uint startChar = arguments?.ElementAtOrDefault(2).GetUInt32() ?? 0;
        uint endChar = arguments?.ElementAtOrDefault(3).GetUInt32() ?? 0;
        if (string.IsNullOrWhiteSpace(sourcePath) || File.Exists(sourcePath) is false)
        {
            return;
        }

        string[] lines = File.ReadAllLines(sourcePath);
        if (startLine >= lines.Length)
        {
            return;
        }

        string line = lines[startLine];
        if (endChar <= startChar || endChar > line.Length)
        {
            return;
        }

        string selected = line[(int)startChar..(int)endChar];
        string noteId = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        string targetPath = Path.Combine(Path.GetDirectoryName(sourcePath)!, noteId + ".norg");
        File.WriteAllText(targetPath, $"@document.meta\ntitle: {selected}\n@end\n\n{selected}\n");
        lines[startLine] = line[..(int)startChar] + $"{{:{noteId}:}}[{selected}]" + line[(int)endChar..];
        File.WriteAllLines(sourcePath, lines);

        _ = await _state.UpdateDocument(new Uri(Path.GetFullPath(sourcePath)));
        _ = await _state.UpdateDocument(new Uri(Path.GetFullPath(targetPath)));
    }

    private async Task MoveNoteToWorkspace(JsonElement[]? arguments)
    {
        string? sourcePath = arguments?.ElementAtOrDefault(0).GetString();
        string? workspaceName = arguments?.ElementAtOrDefault(1).GetString();
        if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(workspaceName))
        {
            return;
        }

        if (_state.Workspaces.TryGetValue(workspaceName, out Uri? workspaceUri) is false)
        {
            return;
        }

        if (File.Exists(sourcePath) is false)
        {
            return;
        }

        string fileName = Path.GetFileName(sourcePath);
        string destinationPath = Path.Combine(workspaceUri.LocalPath, fileName);
        Directory.CreateDirectory(workspaceUri.LocalPath);
        File.Move(sourcePath, destinationPath, true);
        _ = await _state.UpdateDocument(new Uri(Path.GetFullPath(destinationPath)));
    }

    private async Task CreateNoteFromLinkText(JsonElement[]? arguments)
    {
        string? sourcePath = arguments?.ElementAtOrDefault(0).GetString();
        uint lineNr = arguments?.ElementAtOrDefault(1).GetUInt32() ?? 0;
        uint start = arguments?.ElementAtOrDefault(2).GetUInt32() ?? 0;
        uint end = arguments?.ElementAtOrDefault(3).GetUInt32() ?? 0;
        string title = arguments?.ElementAtOrDefault(4).GetString() ?? "Untitled";
        if (string.IsNullOrWhiteSpace(sourcePath) || File.Exists(sourcePath) is false)
        {
            return;
        }

        Uri rootUri = _state.WorkspaceRoot
            ?? _state.Workspaces.Values.FirstOrDefault()
            ?? new Uri(Path.GetDirectoryName(sourcePath)!);
        string noteId = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        string targetPath = Path.Combine(rootUri.LocalPath, noteId + ".norg");
        Directory.CreateDirectory(rootUri.LocalPath);
        File.WriteAllText(targetPath, $"@document.meta\ntitle: {title}\n@end\n");

        string[] lines = File.ReadAllLines(sourcePath);
        if (lineNr < lines.Length)
        {
            string line = lines[lineNr];
            if (start <= line.Length && end <= line.Length && end > start)
            {
                lines[lineNr] = line[..(int)start] + $"{{:{noteId}:}}[{title}]" + line[(int)end..];
                File.WriteAllLines(sourcePath, lines);
            }
        }

        _ = await _state.UpdateDocument(new Uri(Path.GetFullPath(sourcePath)));
        _ = await _state.UpdateDocument(new Uri(Path.GetFullPath(targetPath)));
        await ShowDocument(targetPath);
    }
}
