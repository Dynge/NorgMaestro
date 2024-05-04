using System.Text.RegularExpressions;
using NorgMaestro.Rpc;

namespace NorgMaestro.Parser
{
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

    public partial class NorgParser
    {
        public static NeorgMetadata GetMetadata(Uri fileUri)
        {
            NeorgMetadata metadata = new() { FileUri = fileUri };

            using (
                FileStream fs = File.Open(
                    fileUri.LocalPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                )
            )
            {
                using StreamReader streamReader = new(fs, true);

                string? line = streamReader.ReadLine()?.Trim();
                bool foundMetadata = false;
                bool insideMetadata = false;
                uint lineNr = 0;
                while (line is not null)
                {
                    insideMetadata = line switch
                    {
                        "@document.meta" => true,
                        "@end" => false,
                        _ => insideMetadata
                    };
                    if (insideMetadata is false)
                    {
                        if (foundMetadata is true)
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    foundMetadata = true;

                    switch (line)
                    {
                        case string when NorgMetaTitle().Matches(line).Count > 0:
                            Match match = NorgMetaTitle().Matches(line).First();
                            uint matchEnd = (uint)(match.Index + match.Length);
                            metadata = metadata with
                            {
                                Title = new()
                                {
                                    Name = line[(int)matchEnd..],
                                    Range = new()
                                    {
                                        Start = new() { Line = lineNr, Character = matchEnd },
                                        End = new() { Line = lineNr, Character = (uint)line.Length }
                                    }
                                }
                            };
                            break;
                        case string when NorgMetaAuthors().Matches(line).Count > 0:
                            match = NorgMetaAuthors().Matches(line).First();
                            matchEnd = (uint)(match.Index + match.Length);
                            metadata = metadata with
                            {
                                Title = new()
                                {
                                    Name = line[(int)matchEnd..],
                                    Range = new()
                                    {
                                        Start = new() { Line = lineNr, Character = matchEnd },
                                        End = new() { Line = lineNr, Character = (uint)line.Length }
                                    }
                                }
                            };
                            break;

                        case string when NorgMetaCategories().Matches(line).Count > 0:
                            List<MetaField> categories = [];
                            match = NorgMetaCategories().Matches(line).First();
                            matchEnd = (uint)(match.Index + match.Length);
                            if (line[(int)matchEnd] == '[')
                            {
                                matchEnd++;
                            }

                            line = line[(int)matchEnd..];
                            uint categoryStart = matchEnd;
                            uint categoryEnd = (uint)line.Length;

                            categories.Add(
                                new()
                                {
                                    Name = line,
                                    Range = new()
                                    {
                                        Start = new() { Line = lineNr, Character = categoryStart },
                                        End = new() { Line = lineNr, Character = categoryEnd },
                                    }
                                }
                            );
                            line = streamReader.ReadLine();
                            lineNr++;

                            while (line is not null && !line.EndsWith(']'))
                            {
                                categoryStart = (uint)line.TakeWhile(char.IsWhiteSpace).Count();
                                categoryEnd = (uint)line.Length;
                                if (line.Trim().Length is not 0)
                                {
                                    categories.Add(
                                        new()
                                        {
                                            Name = line,
                                            Range = new()
                                            {
                                                Start = new()
                                                {
                                                    Line = lineNr,
                                                    Character = categoryStart
                                                },
                                                End = new()
                                                {
                                                    Line = lineNr,
                                                    Character = categoryEnd
                                                },
                                            }
                                        }
                                    );
                                }
                                line = streamReader.ReadLine();
                                lineNr++;
                            }
                            break;
                        case string when NorgMetaCreated().Matches(line).Count > 0:
                            match = NorgMetaCreated().Matches(line).First();
                            matchEnd = (uint)(match.Index + match.Length);
                            metadata = metadata with
                            {
                                Title = new()
                                {
                                    Name = line[(int)matchEnd..],
                                    Range = new()
                                    {
                                        Start = new() { Line = lineNr, Character = matchEnd },
                                        End = new() { Line = lineNr, Character = (uint)line.Length }
                                    }
                                }
                            };
                            break;
                        case string when NorgMetaUpdated().Matches(line).Count > 0:
                            match = NorgMetaUpdated().Matches(line).First();
                            matchEnd = (uint)(match.Index + match.Length);
                            metadata = metadata with
                            {
                                Title = new()
                                {
                                    Name = line[(int)matchEnd..],
                                    Range = new()
                                    {
                                        Start = new() { Line = lineNr, Character = matchEnd },
                                        End = new() { Line = lineNr, Character = (uint)line.Length }
                                    }
                                }
                            };
                            break;
                        case string when NorgMetaVersion().Matches(line).Count > 0:
                            match = NorgMetaVersion().Matches(line).First();
                            matchEnd = (uint)(match.Index + match.Length);
                            metadata = metadata with
                            {
                                Title = new()
                                {
                                    Name = line[(int)matchEnd..],
                                    Range = new()
                                    {
                                        Start = new() { Line = lineNr, Character = matchEnd },
                                        End = new() { Line = lineNr, Character = (uint)line.Length }
                                    }
                                }
                            };
                            break;

                        default:
                            break;
                    }

                    line = streamReader.ReadLine();
                    lineNr++;
                }
                if (insideMetadata is true)
                {
                    // Malformed metadata - should have an end statement.
                    return new() { FileUri = metadata.FileUri };
                }
            }
            return metadata;
        }

        public static Dictionary<Uri, HashSet<Location>> GetReferences(
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
            Dictionary<Uri, HashSet<Location>> references = [];
            uint lineNumber = 0;
            foreach (string line in content)
            {
                Regex regex = NorgFileLinkRegex();
                MatchCollection matches = regex.Matches(line);
                foreach (Match match in matches.AsEnumerable())
                {
                    string path = Path.Join(match.Groups["File"].Value + ".norg");
                    Uri uriMatch = path.StartsWith('/') switch
                    {
                        true => new("file://" + path),
                        false
                            => new(
                                "file://"
                                    + Path.Join(
                                        Directory.GetParent(fileUri.AbsolutePath)!.FullName,
                                        path
                                    )
                            )
                    };

                    Location refLocation =
                        new()
                        {
                            Uri = fileUri.AbsoluteUri,
                            Range = new()
                            {
                                Start = new() { Line = lineNumber, Character = (uint)match.Index },
                                End = new()
                                {
                                    Line = lineNumber,
                                    Character = (uint)(match.Index + match.Length)
                                }
                            }
                        };
                    references[uriMatch] = references.TryGetValue(
                        uriMatch,
                        out HashSet<Location>? value
                    )
                        ? ([.. value, refLocation])
                        : ([refLocation]);
                }
                lineNumber++;
            }
            return references;
        }

        [GeneratedRegex(@"{:(?<File>(\w|[-./~])+):}\[(?<LinkText>.+)\]")]
        public static partial Regex NorgFileLinkRegex();

        [GeneratedRegex(@"^title: ")]
        private static partial Regex NorgMetaTitle();

        [GeneratedRegex(@"^description: ")]
        private static partial Regex NorgMetaDescription();

        [GeneratedRegex(@"^authors: ")]
        private static partial Regex NorgMetaAuthors();

        [GeneratedRegex(@"^categories: ")]
        private static partial Regex NorgMetaCategories();

        [GeneratedRegex(@"^created: ")]
        private static partial Regex NorgMetaCreated();

        [GeneratedRegex(@"^updated: ")]
        private static partial Regex NorgMetaUpdated();

        [GeneratedRegex(@"^version: ")]
        private static partial Regex NorgMetaVersion();
    }
}
