using NorgMaestro.Rpc;

namespace NorgMaestro.Methods;

public interface IMessageHandler
{
    public Response HandleRequest();
}
