using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class InitializeHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public async Task<Response?> HandleRequest()
    {
        InitializeRequest initRequest = InitializeRequest.From(_request);
        Uri rootUri;
        if (
            initRequest.Params.WorkspaceFolders is not null
            && initRequest.Params.WorkspaceFolders.Any()
        )
        {
            rootUri = initRequest.Params.WorkspaceFolders.First().Uri;
        }
        else if (initRequest.Params.RootUri is not null)
        {
            rootUri = initRequest.Params.RootUri;
        }
        else
        {
            throw new ArgumentException(
                $"Found no root folder in init params: {initRequest.Params}"
            );
        }
        await _state.Initialize(rootUri);
        InitializeResultParams res = new()
        {
            Capabilities = new()
            {
                CompletionProvider = new()
                {
                    ResolveProvider = false,
                    TriggerCharacters = ['æ', 'ø', 'å', '{'],
                },
                WorkspaceSymbolProvider = true,
                ReferencesProvider = true,
                CallHierarchyProvider = true,
                RenameProvider = true,
                HoverProvider = true,
            },
        };

        return Response.OfSuccess(initRequest.Id, res);
    }
}

public class InitializedHandler(IRpcWriter writer) : IMessageHandler
{
    private readonly IRpcWriter _writer = writer;

    public async Task<Response?> HandleRequest()
    {
        await _writer.EncodeAndWrite(Notification.Default("Initialized!", MessageType.Debug));
        return null;
    }
}
