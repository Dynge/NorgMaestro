using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class PrepareRenameHandler(RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;

    public Response? HandleRequest()
    {
        PrepareRenameRequest prepareRenameRequest = PrepareRenameRequest.From(_request);
        string line = FileUtil
            .ReadRange(
                prepareRenameRequest.Params.TextDocument.Uri,
                new() { Start = prepareRenameRequest.Params.Position, End = prepareRenameRequest.Params.Position }
            )
            .FirstOrDefault("");

        NorgLink? link = NorgParser.ParseLink(
            prepareRenameRequest.Params.TextDocument.Uri,
            prepareRenameRequest.Params.Position,
            line
        );

        if (link is null)
        {
            return Response.OfSuccess(prepareRenameRequest.Id);
        }

        return Response.OfSuccess(prepareRenameRequest.Id, link.LinkTextRange);
    }
}
