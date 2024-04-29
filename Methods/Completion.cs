using CeorgLsp.Parser;
using CeorgLsp.Rpc;

namespace CeorgLsp.Methods
{
    public class CompletionHandler : IMessageHandler
    {
        public required RpcMessage Request { get; init; }

        public Response HandleRequest()
        {
            CompletionRequest completionRequest = CompletionRequest.From(Request);
            List<CompletionItem> res = new([]);

            NeorgMetadata metadata = NorgParser.GetMetadata(
                completionRequest.Params.TextDocument.Uri
            );
            foreach (string category in metadata.Categories)
            {
                res.Add(new() { Label = category });
            }
            return Response.OfSuccess(completionRequest.Id, res);
        }
    }

    // string[] notes = Directory.GetFiles("/home/michael/notes/");
    // foreach (string note_path in notes)
    // {
    //     if (Path.GetExtension(note_path) is not "norg")
    //     {
    //         continue;
    //     }
    //
    //
    //     if (note_path.SequenceEqual(completionParams.TextDocument.Uri.LocalPath))
    //     {
    //         continue;
    //     }
    //
    //     NeorgMetadata metadata = NorgParser.GetMetadata(new Uri(note_path));
    //     foreach (string category in metadata.Categories)
    //     {
    //         res.Add(new() { Label = category });
    //     }
    // }
}
