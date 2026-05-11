using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class PrepareRenameHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Task<Response?> HandleRequest()
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
            if (_state.TryGetTitleTarget(prepareRenameRequest.Params.TextDocument.Uri, prepareRenameRequest.Params.Position, out Uri titleTarget)
                && _state.Documents.TryGetValue(titleTarget, out Document? doc)
                && doc.Metadata.Title is not null)
            {
                return Task.FromResult<Response?>(
                    Response.OfSuccess(prepareRenameRequest.Id, doc.Metadata.Title.Range)
                );
            }

            return Task.FromResult<Response?>(Response.OfSuccess(prepareRenameRequest.Id));
        }

        TextRange range = IsWithinRange(prepareRenameRequest.Params.Position, link.FileRange)
            ? link.FileRange
            : link.LinkTextRange;
        return Task.FromResult<Response?>(Response.OfSuccess(prepareRenameRequest.Id, range));
    }

    private static bool IsWithinRange(Position position, TextRange range)
    {
        if (position.Line < range.Start.Line || position.Line > range.End.Line)
        {
            return false;
        }

        if (position.Line == range.Start.Line && position.Character < range.Start.Character)
        {
            return false;
        }

        if (position.Line == range.End.Line && position.Character > range.End.Character)
        {
            return false;
        }

        return true;
    }
}
