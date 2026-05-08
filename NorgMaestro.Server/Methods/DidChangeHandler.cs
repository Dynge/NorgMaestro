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
        if (_state.Documents.TryGetValue(uri, out Document? document) is false)
        {
            return null;
        }

        string contentText = string.Join('\n', document.Content);
        foreach (TextDocumentContentChangeEvent change in didChangeNotification.Params.ContentChanges)
        {
            contentText = ApplyChange(contentText, change);
        }

        string[] content = NormalizeLines(contentText);
        _ = _state.UpdateDocument(uri, content);
        PublishDiagnostics();
        return null;
    }

    private static string ApplyChange(string source, TextDocumentContentChangeEvent change)
    {
        if (change.Range is null)
        {
            return change.Text;
        }

        int startOffset = GetOffset(source, change.Range.Start);
        int endOffset = GetOffset(source, change.Range.End);
        return source[..startOffset] + change.Text + source[endOffset..];
    }

    private static int GetOffset(string source, Position position)
    {
        int offset = 0;
        uint line = 0;
        while (line < position.Line && offset < source.Length)
        {
            if (source[offset] == '\n')
            {
                line++;
            }
            offset++;
        }

        return Math.Min(offset + (int)position.Character, source.Length);
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
