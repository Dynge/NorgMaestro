using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server;

public sealed class Program
{
    public static async Task Main()
    {
        var server = new NeorgLspServer(
            new RpcMessageWriter(Console.OpenStandardOutput()),
            new RpcMessageReader(Console.OpenStandardInput()),
            new()
        );
        var _ = server.Startup();
    }
}
