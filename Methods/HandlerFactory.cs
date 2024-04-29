using CeorgLsp.Rpc;

namespace CeorgLsp.Methods
{
    public class HandlerFactory
    {
        public struct MethodType
        {
            public const string Initialize = "initialize";
            public const string Initialized = "initialized";
            public const string Completion = "textDocument/completion";
            public const string DidSave = "textDocument/didSave";
            public const string Shutdown = "shutdown";
            public const string Exit = "exit";
        }

        public required RpcMessageWriter Writer { get; init; }

        public IMessageHandler CreateHandler(RpcMessage req)
        {
            IMessageHandler handler = req.Method switch
            {
                MethodType.Shutdown => new ShutdownHandler() { Request = req, Writer = Writer },
                MethodType.Exit => new ExitHandler() { Writer = Writer },
                MethodType.DidSave => new EmptyHandler() { Request = req },
                MethodType.Initialize => new InitializeHandler() { Request = req, },
                MethodType.Initialized => new InitializedHandler() { Writer = Writer, },
                MethodType.Completion => new CompletionHandler() { Request = req },
                _ => new CantHandler() { Request = req, Writer = Writer },
            };

            return handler;
        }
    }
}
