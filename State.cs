using CeorgLsp.Parser;
using CeorgLsp.Rpc;

namespace CeorgLsp
{
    public class LanguageServerState
    {
        public Dictionary<Uri, Document> Documents { get; init; } = [];
        public Dictionary<Uri, HashSet<Location>> References { get; init; } = [];
        private readonly object _updateDocLock = new();

        public void Initialize(Uri rootUri)
        {
            string[] notes = Directory.GetFiles(rootUri.LocalPath);
            foreach (string note in notes)
            {
                if (Path.GetExtension(note) is not ".norg")
                {
                    continue;
                }
                _ = UpdateDocument(new Uri(note));
            }
        }

        public Document? UpdateDocument(Uri fileUri)
        {
            NeorgMetadata metadata = NorgParser.GetMetadata(fileUri);
            string[] content = File.ReadAllLines(fileUri.LocalPath);
            Dictionary<Uri, HashSet<Location>> references = NorgParser.GetReferences(
                fileUri,
                content
            );
            Document doc =
                new()
                {
                    Uri = fileUri,
                    Metadata = metadata,
                    Content = content
                };

            lock (_updateDocLock)
            {
                Documents[fileUri] = doc;
                foreach (KeyValuePair<Uri, HashSet<Location>> refKvp in references)
                {
                    References[refKvp.Key] = References.TryGetValue(
                        refKvp.Key,
                        out HashSet<Location>? value
                    )
                        ? ([.. value, .. refKvp.Value])
                        : ([.. refKvp.Value]);
                }
            }

            return doc;
        }
    }

    public record Document
    {
        public required Uri Uri { get; init; }
        public required NeorgMetadata Metadata { get; init; }
        public required string[] Content { get; init; }
    }
}