using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class InitializeHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Response? HandleRequest()
    {
        InitializeRequest initRequest = InitializeRequest.From(_request);
        _state.Initialize(initRequest.Params.RootUri);
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
    private readonly IRpcWriter _writer = writer;

    public Response? HandleRequest()
    {
        _writer.EncodeAndWrite(Notification.Default("Initialized!", MessageType.Debug));
        return null;
    }
}
