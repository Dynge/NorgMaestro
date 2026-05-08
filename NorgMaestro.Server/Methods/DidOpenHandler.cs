using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class DidOpenHandler(LanguageServerState state, IRpcWriter writer, RpcMessage request)
    : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;
    private readonly IRpcWriter _writer = writer;

    public Response? HandleRequest()
    {
        DidOpenNotification didOpenNotification = DidOpenNotification.From(_request);
        Uri uri = didOpenNotification.Params.TextDocument.Uri;
        string? text = didOpenNotification.Params.TextDocument.Text;
        if (text is not null)
        {
            string normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
            string[] content = normalized.Split('\n');
            _ = _state.UpdateDocument(uri, content);
        }
        else if (File.Exists(uri.LocalPath))
        {
            _ = _state.UpdateDocument(uri);
        }
        PublishDiagnostics();
        return null;
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
