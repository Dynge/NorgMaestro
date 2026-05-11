using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class DefinitionHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Task<Response?> HandleRequest()
    {
        DefinitionRequest definitionRequest = DefinitionRequest.From(_request);

        string line = FileUtil
            .ReadRange(
                definitionRequest.Params.TextDocument.Uri,
                new()
                {
                    Start = definitionRequest.Params.Position,
                    End = definitionRequest.Params.Position
                }
            )
            .FirstOrDefault("");

        NorgLink? link = NorgParser.ParseLink(
            definitionRequest.Params.TextDocument.Uri,
            definitionRequest.Params.Position,
            line
        );

        Uri targetUri;
        if (link is not null)
        {
            targetUri = _state.ResolveLinkUri(link);
        }
        else if (_state.TryGetTitleTarget(definitionRequest.Params.TextDocument.Uri, definitionRequest.Params.Position, out Uri titleTarget))
        {
            targetUri = titleTarget;
        }
        else
        {
            return Task.FromResult<Response?>(
                Response.OfSuccess(definitionRequest.Id, Array.Empty<Location>())
            );
        }

        if (_state.Documents.TryGetValue(targetUri, out Document? doc) is false)
        {
            return Task.FromResult<Response?>(
                Response.OfSuccess(definitionRequest.Id, Array.Empty<Location>())
            );
        }

        TextRange targetRange = doc.Metadata.Title?.Range
            ?? new()
            {
                Start = new() { Line = 0, Character = 0 },
                End = new() { Line = 0, Character = 0 },
            };

        Location location = new() { Uri = doc.Uri.AbsoluteUri, Range = targetRange };
        return Task.FromResult<Response?>(Response.OfSuccess(definitionRequest.Id, new[] { location }));
    }
}
