using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class DidSaveHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Response? HandleRequest()
    {
        DidSaveNotification didSaveNotification = DidSaveNotification.From(_request);
        _ = _state.UpdateDocument(didSaveNotification.Params.TextDocument.Uri);
        return null;
    }
}
