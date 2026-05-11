using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class ReferencesHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly DocumentLineReader _lineReader = new(state);
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Task<Response?> HandleRequest()
    {
        ReferencesRequest referenceRequest = ReferencesRequest.From(_request);

        string line = _lineReader.GetLine(
            referenceRequest.Params.TextDocument.Uri,
            referenceRequest.Params.Position
        );

        if (
            _state.TryResolveTargetUriAtPosition(
                referenceRequest.Params.TextDocument.Uri,
                referenceRequest.Params.Position,
                line,
                out Uri targetUri
            ) is false
        )
        {
            return Task.FromResult<Response?>(
                Response.OfSuccess(referenceRequest.Id, Array.Empty<Location>())
            );
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

        return Task.FromResult<Response?>(
            Response.OfSuccess(referenceRequest.Id, references.ToArray())
        );
    }

}
