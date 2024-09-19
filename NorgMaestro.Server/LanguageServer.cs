using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server;

internal sealed class NeorgLspServer
{
    private readonly IRpcReader _reader;
    private readonly IRpcWriter _writer;
    private readonly HandlerFactory _handlerFactory;
    private readonly LanguageServerState _state = new();

    public NeorgLspServer()
    {
        RpcMessageReaderWriter readerWriter =
            new(Console.OpenStandardInput(), Console.OpenStandardOutput());
        _reader = readerWriter;
        _writer = readerWriter;
        _handlerFactory = new() { Writer = readerWriter, State = _state };
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
