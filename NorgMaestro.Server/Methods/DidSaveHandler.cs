using NorgMaestro.Rpc;

namespace NorgMaestro.Methods;

public class DidSaveHandler : IMessageHandler
{
    public required RpcMessage Request { get; init; }
    public required LanguageServerState State { get; init; }

    public Response? HandleRequest()
    {
        DidSaveNotification didSaveNotification = DidSaveNotification.From(Request);
        _ = State.UpdateDocument(didSaveNotification.Params.TextDocument.Uri);
        return null;
    }
}
