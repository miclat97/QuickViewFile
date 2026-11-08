using System;
using System.Globalization;
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
        // Keep structural whitespace (\n, \r, \t) and safe printable display characters
        return c == '\n' || c == '\r' || c == '\t' || IsSafePrintableChar(c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char SanitizeForDisplay(char c)
    {
        return IsSafePrintableChar(c) ? c : ' ';
    }

    /// <summary>
    /// Returns true if <paramref name="c"/> is a safe, single-code-point printable character that will not
    /// destabilize WPF's TextFormatter / LineServices (which triggers MS.Internal.Invariant.FailFast in
    /// TextBoxView.ArrangeVisuals when viewing binary files decoded as UTF-8 or Latin1).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSafePrintableChar(char c)
    {
        // Fast path: standard printable ASCII (0x20 ' ' .. 0x7E '~')
        if (c >= 0x20 && c <= 0x7E)
            return true;

        // Reject C0 controls (< 0x20), DEL + C1 controls + NBSP (0x7F..0xA0, including 0x85 NEL which
        // LineServices treats as a hard line break inside a TextRun), and Soft Hyphen (0xAD).
        if (c <= 0xA0 || c == 0xAD)
            return false;

        // Reject UTF-16 surrogates (0xD800..0xDFFF) and Private Use Area (0xE000..0xF8FF).
        // Surrogates in random binary streams can be unpaired or split across wrap boundaries.
        if (c >= 0xD800 && c <= 0xF8FF)
            return false;

        // Reject Combining Diacritical Marks, Right-to-Left (Bidi) scripts, complex cluster-shaping scripts,
        // Bidi/Format controls, and Unicode Line/Paragraph separators (U+2028, U+2029) frequently produced
        // by random 2/3-byte UTF-8 sequences in binary files (*.exe, *.dll, etc.).
        if ((c >= 0x0300 && c <= 0x036F) ||
            (c >= 0x0483 && c <= 0x0489) ||
            (c >= 0x0590 && c <= 0x109F) ||
            (c >= 0x1700 && c <= 0x1DFF) ||
            (c >= 0x2000 && c <= 0x200F) ||
            (c >= 0x2028 && c <= 0x202F) ||
            (c >= 0x205F && c <= 0x206F) ||
            (c >= 0x20D0 && c <= 0x20FF) ||
            (c >= 0xA800 && c <= 0xABFF) ||
            (c >= 0xFB1D && c <= 0xFEFF) ||
            c >= 0xFFF0)
        {
            return false;
        }

        UnicodeCategory cat = char.GetUnicodeCategory(c);
        switch (cat)
        {
            case UnicodeCategory.UppercaseLetter:
            case UnicodeCategory.LowercaseLetter:
            case UnicodeCategory.TitlecaseLetter:
            case UnicodeCategory.ModifierLetter:
            case UnicodeCategory.OtherLetter:
            case UnicodeCategory.DecimalDigitNumber:
            case UnicodeCategory.LetterNumber:
            case UnicodeCategory.OtherNumber:
            case UnicodeCategory.ConnectorPunctuation:
            case UnicodeCategory.DashPunctuation:
            case UnicodeCategory.OpenPunctuation:
            case UnicodeCategory.ClosePunctuation:
            case UnicodeCategory.InitialQuotePunctuation:
            case UnicodeCategory.FinalQuotePunctuation:
            case UnicodeCategory.OtherPunctuation:
            case UnicodeCategory.MathSymbol:
            case UnicodeCategory.CurrencySymbol:
            case UnicodeCategory.ModifierSymbol:
            case UnicodeCategory.OtherSymbol:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Returns true if <paramref name="c"/> is an East Asian Wide / Fullwidth character that occupies
    /// roughly two monospace columns.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWideChar(char c)
    {
        return c >= 0x1100 && (
            c <= 0x115F ||
            c == 0x2329 || c == 0x232A ||
            (c >= 0x2E80 && c <= 0xA4CF && c != 0x303F) ||
            (c >= 0xAC00 && c <= 0xD7A3) ||
            (c >= 0xF900 && c <= 0xFAFF) ||
            (c >= 0xFE10 && c <= 0xFE6F) ||
            (c >= 0xFF00 && c <= 0xFF60) ||
            (c >= 0xFFE0 && c <= 0xFFE6));
    }
}