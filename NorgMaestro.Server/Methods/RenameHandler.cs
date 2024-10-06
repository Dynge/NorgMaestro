using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class RenameHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Response? HandleRequest()
    {
        RenameRequest renameRequest = RenameRequest.From(_request);

        string line = File.ReadLines(renameRequest.Params.TextDocument.Uri.AbsolutePath)
            .Skip((int)renameRequest.Params.Position.Line)
            .FirstOrDefault("");

        uint charPos = renameRequest.Params.Position.Character;
        NorgLink? link = NorgParser.ParseLink(
            renameRequest.Params.TextDocument.Uri,
            renameRequest.Params.Position,
            line
        );

        if (link is null)
        {
            return Response.OfSuccess(renameRequest.Id);
        }

        Document cursorDocument = _state.Documents[link.GetFileLinkUri()];
        List<TextEdit> changeInCursor = cursorDocument.Metadata.Title switch
        {
            MetaField titleField
                => [new() { NewText = renameRequest.Params.NewName, Range = titleField.Range, }],
            _ => [],
        };

        Dictionary<string, TextEdit[]> changeInRefs = _state
            .References[link.GetFileLinkUri()]
            .ToLookup(reference => reference.Location.Uri)
            .ToDictionary(
                kvp => kvp.Key,
                kvp =>
                    kvp.ToList()
                        .Select(reference => new TextEdit()
                        {
                            NewText = renameRequest.Params.NewName,
                            Range = NorgParser
                                .ParseLink(
                                    link.GetFileLinkUri(),
                                    reference.Location.Range.Start,
                                    reference.Line
                                )!
                                .LinkTextRange
                        })
                        .ToArray()
            );

        _ = changeInRefs.TryGetValue(link.GetFileLinkUri().AbsoluteUri, out TextEdit[]? value);
        List<TextEdit> textEdits = value switch
        {
            TextEdit[] => [.. value],
            _ => [],
        };
        textEdits.AddRange(changeInCursor);
        changeInRefs[link.GetFileLinkUri().AbsoluteUri] = [.. textEdits];

        WorkspaceEdit edit = new() { Changes = changeInRefs };

        return Response.OfSuccess(renameRequest.Id, edit);
    }
}
