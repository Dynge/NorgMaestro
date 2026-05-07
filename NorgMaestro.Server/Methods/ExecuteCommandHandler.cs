using NorgMaestro.Server.Rpc;
using System.Text.Json;

namespace NorgMaestro.Server.Methods;

public class ExecuteCommandHandler(LanguageServerState state, IRpcWriter writer, RpcMessage request)
    : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;
    private readonly IRpcWriter _writer = writer;

    public Response? HandleRequest()
    {
        ExecuteCommandRequest executeRequest = ExecuteCommandRequest.From(_request);
        switch (executeRequest.Params.Command)
        {
            case CodeActionHandler.CreateNoteCommand:
                CreateMissingNote(executeRequest.Params.Arguments);
                PublishDiagnostics();
                break;
        }

        return executeRequest.Id is int id ? Response.OfSuccess(id) : null;
    }

    private void CreateMissingNote(JsonElement[]? arguments)
    {
        string? targetPath = arguments?.FirstOrDefault().GetString();
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            return;
        }

        if (File.Exists(targetPath))
        {
            return;
        }

        string? directory = Path.GetDirectoryName(targetPath);
        if (string.IsNullOrWhiteSpace(directory) is false)
        {
            Directory.CreateDirectory(directory);
        }

        string noteId = Path.GetFileNameWithoutExtension(targetPath);
        File.WriteAllText(targetPath, $"@document.meta\ntitle: {noteId}\n@end\n");
        _state.UpdateDocument(new Uri(Path.GetFullPath(targetPath)));
    }

    private void PublishDiagnostics()
    {
        Dictionary<Uri, Diagnostic[]> diagnosticsByFile = _state.GetDiagnostics();
        foreach (Document document in _state.Documents.Values)
        {
            Diagnostic[] diagnostics = diagnosticsByFile.GetValueOrDefault(document.Uri, []);
            _writer.EncodeAndWrite(
                Notification.PublishDiagnostics(
                    new() { Uri = document.Uri.AbsoluteUri, Diagnostics = diagnostics }
                )
            );
        }
    }
}
