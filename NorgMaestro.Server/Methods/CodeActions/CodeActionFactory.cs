using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods.CodeActions;

internal class CodeActionFactory
{
    private readonly ICodeActionProvider[] _providers;

    public CodeActionFactory()
    {
        _providers =
        [
            new CreateMissingNoteActionProvider(),
            new CreateMissingNoteAndOpenActionProvider(),
            new FixBrokenLinkCandidateActionProvider(),
            new RenameLinkLabelToTitleActionProvider(),
            new ConvertTokenToCanonicalLinkActionProvider(),
            new CreateBacklinkSectionActionProvider(),
            new ExtractSelectionToNoteActionProvider(),
            new MoveNoteToWorkspaceActionProvider(),
            new NormalizeToWorkspaceAliasActionProvider(),
            new CreateNoteFromLinkTextActionProvider(),
        ];
    }

    public CodeAction[] Build(CodeActionContext context)
    {
        List<CodeAction> actions = [];
        foreach (ICodeActionProvider provider in _providers)
        {
            if (provider.CanHandle(context) is false)
            {
                continue;
            }

            actions.AddRange(provider.Build(context));
        }

        return [.. actions];
    }
}
