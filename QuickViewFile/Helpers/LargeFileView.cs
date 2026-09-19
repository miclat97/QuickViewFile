using System.Buffers;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using QuickViewFile.Models;

namespace QuickViewFile.Helpers
{
    /// <summary>
    /// Renders and navigates arbitrarily large files without loading them fully into memory.
    ///
    /// The file is modelled as a sequence of "visual lines" whose boundaries are a pure function
    /// of the absolute byte offset: a line breaks on a newline (0x0A) OR every <see cref="MaxLineBytes"/>
    /// bytes on a fixed grid anchored to offset 0, whichever comes first. Because the boundaries do not
    /// depend on the direction of travel, scrolling up and scrolling down always land on exactly the same
    /// line starts, so nothing is ever skipped or shown twice. The left gutter shows the hex offset of the
    /// first byte of each rendered line.
    /// </summary>
    public sealed class LargeFileView
    {
        // A visual line never exceeds this many bytes. Keeps binary data (no newlines) scrolling smoothly
        // and makes the hex gutter increment on a regular grid.
        private const int MaxLineBytes = 1024;
        private const int ReadWindowBytes = 256 * 1024;
        private const int BackWindowBytes = 256 * 1024;
        private const int RenderBufferLines = 4;
        private const byte LF = 0x0A;

        private readonly TextBox _content;
        private readonly TextBox _gutter;
        private readonly ScrollBar _scrollBar;

        private string? _filePath;
        private long _fileSize;
        private Encoding _encoding = Encoding.Latin1;
        private int _offsetHexDigits = 8;

        private FileStream? _stream;
        private byte[] _forwardBuf = System.Array.Empty<byte>();

        private long _topOffset;
        private long _afterLast;
        private long _lastLineStart;
        private readonly List<long> _renderedStarts = new();
        private readonly List<int> _renderedCharStarts = new();
        private bool _active;
        private bool _wordWrap;
        private bool _rendering;
        private double _charWidthCache;
        private double _charWidthFontSize = -1;
        private bool _fromScrollBar;
        private long _maxTopCache;
        private long _maxTopFileSize = -1;
        private int _maxTopVisible = -1;
        private int _maxTopPerRow = -1;
        private bool _maxTopWrap;
        private bool _maxTopValid;

        private long _lastMatchOffset = -1;
        private string? _lastQuery;
        private System.Threading.CancellationTokenSource? _searchCts;

        /// <summary>Raised with a short human readable status about the current search (shown next to the search box).</summary>
        public event System.Action<string>? SearchStatusChanged;

        public bool IsActive => _active;

        public LargeFileView(TextBox content, TextBox gutter, ScrollBar scrollBar)
        {
            _content = content;
            _gutter = gutter;
            _scrollBar = scrollBar;
        }

        private static Encoding BuildEncoding(ConfigModel config)
        {
            string name = config.Utf8InsteadOfASCIITextPreview == 1 ? "utf-8" : "iso-8859-1";
            return Encoding.GetEncoding(name, new EncoderReplacementFallback("\uFFFD"), new DecoderReplacementFallback("\uFFFD"));
        }

        public void Activate(string filePath, long fileSize, ConfigModel config)
        {
            Deactivate();

            _filePath = filePath;
            _fileSize = System.Math.Max(0, fileSize);
            _encoding = BuildEncoding(config);
            _wordWrap = config.LargeFileWordWrap == 1;
            ApplyWrapMode();
            _offsetHexDigits = System.Math.Max(6, _fileSize <= 1 ? 1 : ((int)System.Math.Floor(System.Math.Log(_fileSize, 16)) + 1));
            _topOffset = 0;
            _afterLast = 0;
            _lastMatchOffset = -1;
            _lastQuery = null;

            try
            {
                _stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, ReadWindowBytes, FileOptions.RandomAccess);
            }
            catch
            {
                _stream = null;
            }

            _forwardBuf = new byte[ReadWindowBytes];
            _lastLineStart = _fileSize > 0 ? ComputeLineStart(_fileSize - 1) : 0;
            _maxTopValid = false;
            _active = true;

            Render(0, aligned: true);
        }

