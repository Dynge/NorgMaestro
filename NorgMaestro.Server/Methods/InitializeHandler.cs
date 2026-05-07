using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class InitializeHandler(LanguageServerState state, IRpcWriter writer, RpcMessage request)
    : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;
    private readonly IRpcWriter _writer = writer;

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
        await _state.Initialize(rootUri, initRequest.Params.WorkspaceFolders);
        PublishDiagnostics();
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
                RenameProvider = new() { PrepareProvider = true },
                HoverProvider = true,
                DefinitionProvider = true,
                DocumentSymbolProvider = true,
                DocumentLinkProvider = new() { ResolveProvider = false },
                CodeActionProvider = new() { CodeActionKinds = [CodeActionKind.QuickFix] },
                ExecuteCommandProvider = new() { Commands = [CodeActionHandler.CreateNoteCommand] },
            },
        };

        return Response.OfSuccess(initRequest.Id, res);
    }

    private void PublishDiagnostics()
    {
        Dictionary<Uri, Diagnostic[]> diagnosticsByFile = _state.GetDiagnostics();
        foreach (Document document in _state.Documents.Values)
        {
            Diagnostic[] diagnostics = diagnosticsByFile.GetValueOrDefault(document.Uri, []);
            _writer.EncodeAndWrite(
                Notification.PublishDiagnostics(
                    new() { Uri = document.Uri.AbsoluteUri, Diagnostics = diagnostics }
                )
            );
        }
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
