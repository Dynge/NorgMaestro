using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class DocumentLinkHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Task<Response?> HandleRequest()
    {
        DocumentLinkRequest linkRequest = DocumentLinkRequest.From(_request);
        if (_state.Documents.TryGetValue(linkRequest.Params.TextDocument.Uri, out Document? doc) is false)
        {
            return Task.FromResult<Response?>(
                Response.OfSuccess(linkRequest.Id, Array.Empty<DocumentLink>())
            );
        }

        DocumentLink[] links = NorgParser
            .ParseLinks(doc.Uri, doc.Content)
            .Select(link => new DocumentLink()
            {
                Range = link.AbsoluteRange,
                Target = _state.ResolveLinkUri(link).AbsoluteUri,
                Tooltip = link.LinkText,
            })
            .ToArray();

        return Task.FromResult<Response?>(Response.OfSuccess(linkRequest.Id, links));
    }
}
