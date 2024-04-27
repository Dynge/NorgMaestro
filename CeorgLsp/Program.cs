using CeorgLsp.Method;
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
                    Stdin = Console.OpenStandardInput(),
                    Stdout = Console.OpenStandardOutput()
                };

            writer.EncodeAndWrite(
                new Notification()
                {
                    Params = new NotificationParams() { Message = "Hello from Csharp" }
                }
            );
            Request? req = writer.Decode();
            if (req is null)
            {
                // Console.WriteLine("Invalid Rpc request received.");
                return;
            }
            Response? res = null;
            if (req.Method == "initialize")
            {
                res = new InitializationHandler() { Request = req }.HandleRequest();
                writer.EncodeAndWrite(
                    new Notification()
                    {
                        Params = new NotificationParams() { Message = "Hello from Csharp" }
                    }
                );
            }
            else
            {
                // Console.WriteLine(string.Format("Unknown method {0}", req.Method));
            }

            if (res is not null)
            {
                writer.EncodeAndWrite(res);
            }
        }
    }
}
