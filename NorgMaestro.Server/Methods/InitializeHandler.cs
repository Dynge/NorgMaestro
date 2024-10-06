using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class InitializeHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage Request = request;
    private readonly LanguageServerState State = state;

    public Response? HandleRequest()
    {
        InitializeRequest initRequest = InitializeRequest.From(Request);
        State.Initialize(initRequest.Params.RootUri);
        InitializeResultParams res =
            new()
            {
                Capabilities = new()
                {
                    CompletionProvider = new()
                    {
                        ResolveProvider = false,
                        TriggerCharacters = ['æ', 'ø', 'å', '{']
                    },
                    WorkspaceSymbolProvider = true,
                    ReferencesProvider = true,
                    CallHierarchyProvider = true,
                    RenameProvider = true,
                    HoverProvider = true,
                }
            };

        return Response.OfSuccess(initRequest.Id, res);
    }
}

public class InitializedHandler(IRpcWriter writer) : IMessageHandler
{
    private readonly IRpcWriter Writer = writer;

    public Response? HandleRequest()
    {
        Writer.EncodeAndWrite(Notification.Default("Initialized!", MessageType.Debug));
        return null;
    }
}
