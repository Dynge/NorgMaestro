using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public sealed record HandlerRegistration(string Method, Func<RpcMessage, IMessageHandler> CreateHandler);

public static class DefaultHandlerRegistrations
{
    public static IEnumerable<HandlerRegistration> Create(LanguageServerState state, IRpcWriter writer)
    {
        return
        [
            new(HandlerFactory.MethodType.Shutdown, req => new ShutdownHandler(writer, req)),
            new(HandlerFactory.MethodType.Exit, _ => new ExitHandler(writer)),
            new(HandlerFactory.MethodType.DidOpen, req => new DidOpenHandler(state, writer, req)),
            new(HandlerFactory.MethodType.DidChange, req => new DidChangeHandler(state, writer, req)),
            new(HandlerFactory.MethodType.DidClose, req => new DidCloseHandler(state, writer, req)),
            new(HandlerFactory.MethodType.DidSave, req => new DidSaveHandler(state, writer, req)),
            new(HandlerFactory.MethodType.Initialize, req => new InitializeHandler(state, writer, req)),
            new(HandlerFactory.MethodType.Initialized, _ => new InitializedHandler(writer)),
            new(HandlerFactory.MethodType.Completion, req => new CompletionHandler(state, req)),
            new(HandlerFactory.MethodType.Hover, req => new HoverHandler(state, req)),
            new(HandlerFactory.MethodType.Rename, req => new RenameHandler(state, req)),
            new(HandlerFactory.MethodType.PrepareRename, req => new PrepareRenameHandler(state, req)),
            new(
                HandlerFactory.MethodType.PrepareCallHierarchy,
                req => new PrepareCallHierarchyHandler(state, req)
            ),
            new(HandlerFactory.MethodType.IncomingCalls, req => new IncomingCallsHandler(state, req)),
            new(HandlerFactory.MethodType.OutgoingCalls, req => new OutgoingCallsHandler(state, req)),
            new(HandlerFactory.MethodType.References, req => new ReferencesHandler(state, req)),
            new(HandlerFactory.MethodType.WorkspaceSymbols, req => new WorkspaceSymbolHandler(state, req)),
            new(HandlerFactory.MethodType.Definition, req => new DefinitionHandler(state, req)),
            new(HandlerFactory.MethodType.DocumentSymbol, req => new DocumentSymbolHandler(state, req)),
            new(HandlerFactory.MethodType.DocumentLink, req => new DocumentLinkHandler(state, req)),
            new(HandlerFactory.MethodType.CodeAction, req => new CodeActionHandler(state, req)),
            new(HandlerFactory.MethodType.ExecuteCommand, req => new ExecuteCommandHandler(state, writer, req)),
        ];
    }
}
