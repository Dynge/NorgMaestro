using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class CodeActionHandler(RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private const string UnresolvedPrefix = "Unresolved note link: ";
    public const string CreateNoteCommand = "norgmaestro.createNote";

    public Response? HandleRequest()
    {
        CodeActionRequest codeActionRequest = CodeActionRequest.From(_request);
        List<CodeAction> actions = [];

        foreach (Diagnostic diagnostic in codeActionRequest.Params.Context.Diagnostics)
        {
            if (diagnostic.Message.StartsWith(UnresolvedPrefix) is false)
            {
                continue;
            }

            string targetPath = diagnostic.Message[UnresolvedPrefix.Length..];
            actions.Add(
                new()
                {
                    Title = "Create missing note",
                    Kind = CodeActionKind.QuickFix,
                    Command = new()
                    {
                        Title = "Create missing note",
                        Command = CreateNoteCommand,
                        Arguments = [targetPath],
                    },
                }
            );
        }

        return Response.OfSuccess(codeActionRequest.Id, actions.ToArray());
    }
}
