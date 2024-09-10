using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class HoverHandler : IMessageHandler
{
    public required RpcMessage Request { get; init; }
    public required LanguageServerState State { get; init; }

    public Response? HandleRequest()
    {
        HoverRequest hoverRequest = HoverRequest.From(Request);

        string? line = FileUtil
            .ReadRange(
                hoverRequest.Params.TextDocument.Uri,
                new() { Start = hoverRequest.Params.Postion, End = hoverRequest.Params.Postion, }
            )
            .FirstOrDefault("");

        NorgLink? link = NorgParser.ParseLink(
            hoverRequest.Params.TextDocument.Uri,
            hoverRequest.Params.Postion,
            line
        );

        if (link is null)
        {
            return Response.OfSuccess(hoverRequest.Id);
        }

        TextRange topRange =
            new()
            {
                Start = new() { Line = 0, Character = 0 },
                End = new() { Line = 200, Character = 0 },
            };
        string[] topLines = FileUtil.ReadRange(link.GetFileLinkUri(), topRange);

        return Response.OfSuccess(
            hoverRequest.Id,
            new Hover()
            {
                Range = topRange,
                Contents = new() { Language = "norg", Value = string.Join('\n', topLines), }
            }
        );
    }
}
