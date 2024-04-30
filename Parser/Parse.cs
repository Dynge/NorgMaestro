using System.Text.RegularExpressions;
using CeorgLsp.Rpc;

namespace CeorgLsp.Parser
{
    public record NeorgMetadata
    {
        public required Uri FileUri { get; init; }
        public string Title { get; init; } = "";
        public string Description { get; init; } = "";
        public string Authors { get; init; } = "";
        public string[] Categories { get; init; } = [];
        public string Created { get; init; } = "";
        public string Updated { get; init; } = "";
        public string Version { get; init; } = "";
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
                        case string _ when line.StartsWith("title: "):
                            metadata = metadata with { Title = line.Replace("title: ", "") };
                            break;
                        case string _ when line.StartsWith("authors: "):
                            metadata = metadata with { Authors = line.Replace("authors: ", "") };
                            break;
                        case string _ when line.StartsWith("categories: "):

                            List<string> categories = [];
                            while (line is not null && !line.EndsWith("]"))
                            {
                                string category = line.Replace("categories: [", "").Trim();
                                if (category.Trim().Length is not 0)
                                {
                                    categories.Add(category);
                                }
                                line = streamReader.ReadLine();
                            }
                            metadata = metadata with { Categories = [.. categories] };

                            break;
                        case string _ when line.StartsWith("created: "):
                            metadata = metadata with { Created = line.Replace("created: ", "") };
                            break;
                        case string _ when line.StartsWith("updated: "):
                            metadata = metadata with { Updated = line.Replace("updated: ", "") };
                            break;
                        case string _ when line.StartsWith("version: "):
                            metadata = metadata with { Version = line.Replace("version: ", "") };
                            break;

                        default:
                            break;
                    }

                    line = streamReader.ReadLine();
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
                    string path = Path.Join(match.Groups[1].Value + ".norg");
                    Uri uriMatch = path.StartsWith("/") switch
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

        [GeneratedRegex(@"{:((\w|[-./~])+):}")]
        public static partial Regex NorgFileLinkRegex();
    }
}
