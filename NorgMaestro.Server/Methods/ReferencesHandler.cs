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

        if (link is null)
        {
            return Task.FromResult<Response?>(Response.OfSuccess(referenceRequest.Id));
        }

        Uri targetUri = _state.ResolveLinkUri(link);
        HashSet<Location> references = _state
            .References.GetValueOrDefault(targetUri, [])
            .Select(reference => reference.Location)
            .ToHashSet();
        if (referenceRequest.Params.Context.IncludeDeclaration)
        {
            _ = references.Add(
                new()
                {
                    Uri = targetUri.AbsoluteUri,
                    Range = new()
                    {
                        Start = new() { Line = 0, Character = 0 },
                        End = new() { Line = 0, Character = 0 },
                    },
                }
            );
        }

        return (references.Count > 0) switch
        {
            true => Task.FromResult<Response?>(Response.OfSuccess(referenceRequest.Id, references)),
            false => Task.FromResult<Response?>(Response.OfSuccess(referenceRequest.Id)),
        };
    }
}
