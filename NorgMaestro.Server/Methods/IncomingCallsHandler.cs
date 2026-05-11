using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class IncomingCallsHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Task<Response?> HandleRequest()
    {
        IncomingCallsRequest completionRequest = IncomingCallsRequest.From(_request);

        if (
            _state.References.TryGetValue(new Uri(completionRequest.Params.Item.Uri), out var refs)
            is false
        )
        {
            return Task.FromResult<Response?>(
                Response.OfSuccess(completionRequest.Id, new List<IncomingCallsResponseParams>())
            );
        }

        List<IncomingCallsResponseParams> response = refs
            .GroupBy(reference => reference.Location.Uri)
            .Select(group =>
            {
                _state.Documents.TryGetValue(new(group.Key), out var doc);
                TextRange displayRange = group.First().Location.Range;
                return new IncomingCallsResponseParams()
                {
                    From = SymbolBuilders.FileCallHierarchyItem(
                        group.Key,
                        doc?.Metadata.Title?.Name ?? "Unknown title",
                        displayRange
                    ),
                    FromRanges = [.. group.Select(reference => reference.Location.Range)],
                };
             })
            .OrderBy(call => call.From.Uri)
            .ToList();

        return Task.FromResult<Response?>(Response.OfSuccess(completionRequest.Id, response));
    }
}
