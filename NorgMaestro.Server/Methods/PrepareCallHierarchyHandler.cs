using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class PrepareCallHierarchyHandler(LanguageServerState state, RpcMessage request)
    : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Task<Response?> HandleRequest()
    {
        PrepareCallHierarchyRequest initRequest = PrepareCallHierarchyRequest.From(_request);
        Document doc = _state.Documents[initRequest.Params.TextDocument.Uri];
        CallHierarchyItem[] items =
        [
            SymbolBuilders.FileCallHierarchyItem(doc.Uri.AbsoluteUri, doc.Metadata.Title?.Name ?? ""),
        ];
        return Task.FromResult<Response?>(Response.OfSuccess(initRequest.Id, items));
    }
}
