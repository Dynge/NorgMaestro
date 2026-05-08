using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server;

public sealed class Program
{
    public static Task Main()
    {
        LanguageServerState state = new();
        IRpcWriter writer = new RpcMessageWriter(Console.OpenStandardOutput());
        IRpcReader reader = new RpcMessageReader(Console.OpenStandardInput());
        HandlerFactory handlerFactory = new(state, writer);

        var server = new NeorgLspServer(reader, handlerFactory);
        return server.Startup();
    }
}
