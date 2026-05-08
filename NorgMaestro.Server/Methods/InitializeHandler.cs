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
        Uri rootUri = ResolveRootUri(initRequest.Params);
        _state.Initialize(
            rootUri,
            initRequest.Params.WorkspaceFolders,
            initRequest.Params.InitializationOptions
        );
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
                CodeActionProvider = new()
                {
                    CodeActionKinds = [CodeActionKind.QuickFix, CodeActionKind.RefactorRewrite]
                },
                ExecuteCommandProvider = new()
                {
                    Commands =
                    [
                        CodeActionHandler.CreateNoteCommand,
                        CodeActionHandler.CreateNoteAndOpenCommand,
                        CodeActionHandler.CreateBacklinkSectionCommand,
                        CodeActionHandler.ExtractSelectionToNoteCommand,
                        CodeActionHandler.MoveNoteToWorkspaceCommand,
                        CodeActionHandler.CreateNoteFromLinkTextCommand,
                    ]
                },
            },
        };

        await Task.CompletedTask;
        return Response.OfSuccess(initRequest.Id, res);
    }

    private static Uri ResolveRootUri(InitializeRequestParams requestParams)
    {
        if (requestParams.WorkspaceFolders?.Any() is true)
        {
            return requestParams.WorkspaceFolders.First().Uri;
        }

        if (requestParams.RootUri is not null)
        {
            return requestParams.RootUri;
        }

        if (string.IsNullOrWhiteSpace(requestParams.RootPath) is false)
        {
            return new Uri(Path.GetFullPath(requestParams.RootPath));
        }

        return new Uri(Path.GetFullPath(AppContext.BaseDirectory));
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
