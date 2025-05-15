using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class DidSaveHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public async Task<Response?> HandleRequest()
    {
        DidSaveNotification didSaveNotification = DidSaveNotification.From(_request);
        _ = await _state.UpdateDocument(didSaveNotification.Params.TextDocument.Uri);
        return null;
    }
}
