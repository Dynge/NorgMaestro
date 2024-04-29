using CeorgLsp.Methods;
using CeorgLsp.Rpc;

namespace CeorgLsp
{
    public class StartLsp
    {
        public static void Main()
        {
            RpcMessageWriter writer =
                new(Console.OpenStandardInput(), Console.OpenStandardOutput());
            while (true)
            {
                RpcMessage? req = writer.Decode();
                if (req is null)
                {
                    continue;
                }

                try
                {
                    Response? res = new HandlerFactory() { Writer = writer }
                        .CreateHandler(req)
                        ?.HandleRequest();
                    if (res is not null)
                    {
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
