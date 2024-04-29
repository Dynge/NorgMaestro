using CeorgLsp.Rpc;

namespace CeorgLsp.Methods
{
    public class NoneHandler : IMessageHandler
    {
        public Response? HandleRequest()
        {
            return null;
        }
    }
}
