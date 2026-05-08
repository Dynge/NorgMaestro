using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class HandlerFactory(LanguageServerState state, IRpcWriter writer) : IHandlerResolver
{
    private readonly IRpcWriter _writer = writer;
    private readonly Dictionary<string, Func<RpcMessage, IMessageHandler>> _registrations =
        BuildRegistrationMap(DefaultHandlerRegistrations.Create(state, writer));

    public HandlerFactory(
        LanguageServerState state,
        IRpcWriter writer,
        IEnumerable<HandlerRegistration> registrations
    )
        : this(state, writer)
    {
        _registrations = BuildRegistrationMap(registrations);
    }

    public IMessageHandler CreateHandler(RpcMessage req)
    {
        if (_registrations.TryGetValue(req.Method, out Func<RpcMessage, IMessageHandler>? factory))
        {
            return factory(req);
        }

        return new CantHandler(_writer, req);
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
        public const string DidOpen = "textDocument/didOpen";
        public const string DidChange = "textDocument/didChange";
        public const string DidClose = "textDocument/didClose";
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

    private static Dictionary<string, Func<RpcMessage, IMessageHandler>> BuildRegistrationMap(
        IEnumerable<HandlerRegistration> registrations
    )
    {
        ArgumentNullException.ThrowIfNull(registrations);

        Dictionary<string, Func<RpcMessage, IMessageHandler>> map = [];
        foreach (HandlerRegistration registration in registrations)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(registration.Method);
            if (!map.TryAdd(registration.Method, registration.CreateHandler))
            {
                throw new ArgumentException(
                    $"Duplicate handler registration for method '{registration.Method}'.",
                    nameof(registrations)
                );
            }
        }

        return map;
    }
}
