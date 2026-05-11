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

        string line = GetLine(referenceRequest.Params.TextDocument.Uri, referenceRequest.Params.Position);

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

    private string GetLine(Uri sourceUri, Position position)
    {
        if (
            _state.Documents.TryGetValue(sourceUri, out Document? document)
            && position.Line < (uint)document.Content.Length
        )
        {
            return document.Content[(int)position.Line];
        }

        return FileUtil
            .ReadRange(
                sourceUri,
                new()
                {
                    Start = position,
                    End = position,
                }
            )
            .FirstOrDefault("");
    }
}
