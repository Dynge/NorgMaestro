using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class DefinitionHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly DocumentLineReader _lineReader = new(state);
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Task<Response?> HandleRequest()
    {
        DefinitionRequest definitionRequest = DefinitionRequest.From(_request);

        string line = _lineReader.GetLine(
            definitionRequest.Params.TextDocument.Uri,
            definitionRequest.Params.Position
        );

        if (
            _state.TryResolveTargetUriAtPosition(
                definitionRequest.Params.TextDocument.Uri,
                definitionRequest.Params.Position,
                line,
                out Uri targetUri
            ) is false
        )
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
