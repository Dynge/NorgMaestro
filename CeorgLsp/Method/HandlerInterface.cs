using CeorgLsp.Rpc;

namespace CeorgLsp.Method
{
    public interface IMessageHandler
    {
        public Response? HandleRequest();
    }
}
