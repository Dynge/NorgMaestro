using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server;

internal sealed class NeorgLspServer(IRpcReader reader, IHandlerResolver handlerResolver)
{
    private readonly IRpcReader _reader = reader;
    private readonly IHandlerResolver _handlerResolver = handlerResolver;

    public async Task Startup()
    {
        while (true)
        {
            var req = await _reader.DecodeAsync();
            if (req is null)
            {
                continue;
            }
            _ = Task.Run(async () => await HandleRequest(req));
        }
    }

    private async Task HandleRequest(RpcMessage req)
    {
        try
        {
            var didSucceed = _handlerResolver.TryHandleRequest(req);
            if (!didSucceed) { }
        }
        catch (InvalidDataException)
        {
            return;
        }

        await Task.CompletedTask;
    }
}
