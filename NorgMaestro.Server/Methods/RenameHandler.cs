using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class RenameHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Task<Response?> HandleRequest()
    {
        RenameRequest renameRequest = RenameRequest.From(_request);

        string line = File.ReadLinesAsync(renameRequest.Params.TextDocument.Uri.AbsolutePath)
            .ToBlockingEnumerable()
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
            return Task.FromResult<Response?>(Response.OfSuccess(renameRequest.Id));
        }

        Uri targetUri = _state.ResolveLinkUri(link);
        Document cursorDocument = _state.Documents[targetUri];
        List<TextEdit> changeInCursor = cursorDocument.Metadata.Title switch
        {
            MetaField titleField =>
            [
                new() { NewText = renameRequest.Params.NewName, Range = titleField.Range },
            ],
            _ => [],
        };

        Dictionary<string, TextEdit[]> changeInRefs = _state
            .References[targetUri]
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
                                    targetUri,
                                    reference.Location.Range.Start,
                                    reference.Line
                                )!
                                .LinkTextRange,
                        })
                        .ToArray()
            );

        _ = changeInRefs.TryGetValue(targetUri.AbsoluteUri, out TextEdit[]? value);
        List<TextEdit> textEdits = value switch
        {
            TextEdit[] => [.. value],
            _ => [],
        };
        textEdits.AddRange(changeInCursor);
        changeInRefs[targetUri.AbsoluteUri] = [.. textEdits];

        WorkspaceEdit edit = new() { Changes = changeInRefs };

        return Task.FromResult<Response?>(Response.OfSuccess(renameRequest.Id, edit));
    }
}
