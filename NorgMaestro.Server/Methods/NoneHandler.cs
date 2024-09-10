using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class NoneHandler : IMessageHandler
{
    public Response? HandleRequest()
    {
        return null;
    }
}
