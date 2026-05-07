using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class DidSaveHandler(LanguageServerState state, IRpcWriter writer, RpcMessage request)
    : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;
    private readonly IRpcWriter _writer = writer;

    public async Task<Response?> HandleRequest()
    {
        DidSaveNotification didSaveNotification = DidSaveNotification.From(_request);
        _ = await _state.UpdateDocument(didSaveNotification.Params.TextDocument.Uri);
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
