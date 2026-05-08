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
        if (File.Exists(uri.LocalPath))
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
