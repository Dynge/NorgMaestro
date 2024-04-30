using System.Text;
using System.Text.Json;
using CeorgLsp.Methods;
using CeorgLsp.Rpc;

namespace CeorgLsp
{
    public class NeorgLspServer
    {
        public static void Main()
        {
            RpcMessageReaderWriter writer =
                new(Console.OpenStandardInput(), Console.OpenStandardOutput());
            LanguageServerState state = new();
            HandlerFactory handlerFactory = new() { Writer = writer, State = state };
            while (true)
            {
                RpcMessage? req = writer.Decode();
                if (req is null)
                {
                    continue;
                }

                try
                {
                    Response? res = handlerFactory.CreateHandler(req)?.HandleRequest();
                    if (res is not null)
                    {
                        writer.EncodeAndWrite(
                            Notification.Default(
                                Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(res)),
                                1
                            )
                        );

                        writer.EncodeAndWrite(res);
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
}
