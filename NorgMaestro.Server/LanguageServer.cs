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

    public void Startup()
    {
        while (true)
        {
            RpcMessage? req = _reader.Decode();
            if (req is null)
            {
                continue;
            }

            try
            {
                var didSucceed = _handlerFactory.TryHandleRequest(req);
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
}
