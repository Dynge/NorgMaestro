using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

internal sealed class DiagnosticsPublisher(LanguageServerState state, IRpcWriter writer)
{
    private readonly LanguageServerState _state = state;
    private readonly IRpcWriter _writer = writer;

    public async Task PublishAsync()
    {
        Dictionary<Uri, Diagnostic[]> diagnosticsByFile = _state.GetDiagnostics();
        foreach (Document document in _state.Documents.Values)
        {
            Diagnostic[] diagnostics = diagnosticsByFile.GetValueOrDefault(document.Uri, []);
            await _writer.EncodeAndWrite(
                Notification.PublishDiagnostics(
                    new() { Uri = document.Uri.AbsoluteUri, Diagnostics = diagnostics }
                )
            );
        }
    }
}
