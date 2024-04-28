using CeorgLsp.Rpc;

namespace CeorgLsp.Methods
{
    public interface IMessageHandler
    {
        public Response? HandleRequest();
    }
}
