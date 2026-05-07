using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class DefinitionHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Response? HandleRequest()
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

        if (link is null)
        {
            return Response.OfSuccess(definitionRequest.Id);
        }

        Uri targetUri = _state.ResolveLinkUri(link);
        if (_state.Documents.TryGetValue(targetUri, out Document? doc) is false)
        {
            return Response.OfSuccess(definitionRequest.Id);
        }

        TextRange targetRange = doc.Metadata.Title?.Range
            ?? new()
            {
                Start = new() { Line = 0, Character = 0 },
                End = new() { Line = 0, Character = 0 },
            };

        Location location = new() { Uri = doc.Uri.AbsoluteUri, Range = targetRange };
        return Response.OfSuccess(definitionRequest.Id, new[] { location });
    }
}