        public void Deactivate()
        {
            _active = false;
            _searchCts?.Cancel();
            _searchCts = null;
            _stream?.Dispose();
            _stream = null;
            _forwardBuf = System.Array.Empty<byte>();
            _renderedStarts.Clear();
            _renderedCharStarts.Clear();
            _filePath = null;

            // Hand wrapping back to the XAML style so edit mode keeps its configured behaviour.
            _content.ClearValue(TextBox.TextWrappingProperty);
            _content.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        private void ApplyWrapMode()
        {
            if (_wordWrap)
            {
                _content.TextWrapping = TextWrapping.Wrap;
                _content.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
            else
            {
                _content.TextWrapping = TextWrapping.NoWrap;
                _content.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }


        private int ReadAt(long offset, byte[] dest, int count)
        {
            if (_stream == null || offset < 0 || offset >= _fileSize || count <= 0)
                return 0;

            long avail = _fileSize - offset;
            int toRead = (int)System.Math.Min(count, avail);
            try
            {
                _stream.Seek(offset, SeekOrigin.Begin);
                int total = 0;
                while (total < toRead)
                {
                    int r = _stream.Read(dest, total, toRead - total);
                    if (r <= 0) break;
                    total += r;
                }
                return total;
            }
            catch
            {
                return 0;
            }
        }


        /// <summary>Start offset of the visual line that contains byte <paramref name="x"/>.</summary>
        private long ComputeLineStart(long x)
        {
            if (x <= 0) return 0;
            long gridFloor = (x / MaxLineBytes) * MaxLineBytes;
            int span = (int)(x - gridFloor); // bytes in [gridFloor, x)
            if (span <= 0) return gridFloor;

            byte[] tmp = ArrayPool<byte>.Shared.Rent(span);
            try
            {
                int n = ReadAt(gridFloor, tmp, span);
                for (int i = n - 1; i >= 0; i--)
                {
                    if (tmp[i] == LF)
                        return gridFloor + i + 1;
                }
                return gridFloor;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(tmp);
            }
        }

        /// <summary>Walks <paramref name="k"/> visual lines up from <paramref name="s"/> and returns the resulting line start.</summary>
        private long UpKLines(long s, int k)
        {
            if (s <= 0 || k <= 0) return 0;

            long readStart = System.Math.Max(0, s - BackWindowBytes);
            int len = (int)(s - readStart);
            byte[] back = ArrayPool<byte>.Shared.Rent(len);
            try
            {
                int n = ReadAt(readStart, back, len);
                long cur = s;
                for (int i = 0; i < k && cur > 0; i++)
                {
                    long x = cur - 1; // last byte of the previous line
                    long gridFloor = (x / MaxLineBytes) * MaxLineBytes;
                    long lo = System.Math.Max(gridFloor, readStart);
                    int fromIdx = (int)(lo - readStart);
                    int toIdx = (int)(x - readStart) - 1; // newline candidates are strictly before x
                    if (toIdx > n - 1) toIdx = n - 1;

                    long start = gridFloor;
                    for (int j = toIdx; j >= fromIdx; j--)
                    {
                        if (back[j] == LF)
                        {
                            start = readStart + j + 1;
                            break;
                        }
                    }
                    if (start >= cur) start = System.Math.Max(0, cur - 1); // guarantee progress
                    cur = start;
                }
                return cur;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(back);
            }
        }

        private long ComputeMaxTop(int visibleLines)
        {
            if (_fileSize <= 0) return 0;
            return UpKLines(_fileSize, System.Math.Max(1, visibleLines));
        }

        /// <summary>
        /// Start of the last page - the offset that, rendered downwards, exactly fills the viewport and ends at
        /// EOF. In wrap mode a logical line can occupy several display rows, so the walk back from EOF counts
        /// display rows rather than lines; otherwise the bottom of the file would sit a screen out of reach.
        /// Depends only on the file and the viewport geometry, so it is cached and recomputed on resize/zoom.
        /// </summary>
        private long MaxTop(int visible)
        {
            if (_fileSize <= 0) return 0;

            int perRow = _wordWrap ? CharsPerRow() : 0;
            if (_maxTopValid && _maxTopFileSize == _fileSize && _maxTopVisible == visible
                && _maxTopPerRow == perRow && _maxTopWrap == _wordWrap)
                return _maxTopCache;

            long result;
            if (!_wordWrap)
            {
                result = ComputeMaxTop(visible);
            }
            else
            {
                long readStart = System.Math.Max(0, _fileSize - BackWindowBytes);
                int len = (int)(_fileSize - readStart);
                byte[] back = ArrayPool<byte>.Shared.Rent(len);
                try
                {
                    int n = ReadAt(readStart, back, len);
                    long cur = _fileSize;
                    int rows = 0;
                    while (cur > 0 && rows < visible)
                    {
                        long x = cur - 1;
                        long gridFloor = (x / MaxLineBytes) * MaxLineBytes;
                        long lo = System.Math.Max(gridFloor, readStart);
                        int fromIdx = (int)(lo - readStart);
                        int toIdx = (int)(x - readStart) - 1;
                        if (toIdx > n - 1) toIdx = n - 1;

                        long start = gridFloor;
                        for (int j = toIdx; j >= fromIdx; j--)
                        {
                            if (back[j] == LF) { start = readStart + j + 1; break; }
                        }
                        if (start >= cur) start = System.Math.Max(0, cur - 1); // guarantee progress
                        if (start < readStart) { cur = start; break; }         // ran past the tail window

                        rows += System.Math.Max(1, (int)((cur - start + perRow - 1) / perRow));
                        cur = start;
                    }
                    result = cur;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(back);
                }
            }

            _maxTopCache = result;
            _maxTopFileSize = _fileSize;
            _maxTopVisible = visible;
            _maxTopPerRow = perRow;
            _maxTopWrap = _wordWrap;
            _maxTopValid = true;
            return result;
        }


        private double LineHeight()
        {
            double fs = _content.FontSize > 0 ? _content.FontSize : 13;
            var ff = _content.FontFamily;
            double ls = (ff != null && ff.LineSpacing > 0) ? ff.LineSpacing : 1.35;
            return System.Math.Max(1, fs * ls);
        }

        private int VisibleLineCount()
        {
            double vh = _content.ViewportHeight;
            if (double.IsNaN(vh) || vh <= 1) vh = _content.ActualHeight;
            if (double.IsNaN(vh) || vh <= 1) vh = 400;
            return System.Math.Max(1, (int)(vh / LineHeight()));
        }

        /// <summary>Width of one character of the (monospace) content font, measured once per font size.</summary>
        private double CharWidth()
        {
            double fs = _content.FontSize > 0 ? _content.FontSize : 13;
            if (_charWidthFontSize != fs || _charWidthCache <= 0)
            {
                double dpi = 1.0;
                try { dpi = System.Windows.Media.VisualTreeHelper.GetDpi(_content).PixelsPerDip; } catch { }
                double w = 0;
                try
                {
                    var ft = new System.Windows.Media.FormattedText("0",
                        System.Globalization.CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        new System.Windows.Media.Typeface(_content.FontFamily, _content.FontStyle, _content.FontWeight, _content.FontStretch),
                        fs, System.Windows.Media.Brushes.Black, dpi);
                    w = ft.Width;
                }
                catch { }
                _charWidthCache = w > 0 ? w : System.Math.Max(1, fs * 0.55);
                _charWidthFontSize = fs;
            }
            return _charWidthCache;
        }

        /// <summary>
        /// Upper bound on the number of characters worth rendering. In wrap mode a single logical line
        /// occupies several display rows, so a logical-line count alone would emit several screenfuls per
        /// scroll tick - which is what makes both the layout cost and the peak memory grow with how narrow
        /// the pane is. Bounding the payload by what can actually be displayed (with slack) keeps them flat.
        /// </summary>
        private int CharBudget(int visible)
        {
            if (!_wordWrap) return int.MaxValue;

            long budget = (long)MaxLineBytes + (long)visible * CharsPerRow() * 2; // one line + twice a screenful
            return (int)System.Math.Min(int.MaxValue, budget);
        }

        /// <summary>How many characters of the content font fit on one display row.</summary>
        private int CharsPerRow()
        {
            double usable = _content.ViewportWidth;
            if (double.IsNaN(usable) || usable <= 1) usable = _content.ActualWidth;
            if (double.IsNaN(usable) || usable <= 1) usable = 600;

            return System.Math.Max(1, (int)(usable / CharWidth()));
        }


        private string FormatOffset(long off) => off.ToString("X" + _offsetHexDigits.ToString());

        private string DecodeLine(byte[] buf, int start, int len)
        {
            if (len <= 0) return string.Empty;

            int charCount = _encoding.GetCharCount(buf, start, len);
            char[] chars = ArrayPool<char>.Shared.Rent(charCount);

            try
            {
                _encoding.GetChars(buf, start, len, chars, 0);

                var sb = new StringBuilder(charCount);
                int sinceSpace = 0;

                for (int i = 0; i < charCount; i++)
                {
                    char c = chars[i];
                    if (c == '\n' || c == '\r')
                    {
                        continue; // line breaks are structural, handled externally
                    }

                    if (c < 32 && c != '\t')
                    {
                        sb.Append('\uFFFD');
                        sinceSpace++;
                    }
                    else
                    {
                        sb.Append(c);
                        if (c == ' ' || c == '\t')
                        {
                            sinceSpace = 0;
                        }
                        else
                        {
                            sinceSpace++;
                        }
                    }

                    if (sinceSpace >= 500)
                    {
                        sb.Append('\n');
                        sinceSpace = 0;
                    }
                }

                return sb.ToString();
            }
            finally
            {
                ArrayPool<char>.Shared.Return(chars);
            }
        }

        /// <summary>Renders the file starting at (or line-aligned to) <paramref name="requested"/> byte offset.</summary>
        public void Render(long requested, bool aligned)
        {
            // Guard against re-entrancy and swallow the rare native failure in the WPF text/layout
            // layer (surfaces as a Windows "hard error") so a single hiccup never tears down the window.
            if (!_active || _rendering) return;
            _rendering = true;
            try
            {
                RenderCore(requested, aligned);
            }
            catch
            {
                // Intentionally ignored: keep the viewer alive; the next scroll/refresh re-renders.
            }
            finally
            {
                _rendering = false;
            }
        }

        private void RenderCore(long requested, bool aligned)
        {
            if (_fileSize <= 0)
            {
                _content.Text = string.Empty;
                _gutter.Text = string.Empty;
                _scrollBar.IsEnabled = false;
                return;
            }

            long top;
            if (requested <= 0) top = 0;
            else if (requested >= _fileSize) top = _lastLineStart;
            else top = aligned ? requested : ComputeLineStart(requested);

            int visible = VisibleLineCount();
            int toRender = visible + RenderBufferLines;
            int charBudget = CharBudget(visible);

            long maxTop = MaxTop(visible);
            if (top > maxTop) top = maxTop;
            if (top < 0) top = 0;
            _topOffset = top;

            // On the last page the budget must not apply: it would stop the render short of EOF, leaving the
            // end of the file unreachable from the scroll bar (only draggable into view by selecting text).
            bool lastPage = top >= maxTop;

            // Read only what the loop below can consume instead of the whole 256 KB window: at most one
            // line per rendered row, and in wrap mode no more than the character budget allows.
            long needBytes = (long)toRender * MaxLineBytes;
            if (_wordWrap) needBytes = System.Math.Min(needBytes, (long)charBudget + MaxLineBytes);
            if (lastPage) needBytes = _fileSize - top;
            int windowLen = ReadAt(top, _forwardBuf, (int)System.Math.Min(_forwardBuf.Length, needBytes));

            _renderedStarts.Clear();
            _renderedCharStarts.Clear();
            var sbContent = new StringBuilder();

            long pos = top;
            int produced = 0;
            while (produced < toRender && pos < _fileSize)
            {
                if (!lastPage && produced > 0 && sbContent.Length >= charBudget) break;

                int bufIdx = (int)(pos - top);
                if (bufIdx < 0 || bufIdx >= windowLen) break;

                long gridEnd = ((pos / MaxLineBytes) + 1) * MaxLineBytes;
                long searchEndAbs = System.Math.Min(gridEnd, _fileSize);
                int searchEndIdx = (int)System.Math.Min(windowLen, searchEndAbs - top);

                int nlIdx = -1;
                for (int i = bufIdx; i < searchEndIdx; i++)
                {
                    if (_forwardBuf[i] == LF) { nlIdx = i; break; }
                }

                long end = nlIdx >= 0 ? top + nlIdx + 1 : searchEndAbs;
                int segLen = (int)(end - pos);
                if (bufIdx + segLen > windowLen) segLen = windowLen - bufIdx;

                if (produced > 0) sbContent.Append('\n');
                _renderedCharStarts.Add(sbContent.Length);
                sbContent.Append(DecodeLine(_forwardBuf, bufIdx, segLen));

                _renderedStarts.Add(pos);
                produced++;
                pos = end;
            }

            _afterLast = pos;

            _content.Text = sbContent.ToString();
            _content.CaretIndex = 0;
            _content.ScrollToHome();

            // One layout-free gutter write per render, so the numbering can never lag or flicker.
            if (_wordWrap) UpdateGutterWrapped(sbContent.Length);
            else UpdateGutterFast();

            // Maximum is the real start of the last page, so dragging the thumb to the bottom lands exactly on
            // the screen that ends at EOF. ViewportSize, on the other hand, must depend only on the viewport
            // and the font - never on how many bytes this screen happened to consume. Deriving it from actual
            // content made the thumb resize on every scroll (binary data breaks lines on stray 0x0A bytes, so
            // bytes-per-screen swings wildly): visible as a rubber-banding thumb, and destabilising, because a
            // thumb that changes size mid-drag moves under the cursor and feeds a Scroll event back into here.
            long page = _wordWrap
                ? System.Math.Max(MaxLineBytes, (long)charBudget)
                : (long)toRender * MaxLineBytes;

            _scrollBar.ViewportSize = page;
            _scrollBar.Maximum = System.Math.Max(0, maxTop);
            _scrollBar.LargeChange = page;                    // clicking the track pages by ~one screen
            _scrollBar.SmallChange = MaxLineBytes;
            _scrollBar.IsEnabled = maxTop > 0;

            // Value is the one thing the drag itself owns: writing it back mid-drag would yank the thumb out
            // from under the cursor. Render() still clamps top to maxTop, so the last page stays reachable.
            if (!_fromScrollBar)
                _scrollBar.Value = System.Math.Min(_topOffset, _scrollBar.Maximum);
        }

        /// <summary>
        /// Layout-free numbering: one hex offset per logical line. Used as the immediate baseline in both
        /// modes (so the gutter never lags the text) and as the wrap-mode fallback when the layout has not
        /// been computed yet.
        /// </summary>
        private void UpdateGutterFast()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _renderedStarts.Count; i++)
            {
                if (i > 0) sb.Append('\n');
                sb.Append(FormatOffset(_renderedStarts[i]));
            }
            _gutter.Text = sb.ToString();
        }

        /// <summary>
        /// Wrap-mode numbering: one gutter row per display row, with the offset on the first row of each
        /// logical line and continuation rows left blank so the numbers stay aligned with the text.
        ///
        /// The row count is derived arithmetically from the character width rather than measured via
        /// <c>UpdateLayout</c>/<c>LineCount</c>. Measuring meant writing the gutter a second time, after the
        /// layout pass: the two writes had different row counts, so the numbers visibly jumped on every
        /// render, and the forced layout could itself trigger the next render and keep the cycle going while
        /// the view was otherwise idle.
        /// </summary>
        private void UpdateGutterWrapped(int totalChars)
        {
            int perRow = CharsPerRow();
            var sb = new StringBuilder();
            for (int i = 0; i < _renderedStarts.Count; i++)
            {
                int start = _renderedCharStarts[i];
                // Lines are joined with '\n', so the next line's start is one past this line's terminator.
                int endExclusive = (i + 1 < _renderedCharStarts.Count) ? _renderedCharStarts[i + 1] - 1 : totalChars;
                int len = System.Math.Max(0, endExclusive - start);
                int rows = System.Math.Max(1, (len + perRow - 1) / perRow);

                if (i > 0) sb.Append('\n');
                sb.Append(FormatOffset(_renderedStarts[i]));
                for (int r = 1; r < rows; r++) sb.Append('\n');
            }
            _gutter.Text = sb.ToString();
        }

        /// <summary>Re-renders at the current position (e.g. after the viewport is resized).</summary>
        public void Refresh()
        {
            if (_active) Render(_topOffset, aligned: true);
        }


        public void OnMouseWheel(int delta)
        {
            if (!_active || delta == 0) return;
            int lines = SystemParameters.WheelScrollLines;
            if (lines <= 0) lines = 3;
            int steps = System.Math.Max(1, (int)System.Math.Round(System.Math.Abs(delta) / 120.0 * lines));
            if (delta < 0) ScrollDownLines(steps);
            else ScrollUpLines(steps);
        }

        private void ScrollDownLines(int k)
        {
            if (!_active || _renderedStarts.Count == 0) return;
            long newTop = k < _renderedStarts.Count ? _renderedStarts[k] : _afterLast;
            if (newTop <= _topOffset) return;
            Render(newTop, aligned: true);
        }

        private void ScrollUpLines(int k)
        {
            if (!_active || _topOffset <= 0) return;
            long newTop = UpKLines(_topOffset, k);
            if (newTop >= _topOffset) newTop = System.Math.Max(0, _topOffset - 1);
            Render(newTop, aligned: true);
        }

        public void OnScrollBar(double value)
        {
            if (!_active) return;
            if (double.IsNaN(value) || double.IsInfinity(value)) return;

            _fromScrollBar = true;
            try
            {
                Render((long)value, aligned: false);
            }
            finally
            {
                _fromScrollBar = false;
            }
        }

        public bool OnKeyDown(KeyEventArgs e)
        {
            if (!_active) return false;
            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

            switch (e.Key)
            {
                case Key.Down: ScrollDownLines(1); return true;
                case Key.Up: ScrollUpLines(1); return true;
                case Key.PageDown: ScrollDownLines(System.Math.Max(1, VisibleLineCount() - 1)); return true;
                case Key.PageUp: ScrollUpLines(System.Math.Max(1, VisibleLineCount() - 1)); return true;
                case Key.Home when ctrl: Render(0, aligned: true); return true;
                case Key.End when ctrl: Render(_fileSize, aligned: false); return true;
                default: return false;
            }
        }


        private byte[]? EncodePattern(string query)
        {
            try
            {
                byte[] bytes = _encoding.GetBytes(query);
                return bytes.Length == 0 ? null : bytes;
            }
            catch
            {
                return null;
            }
        }

        private static byte ToLowerAscii(byte b) => (b >= (byte)'A' && b <= (byte)'Z') ? (byte)(b + 32) : b;

        private static bool MatchAt(byte[] hay, int idx, byte[] patLower)
        {
            for (int i = 0; i < patLower.Length; i++)
            {
                if (ToLowerAscii(hay[idx + i]) != patLower[i]) return false;
            }
            return true;
        }

        private FileStream? OpenSearchStream()
        {
            if (string.IsNullOrEmpty(_filePath)) return null;
            try
            {
                return new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 1 << 16, FileOptions.SequentialScan);
            }
            catch
            {
                return null;
            }
        }

