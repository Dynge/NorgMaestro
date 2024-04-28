using System.Text;
using CeorgLsp.Methods;
using CeorgLsp.Rpc;

namespace CeorgLsp
{
    public class StartLsp
    {
        public static void Main()
        {
            RpcMessageWriter writer =
                new()
                {
                    StdinReader = new(Console.OpenStandardInput(), Encoding.UTF8),
                    Stdout = Console.OpenStandardOutput()
                };
            while (true)
            {
                RpcMessage? req = writer.Decode();

                if (req is null)
                {
                    continue;
                }
                Response? res = null;
                if (req.Method == "initialize")
                {
                    writer.EncodeAndWrite(Notification.Default("Hello from Csharp Neorg-Lsp!!", 1));
                    res = new InitializeHandler()
                    {
                        Request = InitializeRequest.From(req)
                    }.HandleRequest();
                }
                else
                {
                    writer.EncodeAndWrite(
                        Notification.Default($"Cannot handle '{req.Method}'!!.", 1)
                    );
                }

                if (res is not null)
                {
                    writer.EncodeAndWrite(res);
                }

                writer.EncodeAndWrite(Notification.Default($"Parsed request!!.", 1));
            }
        }
    }
}
