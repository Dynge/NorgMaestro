using NorgMaestro.Rpc;

namespace NorgMaestro.Methods
{
    public class HandlerFactory
    {
        public struct MethodType
        {
            public const string Initialize = "initialize";
            public const string Initialized = "initialized";
            public const string Completion = "textDocument/completion";
            public const string Rename = "textDocument/rename";
            public const string WorkspaceSymbols = "workspace/symbol";
            public const string References = "textDocument/references";
            public const string IncomingCalls = "callHierarchy/incomingCalls";
            public const string PrepareCallHierarchy = "textDocument/prepareCallHierarchy";
            public const string DidSave = "textDocument/didSave";
            public const string Shutdown = "shutdown";
            public const string Exit = "exit";
        }

        public required IRpcWriter Writer { get; init; }
        public required LanguageServerState State { get; init; }

        public IMessageHandler CreateHandler(RpcMessage req)
        {
            IMessageHandler handler = req.Method switch
            {
                MethodType.Shutdown => new ShutdownHandler() { Request = req, Writer = Writer },
                MethodType.Exit => new ExitHandler() { Writer = Writer },
                MethodType.DidSave => new DidSaveHandler() { Request = req, State = State },
                MethodType.Initialize => new InitializeHandler() { Request = req, State = State },
                MethodType.Initialized => new InitializedHandler() { Writer = Writer, },
                MethodType.Completion => new CompletionHandler() { Request = req, State = State },
                MethodType.Rename => new RenameHandler() { Request = req, State = State },
                MethodType.PrepareCallHierarchy
                    => new PrepareCallHierarchyHandler() { Request = req, State = State },
                MethodType.IncomingCalls
                    => new IncomingCallsHandler() { Request = req, State = State },
                MethodType.References => new ReferencesHandler() { Request = req, State = State },
                MethodType.WorkspaceSymbols
                    => new WorkspaceSymbolHandler() { Request = req, State = State },
                _ => new CantHandler() { Request = req, Writer = Writer },
            };

            return handler;
        }
    }
}