        private async Task<long> ScanForwardAsync(byte[] patLower, long from, System.Threading.CancellationToken ct)
        {
            using FileStream? fs = OpenSearchStream();
            if (fs == null) return -1;

            int overlap = patLower.Length - 1;
            const int block = 1 << 20;
            byte[] buf = ArrayPool<byte>.Shared.Rent(block + overlap);
            try
            {
                long pos = System.Math.Max(0, from);
                while (pos < _fileSize && !ct.IsCancellationRequested)
                {
                    int want = (int)System.Math.Min(buf.Length, _fileSize - pos);
                    fs.Seek(pos, SeekOrigin.Begin);
                    int n = 0;
                    while (n < want)
                    {
                        int r = await fs.ReadAsync(buf.AsMemory(n, want - n), ct).ConfigureAwait(false);
                        if (r <= 0) break;
                        n += r;
                    }

                    int limit = n - patLower.Length + 1;
                    for (int i = 0; i < limit; i++)
                    {
                        if (MatchAt(buf, i, patLower)) return pos + i;
                    }

                    if (n < want || n <= overlap) break;
                    pos += n - overlap;
                }
                return -1;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf);
            }
        }

        private async Task<long> ScanBackwardAsync(byte[] patLower, long before, System.Threading.CancellationToken ct)
        {
            using FileStream? fs = OpenSearchStream();
            if (fs == null) return -1;

            int overlap = patLower.Length - 1;
            const int block = 1 << 20;
            byte[] buf = ArrayPool<byte>.Shared.Rent(block + overlap);
            try
            {
                long pos = System.Math.Min(before, _fileSize);
                while (pos > 0 && !ct.IsCancellationRequested)
                {
                    long start = System.Math.Max(0, pos - block);
                    long readEnd = System.Math.Min(_fileSize, pos + overlap);
                    int want = (int)(readEnd - start);
                    fs.Seek(start, SeekOrigin.Begin);
                    int n = 0;
                    while (n < want)
                    {
                        int r = await fs.ReadAsync(buf.AsMemory(n, want - n), ct).ConfigureAwait(false);
                        if (r <= 0) break;
                        n += r;
                    }

                    for (int i = n - patLower.Length; i >= 0; i--)
                    {
                        long abs = start + i;
                        if (abs >= pos) continue;              // handled by a later (further) block
                        if (abs + patLower.Length > before) continue;
                        if (MatchAt(buf, i, patLower)) return abs;
                    }
                    pos = start;
                }
                return -1;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf);
            }
        }

        private void JumpToMatch(long matchOffset, string query)
        {
            if (!_active) return;
            long lineStart = ComputeLineStart(matchOffset);
            long top = UpKLines(lineStart, 2); // keep a little context above the hit
            Render(top, aligned: true);

            int idx = _content.Text.IndexOf(query, System.StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                _content.Focus();
                _content.Select(idx, query.Length);
            }
            _lastMatchOffset = matchOffset;
        }

        public async Task FindNextAsync(string query)
        {
            if (!_active || string.IsNullOrEmpty(query)) return;
            byte[]? pat = EncodePattern(query);
            if (pat == null) { SearchStatusChanged?.Invoke("No matches"); return; }

            byte[] patLower = new byte[pat.Length];
            for (int i = 0; i < pat.Length; i++) patLower[i] = ToLowerAscii(pat[i]);

            _searchCts?.Cancel();
            _searchCts = new System.Threading.CancellationTokenSource();
            var ct = _searchCts.Token;

            SearchStatusChanged?.Invoke("Searching...");
            long from = (_lastMatchOffset >= 0 && query == _lastQuery) ? _lastMatchOffset + 1 : _topOffset;

            long m = await ScanForwardAsync(patLower, from, ct);
            bool wrapped = false;
            if (m < 0 && from > 0)
            {
                m = await ScanForwardAsync(patLower, 0, ct);
                wrapped = true;
            }

            if (ct.IsCancellationRequested) return;
            _lastQuery = query;
            if (m >= 0)
            {
                JumpToMatch(m, query);
                SearchStatusChanged?.Invoke($"Match at 0x{m:X}" + (wrapped ? " (wrapped)" : string.Empty));
            }
            else
            {
                SearchStatusChanged?.Invoke("No matches");
            }
        }

        public async Task FindPreviousAsync(string query)
        {
            if (!_active || string.IsNullOrEmpty(query)) return;
            byte[]? pat = EncodePattern(query);
            if (pat == null) { SearchStatusChanged?.Invoke("No matches"); return; }

            byte[] patLower = new byte[pat.Length];
            for (int i = 0; i < pat.Length; i++) patLower[i] = ToLowerAscii(pat[i]);

            _searchCts?.Cancel();
            _searchCts = new System.Threading.CancellationTokenSource();
            var ct = _searchCts.Token;

            SearchStatusChanged?.Invoke("Searching...");
            long before = (_lastMatchOffset >= 0 && query == _lastQuery) ? _lastMatchOffset : _topOffset;

            long m = await ScanBackwardAsync(patLower, before, ct);
            bool wrapped = false;
            if (m < 0 && before < _fileSize)
            {
                m = await ScanBackwardAsync(patLower, _fileSize, ct);
                wrapped = true;
            }

            if (ct.IsCancellationRequested) return;
            _lastQuery = query;
            if (m >= 0)
            {
                JumpToMatch(m, query);
                SearchStatusChanged?.Invoke($"Match at 0x{m:X}" + (wrapped ? " (wrapped)" : string.Empty));
            }
            else
            {
                SearchStatusChanged?.Invoke("No matches");
            }
        }

        public void ResetSearch()
        {
            _searchCts?.Cancel();
            _lastMatchOffset = -1;
            _lastQuery = null;
            SearchStatusChanged?.Invoke(string.Empty);
        }
    }
}
