using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server;

internal sealed class NeorgLspServer
{
    private readonly IRpcReader _reader;
    private readonly IRpcWriter _writer;
    private readonly HandlerFactory _handlerFactory;
    private readonly LanguageServerState _state;

    public NeorgLspServer(IRpcWriter writer, IRpcReader reader, LanguageServerState? state = null)
    {
        _reader = reader;
        _writer = writer;
        _state = state ?? new();
        _handlerFactory = new(writer, _state);
    }

    public NeorgLspServer()
    {
        RpcMessageReader reader = new(Console.OpenStandardInput());
        RpcMessageWriter writer = new(Console.OpenStandardOutput());
        _reader = reader;
        _writer = writer;
        _state = new();
        _handlerFactory = new(writer, _state);
    }

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
                Response? res = _handlerFactory.CreateHandler(req)?.HandleRequest();
                if (res is not null)
                {
                    _writer.EncodeAndWrite(res);
                }
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
