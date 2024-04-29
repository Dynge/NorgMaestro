using CeorgLsp.Parser;

namespace CeorgLsp
{
    public class LanguageServerState
    {
        public Dictionary<Uri, Document> Documents { get; init; } = [];
        private readonly object _updateDocLock = new();

        public LanguageServerState()
        {
            string[] notes = Directory.GetFiles("/home/michael/notes/");
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
