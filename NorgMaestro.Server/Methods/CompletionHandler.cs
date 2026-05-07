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

        int lineIndex = (int)completionRequestParams.Postion.Line;
        if (lineIndex < 0 || lineIndex >= doc.Content.Length)
        {
            return completionRequestParams.CompletionContext?.TriggerCharacter == '{';
        }

        string line = doc.Content[lineIndex];
        int cursor = (int)completionRequestParams.Postion.Character;
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

    private HashSet<CompletionItem> GetCategoryCompletions(
        CompletionRequestParams completionRequestParams
    )
    {
        HashSet<CompletionItem> uniqueCategories = [];
        foreach (Document doc in _state.Documents.Values)
        {
            if (doc.Uri.Equals(completionRequestParams.TextDocument.Uri))
            {
                continue;
            }
            IEnumerable<CompletionItem> completionItems = doc.Metadata.Categories.Select(
                c => new CompletionItem() { Label = c.Name }
            );
            uniqueCategories.UnionWith(completionItems);
        }
        return uniqueCategories;
    }

    private IEnumerable<CompletionItem> GetLinkCompletions(
        CompletionRequestParams completionRequestParams
    )
    {
        TextEdit editTemplate = GetLinkEditRange(completionRequestParams);
        IEnumerable<CompletionItem> completionItems = _state
            .Documents.Values.Where(d => !d.Uri.Equals(completionRequestParams.TextDocument.Uri))
            .Select(d => new CompletionItem()
            {
                Label = d.Metadata.Title?.Name ?? "[No Title]",
                Kind = CompletionKind.File,
                TextEdit = new()
                {
                    Range = editTemplate.Range,
                    NewText =
                        $"{{:{Path.GetFileNameWithoutExtension(d.Uri.AbsolutePath)}:}}[{d.Metadata.Title?.Name ?? ""}]",
                },
            });
        return completionItems;
    }

    private TextEdit GetLinkEditRange(CompletionRequestParams completionRequestParams)
    {
        if (_state.Documents.TryGetValue(completionRequestParams.TextDocument.Uri, out Document? doc) is false)
        {
            return new()
            {
                Range = new()
                {
                    Start = new()
                    {
                        Character = completionRequestParams.Postion.Character - 1,
                        Line = completionRequestParams.Postion.Line,
                    },
                    End = new()
                    {
                        Line = completionRequestParams.Postion.Line,
                        Character = completionRequestParams.Postion.Character,
                    },
                },
                NewText = string.Empty,
            };
        }

        int lineIndex = (int)completionRequestParams.Postion.Line;
        if (lineIndex < 0 || lineIndex >= doc.Content.Length)
        {
            return new()
            {
                Range = new()
                {
                    Start = new()
                    {
                        Character = completionRequestParams.Postion.Character - 1,
                        Line = completionRequestParams.Postion.Line,
                    },
                    End = new()
                    {
                        Line = completionRequestParams.Postion.Line,
                        Character = completionRequestParams.Postion.Character,
                    },
                },
                NewText = string.Empty,
            };
        }

        string line = doc.Content[lineIndex];
        int cursor = Math.Clamp((int)completionRequestParams.Postion.Character, 0, line.Length);
        int openBrace = line.LastIndexOf('{', cursor == 0 ? 0 : cursor - 1);
        int closeBrace = openBrace >= 0 ? line.IndexOf('}', openBrace) : -1;
        if (openBrace < 0 || closeBrace < 0)
        {
            return new()
            {
                Range = new()
                {
                    Start = new()
                    {
                        Character = completionRequestParams.Postion.Character - 1,
                        Line = completionRequestParams.Postion.Line,
                    },
                    End = new()
                    {
                        Line = completionRequestParams.Postion.Line,
                        Character = completionRequestParams.Postion.Character,
                    },
                },
                NewText = string.Empty,
            };
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
                Start = new() { Line = completionRequestParams.Postion.Line, Character = (uint)openBrace },
                End = new() { Line = completionRequestParams.Postion.Line, Character = (uint)end },
            },
            NewText = string.Empty,
        };
    }
}
