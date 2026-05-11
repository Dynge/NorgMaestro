using System.Text.RegularExpressions;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Parser;

public record NeorgMetadata
{
    public required Uri FileUri { get; init; }
    public MetaField? Title { get; init; }
    public MetaField? Description { get; init; }
    public MetaField? Authors { get; init; }
    public MetaField[] Categories { get; init; } = [];
    public MetaField? Created { get; init; }
    public MetaField? Updated { get; init; }
    public MetaField? Version { get; init; }
}

public record MetaField
{
    public required string Name { get; init; }
    public required TextRange Range { get; init; }
}

internal interface INorgParser
{
    public static abstract Task<NeorgMetadata> GetMetadata(Uri fileUri);
    public static abstract Dictionary<Uri, HashSet<ReferenceLocation>> GetReferences(
        Uri fileUri,
        string[] content
    );
    public static abstract NorgLink? ParseLink(Uri fileUri, Position position, string line);
    public static abstract NorgLink[] ParseLinks(Uri fileUri, string[] content);
}

internal partial class NorgParser : INorgParser
{
    public static async Task<NeorgMetadata> GetMetadata(Uri fileUri)
    {
        string[] content = File.ReadAllLines(fileUri.LocalPath);
        return await GetMetadata(fileUri, content);
    }

    public static Task<NeorgMetadata> GetMetadata(Uri fileUri, string[] content)
    {
        NeorgMetadata metadata = new() { FileUri = fileUri };

        bool? insideMetadata = null;
        for (int i = 0; i < content.Length; i++)
        {
            string line = content[i].Trim();
            uint lineNr = (uint)i;

            insideMetadata = line switch
            {
                "@document.meta" => true,
                "@end" => false,
                _ => insideMetadata,
            };

            if (insideMetadata is false)
            {
                break;
            }

            switch (line)
            {
                case string when NorgMetaTitle().Matches(line).Count > 0:
                    metadata = metadata with { Title = ParseScalarMetaField(line, lineNr, NorgMetaTitle()) };
                    break;
                case string when NorgMetaDescription().Matches(line).Count > 0:
                    metadata = metadata with
                    {
                        Description = ParseScalarMetaField(line, lineNr, NorgMetaDescription())
                    };
                    break;
                case string when NorgMetaAuthors().Matches(line).Count > 0:
                    metadata = metadata with
                    {
                        Authors = ParseScalarMetaField(line, lineNr, NorgMetaAuthors())
                    };
                    break;

                case string when NorgMetaCategories().Matches(line).Count > 0:
                    ParsedCategories parsedCategories = ParseCategories(content, i, line, lineNr);
                    metadata = metadata with { Categories = [.. parsedCategories.Categories] };
                    i = parsedCategories.NextLineIndex;
                    break;
                case string when NorgMetaCreated().Matches(line).Count > 0:
                    metadata = metadata with
                    {
                        Created = ParseScalarMetaField(line, lineNr, NorgMetaCreated())
                    };
                    break;
                case string when NorgMetaUpdated().Matches(line).Count > 0:
                    metadata = metadata with
                    {
                        Updated = ParseScalarMetaField(line, lineNr, NorgMetaUpdated())
                    };
                    break;
                case string when NorgMetaVersion().Matches(line).Count > 0:
                    metadata = metadata with
                    {
                        Version = ParseScalarMetaField(line, lineNr, NorgMetaVersion())
                    };
                    break;

                default:
                    break;
            }
        }

        if (insideMetadata is true)
        {
            return Task.FromResult(new NeorgMetadata() { FileUri = metadata.FileUri });
        }

        return Task.FromResult(metadata);
    }

    public static Dictionary<Uri, HashSet<ReferenceLocation>> GetReferences(
        Uri fileUri,
        string[] content
    )
    {
        // - `:path/to/norg/file:` relative to the file which contains this link
        // - `:/path/from/root:` absolute w.r.t. the entire filesystem
        // - `:~/path/from/user/home:` relative to the user's home directory (e.g. `/home/user` on Linux machines)
        // - `:../path/to/norg/file:` these paths also understand `../`

        // TODO: Handle relative to workspaces
        // - `:$/path/from/current/workspace:` relative to current workspace root
        // - `:$gtd/path/in/gtd/workspace:` relative to the root of the workspace called `gtd`.
        Dictionary<Uri, HashSet<ReferenceLocation>> references = [];
        foreach (NorgLink link in ParseLinks(fileUri, content))
        {
            Uri linkUri = link.GetFileLinkUri();
            string line = content[(int)link.AbsoluteRange.Start.Line];
            ReferenceLocation refLocation = new()
            {
                Line = line,
                Location = new() { Uri = fileUri.AbsoluteUri, Range = link.AbsoluteRange },
            };

            if (references.TryGetValue(linkUri, out HashSet<ReferenceLocation>? existing))
            {
                _ = existing.Add(refLocation);
                continue;
            }

            references[linkUri] = [refLocation];
        }
        return references;
    }

    public static NorgLink[] ParseLinks(Uri fileUri, string[] content)
    {
        List<NorgLink> links = [];
        for (int i = 0; i < content.Length; i++)
        {
            links.AddRange(ParseLinks(fileUri, (uint)i, content[i]));
        }
        return [.. links];
    }

