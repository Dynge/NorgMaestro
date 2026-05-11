using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class DidCloseHandler(LanguageServerState state, IRpcWriter writer, RpcMessage request)
    : IMessageHandler
{
    private readonly DiagnosticsPublisher _diagnosticsPublisher = new(state, writer);
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public async Task<Response?> HandleRequest()
    {
        DidCloseNotification didCloseNotification = DidCloseNotification.From(_request);
        _state.RemoveDocument(didCloseNotification.Params.TextDocument.Uri);
        await _diagnosticsPublisher.PublishAsync();
        return null;
    }
}
