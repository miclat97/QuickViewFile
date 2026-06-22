using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

public static class FileTextExtractor
{
    public static string GetCleanTextFast(string filePath)
    {
        // read empty char instead of unreadable utf8 char (in moment of decoding and reading file - super fast)
        var cleanUtf8 = Encoding.GetEncoding(
            "utf-8",
            new EncoderReplacementFallback(""),
            new DecoderReplacementFallback("")
        );

        string rawText = File.ReadAllText(filePath, cleanUtf8);

        return FilterUnwantedCharacters(rawText);
    }

    private static string FilterUnwantedCharacters(string input)
    {
        ReadOnlySpan<char> span = input.AsSpan();

        // count chars that are printable (or new line or tab) to allocate string of correct size
        int validCount = 0;
        foreach (char c in span)
        {
            if (IsPrintable(c)) validCount++;
        }

        // if file hasn't got any readable characters = return empty string just now
        if (validCount == input.Length) return input;
        if (validCount == 0) return string.Empty;

        // string.Create in modern .NET is much faster even than StringBuilder
        return string.Create(validCount, input, (buffer, state) =>
        {
            int index = 0;
            foreach (char c in state.AsSpan())
            {
                if (IsPrintable(c))
                {
                    buffer[index++] = c;
                }
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPrintable(char c)
    {
        // we want to keep: printable characters (ASCII >= 32), new line, carriage return, and tab
        return c >= 32 || c == '\n' || c == '\r' || c == '\t';
    }
}