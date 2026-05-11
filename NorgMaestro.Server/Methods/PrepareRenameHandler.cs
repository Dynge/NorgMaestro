using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class PrepareRenameHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly DocumentLineReader _lineReader = new(state);
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Task<Response?> HandleRequest()
    {
        PrepareRenameRequest prepareRenameRequest = PrepareRenameRequest.From(_request);
        string line = _lineReader.GetLine(
            prepareRenameRequest.Params.TextDocument.Uri,
            prepareRenameRequest.Params.Position
        );

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

        TextRange range = LanguageServerState.IsPositionWithinRange(
            prepareRenameRequest.Params.Position,
            link.FileRange
        )
            ? link.FileRange
            : link.LinkTextRange;
        return Task.FromResult<Response?>(Response.OfSuccess(prepareRenameRequest.Id, range));
    }
}
