using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server;

internal sealed class NeorgLspServer(
    IRpcWriter writer,
    IRpcReader reader,
    LanguageServerState state
)
{
    private readonly IRpcReader _reader = reader;
    private readonly HandlerFactory _handlerFactory = new(state, writer);

    public async Task Startup()
    {
        while (true)
        {
            var req = await _reader.DecodeAsync();
            if (req is null)
            {
                continue;
            }
            // Just run the command!
            _ = Task.Run(async () => await HandleRequest(req));
        }
    }

    private async Task HandleRequest(RpcMessage message)
    {
        try
        {
            var didSucceed = await _handlerFactory.TryHandleRequest(message);
            if (!didSucceed) { }
        }
        catch (InvalidDataException)
        {
            // TODO: Create shutdown exception
            // Shutdown
            return;
        }
    }
}
