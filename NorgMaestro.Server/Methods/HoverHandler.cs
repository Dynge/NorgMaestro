using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class HoverHandler(RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;

    public Task<Response?> HandleRequest()
    {
        HoverRequest hoverRequest = HoverRequest.From(_request);

        string? line = FileUtil
            .ReadRange(
                hoverRequest.Params.TextDocument.Uri,
                new() { Start = hoverRequest.Params.Position, End = hoverRequest.Params.Position }
            )
            .FirstOrDefault("");

        NorgLink? link = NorgParser.ParseLink(
            hoverRequest.Params.TextDocument.Uri,
            hoverRequest.Params.Position,
            line
        );

        if (link is null)
        {
            return Task.FromResult<Response?>(Response.OfSuccess(hoverRequest.Id));
        }

        var topRange = new TextRange()
        {
            Start = new() { Line = 0, Character = 0 },
            End = new() { Line = 200, Character = 0 },
        };
        var topLines = FileUtil.ReadRange(link.GetFileLinkUri(), topRange);

        return Task.FromResult<Response?>(
            Response.OfSuccess(
                hoverRequest.Id,
                new Hover()
                {
                    Range = topRange,
                    Contents = new() { Language = "norg", Value = string.Join('\n', topLines) },
                }
            )
        );
    }
}
