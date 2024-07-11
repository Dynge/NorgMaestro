using NorgMaestro.Rpc;

namespace NorgMaestro.Parser;

public static class FileUtil
{
    public static string[] ReadRange(Uri fileUri, TextRange range)
    {
        List<string> readLines = [];

        using (
            FileStream fs = File.Open(
                fileUri.AbsolutePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            )
        )
        {
            using StreamReader streamReader = new(fs, true);
            uint lineNr = 0;
            string? line = streamReader.ReadLine();
            while (line is not null && lineNr < (range.End.Line + 1))
            {
                if (lineNr < range.Start.Line)
                {
                    line = streamReader.ReadLine();
                    lineNr++;
                    continue;
                }

                int startIndex = (lineNr == range.Start.Line) switch
                {
                    true => (int)range.Start.Character,
                    false => 0
                };

                int endIndex = (lineNr == range.End.Line) switch
                {
                    true => (int)range.End.Character,
                    false => line.Length
                };

                if (startIndex == endIndex)
                {
                    readLines.Add(line);
                }
                else
                {
                    readLines.Add(line.Substring(startIndex, endIndex));
                }

                line = streamReader.ReadLine();
                lineNr++;
            }
        }
        return [.. readLines];
    }
}
