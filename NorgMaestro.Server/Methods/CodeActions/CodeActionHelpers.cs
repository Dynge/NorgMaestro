using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods.CodeActions;

internal static class CodeActionHelpers
{
    internal const string UnresolvedPrefix = "Unresolved note link: ";

    internal static IEnumerable<(Diagnostic Diagnostic, string TargetPath)> UnresolvedDiagnostics(
        CodeActionContext context
    )
    {
        foreach (Diagnostic diagnostic in context.Request.Params.Context.Diagnostics)
        {
            if (diagnostic.Message.StartsWith(UnresolvedPrefix) is false)
            {
                continue;
            }

            yield return (diagnostic, diagnostic.Message[UnresolvedPrefix.Length..]);
        }
    }

    internal static string CanonicalLink(string idOrPath, string title)
    {
        return $"{{:{idOrPath}:}}[{title}]";
    }

    internal static bool IsTokenChar(char c)
    {
        return char.IsLetterOrDigit(c) || c is '_' or '-' or '/' or '.';
    }

    internal static string ExtractTokenAtCursor(string line, int cursor)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return string.Empty;
        }

        int index = Math.Clamp(cursor, 0, Math.Max(0, line.Length - 1));
        int start = index;
        while (start > 0 && IsTokenChar(line[start - 1]))
        {
            start--;
        }

        int end = index;
        while (end < line.Length && IsTokenChar(line[end]))
        {
            end++;
        }

        return start == end ? string.Empty : line[start..end];
    }

    internal static TextRange GetTokenRange(string line, Position cursor)
    {
        int index = Math.Clamp((int)cursor.Character, 0, Math.Max(0, line.Length - 1));
        int start = index;
        while (start > 0 && IsTokenChar(line[start - 1]))
        {
            start--;
        }

        int end = index;
        while (end < line.Length && IsTokenChar(line[end]))
        {
            end++;
        }

        return new()
        {
            Start = new() { Line = cursor.Line, Character = (uint)start },
            End = new() { Line = cursor.Line, Character = (uint)end },
        };
    }

    internal static bool IsWithinRange(Position pos, TextRange range)
    {
        if (pos.Line < range.Start.Line || pos.Line > range.End.Line)
        {
            return false;
        }

        if (pos.Line == range.Start.Line && pos.Character < range.Start.Character)
        {
            return false;
        }

        if (pos.Line == range.End.Line && pos.Character > range.End.Character)
        {
            return false;
        }

        return true;
    }
}
