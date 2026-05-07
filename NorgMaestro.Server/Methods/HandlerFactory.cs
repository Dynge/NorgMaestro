using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class HandlerFactory(LanguageServerState state, IRpcWriter writer)
{
    private readonly IRpcWriter _writer = writer;
    private readonly LanguageServerState _state = state;

    public IMessageHandler CreateHandler(RpcMessage req)
    {
        IMessageHandler handler = req.Method switch
        {
            MethodType.Shutdown => new ShutdownHandler(_writer, req),
            MethodType.Exit => new ExitHandler(_writer),
            MethodType.DidSave => new DidSaveHandler(_state, _writer, req),
            MethodType.Initialize => new InitializeHandler(_state, _writer, req),
            MethodType.Initialized => new InitializedHandler(_writer),
            MethodType.Completion => new CompletionHandler(_state, req),
            MethodType.Hover => new HoverHandler(_state, req),
            MethodType.Rename => new RenameHandler(_state, req),
            MethodType.PrepareRename => new PrepareRenameHandler(_state, req),
            MethodType.PrepareCallHierarchy => new PrepareCallHierarchyHandler(_state, req),
            MethodType.IncomingCalls => new IncomingCallsHandler(_state, req),
            MethodType.OutgoingCalls => new OutgoingCallsHandler(_state, req),
            MethodType.References => new ReferencesHandler(_state, req),
            MethodType.WorkspaceSymbols => new WorkspaceSymbolHandler(_state, req),
            MethodType.Definition => new DefinitionHandler(_state, req),
            MethodType.DocumentSymbol => new DocumentSymbolHandler(_state, req),
            MethodType.DocumentLink => new DocumentLinkHandler(_state, req),
            MethodType.CodeAction => new CodeActionHandler(req),
            MethodType.ExecuteCommand => new ExecuteCommandHandler(_state, _writer, req),
            _ => new CantHandler(_writer, req),
        };

        return handler;
    }

    public async Task<bool> TryHandleRequest(RpcMessage req)
    {
        var handler = CreateHandler(req);
        var didHandle = handler is not CantHandler;
        var res = await handler.HandleRequest();
        if (res is not null)
        {
            await _writer.EncodeAndWrite(res);
        }
        return didHandle;
    }

    public struct MethodType
    {
        public const string Initialize = "initialize";
        public const string Initialized = "initialized";
        public const string Completion = "textDocument/completion";
        public const string Rename = "textDocument/rename";
        public const string PrepareRename = "textDocument/prepareRename";
        public const string WorkspaceSymbols = "workspace/symbol";
        public const string References = "textDocument/references";
        public const string IncomingCalls = "callHierarchy/incomingCalls";
        public const string OutgoingCalls = "callHierarchy/outgoingCalls";
        public const string PrepareCallHierarchy = "textDocument/prepareCallHierarchy";
        public const string DidSave = "textDocument/didSave";
        public const string Hover = "textDocument/hover";
        public const string Definition = "textDocument/definition";
        public const string DocumentSymbol = "textDocument/documentSymbol";
        public const string DocumentLink = "textDocument/documentLink";
        public const string CodeAction = "textDocument/codeAction";
        public const string ExecuteCommand = "workspace/executeCommand";
        public const string Shutdown = "shutdown";
        public const string Exit = "exit";
    }
}
