using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class DidOpenHandler(LanguageServerState state, IRpcWriter writer, RpcMessage request)
    : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly DiagnosticsPublisher _diagnosticsPublisher = new(state, writer);
    private readonly LanguageServerState _state = state;

    public async Task<Response?> HandleRequest()
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
        await _diagnosticsPublisher.PublishAsync();
        return null;
    }
}