    private static NorgLink[] ParseLinks(Uri fileUri, uint lineNr, string line)
    {
        MatchCollection matches = NorgFileLinkRegex().Matches(line);
        return [.. matches.Select(m => NorgLink.From(fileUri, lineNr, m))];
    }

    public static NorgLink? ParseLink(Uri fileUri, Position position, string line)
    {
        Match? match = NorgFileLinkRegex()
            .Matches(line)
            .FirstOrDefault(m =>
                m.Index <= position.Character && m.Index + m.Length >= position.Character
            );
        return match is null ? null : NorgLink.From(fileUri, position.Line, match);
    }

    [GeneratedRegex(@"{:(?<File>[^:\[\]\{\}]+):}\[(?<LinkText>.+)\]")]
    private static partial Regex NorgFileLinkRegex();

    [GeneratedRegex(@"^title: ?")]
    private static partial Regex NorgMetaTitle();

    [GeneratedRegex(@"^description: ?")]
    private static partial Regex NorgMetaDescription();

    [GeneratedRegex(@"^authors: ?")]
    private static partial Regex NorgMetaAuthors();

    [GeneratedRegex(@"^categories: ?")]
    private static partial Regex NorgMetaCategories();

    [GeneratedRegex(@"^created: ?")]
    private static partial Regex NorgMetaCreated();

    [GeneratedRegex(@"^updated: ?")]
    private static partial Regex NorgMetaUpdated();

    [GeneratedRegex(@"^version: ?")]
    private static partial Regex NorgMetaVersion();

    private static MetaField ParseScalarMetaField(string line, uint lineNr, Regex regex)
    {
        Match match = regex.Matches(line).First();
        uint matchEnd = (uint)(match.Index + match.Length);
        return new()
        {
            Name = line[(int)matchEnd..],
            Range = new()
            {
                Start = new() { Line = lineNr, Character = matchEnd },
                End = new() { Line = lineNr, Character = (uint)line.Length },
            },
        };
    }

    private static ParsedCategories ParseCategories(
        IReadOnlyList<string> content,
        int currentLineIndex,
        string line,
        uint lineNr
    )
    {
        List<MetaField> categories = [];
        Match match = NorgMetaCategories().Matches(line).First();
        uint matchEnd = (uint)(match.Index + match.Length);
        if ((int)matchEnd < line.Length && line[(int)matchEnd] == '[')
        {
            matchEnd++;
        }

        string firstCategoryLine = line[(int)matchEnd..];
        uint categoryStart = matchEnd;
        uint categoryEnd = categoryStart + (uint)line.Length;
        if (firstCategoryLine.Trim().Length is not 0)
        {
            categories.Add(
                new()
                {
                    Name = firstCategoryLine,
                    Range = new()
                    {
                        Start = new() { Line = lineNr, Character = categoryStart },
                        End = new() { Line = lineNr, Character = categoryEnd },
                    },
                }
            );
        }

        int i = currentLineIndex;
        i++;
        while (i < content.Count)
        {
            string categoryLine = content[i];
            if (categoryLine.EndsWith(']'))
            {
                break;
            }

            lineNr = (uint)i;
            categoryStart = (uint)categoryLine.TakeWhile(char.IsWhiteSpace).Count();
            categoryEnd = (uint)categoryLine.Length;
            if (categoryLine.Trim().Length is not 0)
            {
                categories.Add(
                    new()
                    {
                        Name = categoryLine,
                        Range = new()
                        {
                            Start = new()
                            {
                                Line = lineNr,
                                Character = categoryStart,
                            },
                            End = new() { Line = lineNr, Character = categoryEnd },
                        },
                    }
                );
            }

            i++;
        }

        return new(categories, i);
    }

    private readonly record struct ParsedCategories(List<MetaField> Categories, int NextLineIndex);
}

internal record NorgLink
{
    public required Uri NorgFile { get; init; }
    public required string File { get; init; }
    public required string LinkText { get; init; }
    public required TextRange FileRange { get; init; }
    public required TextRange LinkTextRange { get; init; }
    public required TextRange AbsoluteRange { get; init; }

    public Uri GetFileLinkUri()
    {
        return File.StartsWith('/') switch
        {
            true => new("file://" + File + ".norg"),
            false => new(
                "file://"
                    + Path.Join(Directory.GetParent(NorgFile.AbsolutePath)!.FullName, File)
                    + ".norg"
            ),
        };
    }

    public static NorgLink From(Uri fileUri, uint lineNr, Match match)
    {
        Group fileGroup = match.Groups["File"];
        Group linkTextGroup = match.Groups["LinkText"];
        return new()
        {
            NorgFile = fileUri,
            File = fileGroup.Value,
            FileRange = new()
            {
                Start = new() { Line = lineNr, Character = (uint)fileGroup.Index },
                End = new()
                {
                    Line = lineNr,
                    Character = (uint)(fileGroup.Index + fileGroup.Length),
                },
            },

            LinkText = linkTextGroup.Value,
            LinkTextRange = new()
            {
                Start = new() { Line = lineNr, Character = (uint)linkTextGroup.Index },
                End = new()
                {
                    Line = lineNr,
                    Character = (uint)(linkTextGroup.Index + linkTextGroup.Length),
                },
            },
            AbsoluteRange = new()
            {
                Start = new() { Line = lineNr, Character = (uint)match.Index },
                End = new() { Line = lineNr, Character = (uint)(match.Index + match.Length) },
            },
        };
    }
}
