using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class CompletionHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Task<Response?> HandleRequest()
    {
        CompletionRequest completionRequest = CompletionRequest.From(_request);

        IEnumerable<CompletionItem> items = IsInsideLinkTarget(completionRequest.Params)
            ? GetLinkCompletions(completionRequest.Params)
            : GetCategoryCompletions(completionRequest.Params);

        return Task.FromResult<Response?>(Response.OfSuccess(completionRequest.Id, items));
    }

    private bool IsInsideLinkTarget(CompletionRequestParams completionRequestParams)
    {
        if (_state.Documents.TryGetValue(completionRequestParams.TextDocument.Uri, out Document? doc) is false)
        {
            return completionRequestParams.CompletionContext?.TriggerCharacter == '{';
        }

        int lineIndex = (int)completionRequestParams.Position.Line;
        if (lineIndex < 0 || lineIndex >= doc.Content.Length)
        {
            return completionRequestParams.CompletionContext?.TriggerCharacter == '{';
        }

        string line = doc.Content[lineIndex];
        int cursor = (int)completionRequestParams.Position.Character;
        if (cursor < 0 || cursor > line.Length)
        {
            return completionRequestParams.CompletionContext?.TriggerCharacter == '{';
        }

        int openBrace = line.LastIndexOf('{', cursor == 0 ? 0 : cursor - 1);
        if (openBrace < 0)
        {
            return completionRequestParams.CompletionContext?.TriggerCharacter == '{';
        }

        int closeBrace = line.IndexOf('}', openBrace);
        if (closeBrace < 0 || cursor > closeBrace)
        {
            return completionRequestParams.CompletionContext?.TriggerCharacter == '{';
        }

        return openBrace + 1 < line.Length && line[openBrace + 1] == ':';
    }

    private IEnumerable<CompletionItem> GetCategoryCompletions(
        CompletionRequestParams completionRequestParams
    )
    {
        HashSet<string> uniqueCategoryNames = [];
        List<CompletionItem> completionItems = [];

        foreach (Document doc in _state.Documents.Values)
        {
            if (doc.Uri.Equals(completionRequestParams.TextDocument.Uri))
            {
                continue;
            }

            foreach (var category in doc.Metadata.Categories)
            {
                if (uniqueCategoryNames.Add(category.Name) is false)
                {
                    continue;
                }

                completionItems.Add(new CompletionItem() { Label = category.Name });
            }
        }

        return completionItems;
    }

    private IEnumerable<CompletionItem> GetLinkCompletions(
        CompletionRequestParams completionRequestParams
    )
    {
        TextEdit editTemplate = GetLinkEditRange(completionRequestParams);
        List<CompletionItem> completionItems = [];

        foreach (Document document in _state.Documents.Values)
        {
            if (document.Uri.Equals(completionRequestParams.TextDocument.Uri))
            {
                continue;
            }

            string title = document.Metadata.Title?.Name ?? "[No Title]";
            completionItems.Add(
                new CompletionItem()
                {
                    Label = title,
                    Kind = CompletionKind.File,
                    TextEdit = new()
                    {
                        Range = editTemplate.Range,
                        NewText =
                            $"{{:{Path.GetFileNameWithoutExtension(document.Uri.AbsolutePath)}:}}[{document.Metadata.Title?.Name ?? ""}]",
                    },
                }
            );
        }

        return completionItems;
    }

    private static TextEdit SingleCharacterFallbackEdit(CompletionRequestParams completionRequestParams)
    {
        return new()
        {
            Range = new()
            {
                Start = new()
                {
                    Character = completionRequestParams.Position.Character - 1,
                    Line = completionRequestParams.Position.Line,
                },
                End = new()
                {
                    Line = completionRequestParams.Position.Line,
                    Character = completionRequestParams.Position.Character,
                },
            },
            NewText = string.Empty,
        };
    }

    private TextEdit GetLinkEditRange(CompletionRequestParams completionRequestParams)
    {
        if (_state.Documents.TryGetValue(completionRequestParams.TextDocument.Uri, out Document? doc) is false)
        {
            return SingleCharacterFallbackEdit(completionRequestParams);
        }

        int lineIndex = (int)completionRequestParams.Position.Line;
        if (lineIndex < 0 || lineIndex >= doc.Content.Length)
        {
            return SingleCharacterFallbackEdit(completionRequestParams);
        }

        string line = doc.Content[lineIndex];
        int cursor = Math.Clamp((int)completionRequestParams.Position.Character, 0, line.Length);
        int openBrace = line.LastIndexOf('{', cursor == 0 ? 0 : cursor - 1);
        int closeBrace = openBrace >= 0 ? line.IndexOf('}', openBrace) : -1;
        if (openBrace < 0 || closeBrace < 0)
        {
            return SingleCharacterFallbackEdit(completionRequestParams);
        }

        int end = closeBrace + 1;
        if (end + 1 < line.Length && line[end] == '[')
        {
            int closeBracket = line.IndexOf(']', end);
            if (closeBracket >= 0)
            {
                end = closeBracket + 1;
            }
        }

        return new()
        {
            Range = new()
            {
                Start = new() { Line = completionRequestParams.Position.Line, Character = (uint)openBrace },
                End = new() { Line = completionRequestParams.Position.Line, Character = (uint)end },
            },
            NewText = string.Empty,
        };
    }
}
