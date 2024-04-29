using System.Text.Json;
using CeorgLsp.Parser;
using CeorgLsp.Rpc;

namespace CeorgLsp.Methods
{
    public class CompletionHandler : IMessageHandler
    {
        public required RpcMessage Request { get; init; }

        public Response HandleRequest()
        {
            CompletionRequestParams completionParams =
                Request.Params!.Value.Deserialize<CompletionRequestParams>();
            List<CompletionItem> res = new([]);

            NeorgMetadata metadata = NorgParser.GetMetadata(completionParams.TextDocument.Uri);
            foreach (string category in metadata.Categories)
            {
                res.Add(new() { Label = category });
            }
            return Response.OfSuccess(Request.Id, res);
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
