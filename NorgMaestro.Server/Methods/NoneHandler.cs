using NorgMaestro.Rpc;

namespace NorgMaestro.Methods
{
    public class NoneHandler : IMessageHandler
    {
        public Response? HandleRequest()
        {
            return null;
        }
    }
}
