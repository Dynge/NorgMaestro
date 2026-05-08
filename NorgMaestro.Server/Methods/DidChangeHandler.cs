using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class DidChangeHandler(LanguageServerState state, IRpcWriter writer, RpcMessage request)
    : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;
    private readonly IRpcWriter _writer = writer;

    public Response? HandleRequest()
    {
        DidChangeNotification didChangeNotification = DidChangeNotification.From(_request);
        Uri uri = didChangeNotification.Params.TextDocument.Uri;
        string? nextText = didChangeNotification.Params.ContentChanges.LastOrDefault()?.Text;
        if (nextText is null)
        {
            return null;
        }

        string[] content = NormalizeLines(nextText);
        _ = _state.UpdateDocument(uri, content);
        PublishDiagnostics();
        return null;
    }

    private static string[] NormalizeLines(string text)
    {
        return text.Replace("\r", string.Empty).Split('\n');
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
