using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class DidSaveHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage Request = request;
    private readonly LanguageServerState State = state;

    public Response? HandleRequest()
    {
        DidSaveNotification didSaveNotification = DidSaveNotification.From(Request);
        _ = State.UpdateDocument(didSaveNotification.Params.TextDocument.Uri);
        return null;
    }
}
