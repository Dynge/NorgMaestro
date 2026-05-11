using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class HoverHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

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

        Uri targetUri = _state.ResolveLinkUri(link);
        if (_state.Documents.TryGetValue(targetUri, out Document? targetDoc) is false)
        {
            return Task.FromResult<Response?>(Response.OfSuccess(hoverRequest.Id));
        }

        var previewRange = new TextRange()
        {
            Start = new() { Line = 0, Character = 0 },
            End = new() { Line = Math.Min((uint)targetDoc.Content.Length, 200u), Character = 0 },
        };
        string preview = string.Join('\n', targetDoc.Content.Take(200));

        return Task.FromResult<Response?>(
            Response.OfSuccess(
                hoverRequest.Id,
                new Hover()
                {
                    Range = link.AbsoluteRange,
                    Contents = new() { Language = "norg", Value = preview },
                }
            )
        );
    }
}
