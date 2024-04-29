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

    public class NorgParser
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
                bool foundMetadata = true;
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
    }
}
