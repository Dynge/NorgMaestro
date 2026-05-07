using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class ReferencesHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Task<Response?> HandleRequest()
    {
        ReferencesRequest referenceRequest = ReferencesRequest.From(_request);

        string line = FileUtil
            .ReadRange(
                referenceRequest.Params.TextDocument.Uri,
                new()
                {
                    Start = referenceRequest.Params.Position,
                    End = referenceRequest.Params.Position,
                }
            )
            .FirstOrDefault("");

        NorgLink? link = NorgParser.ParseLink(
            referenceRequest.Params.TextDocument.Uri,
            referenceRequest.Params.Position,
            line
        );

        Uri targetUri;
        if (link is not null)
        {
            targetUri = _state.ResolveLinkUri(link);
        }
        else if (
            _state.TryGetTitleTarget(
                referenceRequest.Params.TextDocument.Uri,
                referenceRequest.Params.Position,
                out Uri titleTarget
            )
        )
        {
            targetUri = titleTarget;
        }
        else
        {
            return Task.FromResult<Response?>(Response.OfSuccess(referenceRequest.Id), []);
        }

        HashSet<Location> references = _state
            .References.GetValueOrDefault(targetUri, [])
            .Select(reference => reference.Location)
            .ToHashSet();
        if (referenceRequest.Params.Context.IncludeDeclaration)
        {
            var defaultRange = new TextRange()
            {
                Start = new() { Line = 0, Character = 0 },
                End = new() { Line = 0, Character = 0 },
            };
            TextRange declarationRange = _state.Documents.TryGetValue(
                targetUri,
                out Document? targetDoc
            )
                ? targetDoc.Metadata.Title?.Range ?? defaultRange
                : defaultRange;
            _ = references.Add(new() { Uri = targetUri.AbsoluteUri, Range = declarationRange });
        }

        return Response.OfSuccess(referenceRequest.Id, references.ToArray());
    }
}
