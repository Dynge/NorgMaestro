using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server;

public class NeorgLspServer
{
    public static void Main()
    {
        RpcMessageReaderWriter readerWriter =
            new(Console.OpenStandardInput(), Console.OpenStandardOutput());
        LanguageServerState state = new();
        HandlerFactory handlerFactory = new() { Writer = readerWriter, State = state };
        while (true)
        {
            RpcMessage? req = readerWriter.Decode();
            if (req is null)
            {
                continue;
            }

            try
            {
                Response? res = handlerFactory.CreateHandler(req)?.HandleRequest();
                if (res is not null)
                {
                    readerWriter.EncodeAndWrite(res);
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
