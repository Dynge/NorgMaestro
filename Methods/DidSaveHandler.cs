using CeorgLsp.Rpc;

namespace CeorgLsp.Methods
{
    public class DidSaveHandler : IMessageHandler
    {
        public required RpcMessage Request { get; init; }

        public Response? HandleRequest()
        {
            // CompletionRequestParams completionParams =
            //     Request.Params!.Value.Deserialize<CompletionRequestParams>();
            // List<CompletionItem> res = new([]);
            //
            // NeorgMetadata metadata = NorgParser.GetMetadata(completionParams.TextDocument.Uri);
            // foreach (string category in metadata.Categories)
            // {
            //     res.Add(new() { Label = category });
            // }
            return null;
        }
    }
}
