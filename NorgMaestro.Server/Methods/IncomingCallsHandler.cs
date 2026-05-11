using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class IncomingCallsHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Task<Response?> HandleRequest()
    {
        IncomingCallsRequest completionRequest = IncomingCallsRequest.From(_request);
        Uri targetUri = new(completionRequest.Params.Item.Uri);

        if (_state.References.TryGetValue(targetUri, out HashSet<ReferenceLocation>? references) is false)
        {
            return Task.FromResult<Response?>(
                Response.OfSuccess(completionRequest.Id, new List<IncomingCallsResponseParams>())
            );
        }

        List<IncomingCallsResponseParams> response = BuildIncomingCalls(references);

        return Task.FromResult<Response?>(Response.OfSuccess(completionRequest.Id, response));
    }

    private List<IncomingCallsResponseParams> BuildIncomingCalls(
        IEnumerable<ReferenceLocation> references
    )
    {
        return references
            .GroupBy(reference => reference.Location.Uri)
            .Select(CreateIncomingCall)
            .OrderBy(call => call.From.Uri)
            .ToList();
    }

    private IncomingCallsResponseParams CreateIncomingCall(IGrouping<string, ReferenceLocation> group)
    {
        _state.Documents.TryGetValue(new Uri(group.Key), out Document? sourceDocument);
        TextRange displayRange = group.First().Location.Range;

        return new()
        {
            From = SymbolBuilders.FileCallHierarchyItem(
                group.Key,
                sourceDocument?.Metadata.Title?.Name ?? "Unknown title",
                displayRange
            ),
            FromRanges = [.. group.Select(reference => reference.Location.Range)],
        };
    }
}
