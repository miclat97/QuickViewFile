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
            return Encoding.GetEncoding(name, new EncoderReplacementFallback(""), new DecoderReplacementFallback(""));
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


        private string FormatOffset(long off) => off.ToString("X" + _offsetHexDigits.ToString());

        private string DecodeLine(byte[] buf, int start, int len)
        {
            if (len <= 0) return string.Empty;
            string raw = _encoding.GetString(buf, start, len);
            var sb = new StringBuilder(raw.Length);
            foreach (char c in raw)
            {
                if (c == '\n' || c == '\r') continue; // line breaks are structural
                if (c >= 32 || c == '\t') sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>Renders the file starting at (or line-aligned to) <paramref name="requested"/> byte offset.</summary>
        public void Render(long requested, bool aligned)
        {
            if (!_active) return;

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
            // When wrapping, one logical line spans several display rows, so a logical-line count can't bound
            // the scroll; anchoring the bottom at the last logical line guarantees the end stays reachable.
            long maxTop = _wordWrap ? _lastLineStart : ComputeMaxTop(visible);
            if (top > maxTop) top = maxTop;
            if (top < 0) top = 0;
            _topOffset = top;

            int windowLen = ReadAt(top, _forwardBuf, _forwardBuf.Length);

            _renderedStarts.Clear();
            _renderedCharStarts.Clear();
            var sbContent = new StringBuilder();

            long pos = top;
            int produced = 0;
            while (produced < toRender && pos < _fileSize)
            {
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

            UpdateGutter();

            long pageBytes = System.Math.Max(MaxLineBytes, _afterLast - _topOffset);
            _scrollBar.Maximum = System.Math.Max(0, maxTop);
            _scrollBar.ViewportSize = pageBytes;
            _scrollBar.LargeChange = pageBytes;               // clicking the track pages by ~one screen
            _scrollBar.Value = System.Math.Min(_topOffset, _scrollBar.Maximum);
            _scrollBar.IsEnabled = maxTop > 0;
        }

        /// <summary>
        /// Fills the gutter with hex offsets. Without wrapping, one offset per logical line. With wrapping,
        /// one row per visual (wrapped) display row, and the offset is shown only on the first display row of
        /// each logical line; continuation rows are left blank so the numbers stay aligned with the text.
        /// </summary>
        private void UpdateGutter()
        {
            if (!_wordWrap)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < _renderedStarts.Count; i++)
                {
                    if (i > 0) sb.Append('\n');
                    sb.Append(FormatOffset(_renderedStarts[i]));
                }
                _gutter.Text = sb.ToString();
                return;
            }

            _content.UpdateLayout();
            int total;
            try { total = _content.LineCount; } catch { total = -1; }

            if (total <= 0)
            {
                // Layout not ready yet - fall back to one offset per logical line.
                var sb = new StringBuilder();
                for (int i = 0; i < _renderedStarts.Count; i++)
                {
                    if (i > 0) sb.Append('\n');
                    sb.Append(FormatOffset(_renderedStarts[i]));
                }
                _gutter.Text = sb.ToString();
                return;
            }

            string[] rows = new string[total];
            for (int i = 0; i < total; i++) rows[i] = string.Empty;

            int textLen = _content.Text.Length;
            for (int i = 0; i < _renderedStarts.Count; i++)
            {
                int cs = _renderedCharStarts[i];
                if (cs > textLen) cs = textLen;
                int row;
                try { row = _content.GetLineIndexFromCharacterIndex(cs); }
                catch { row = -1; }
                if (row >= 0 && row < total) rows[row] = FormatOffset(_renderedStarts[i]);
            }
            _gutter.Text = string.Join("\n", rows);
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
            Render((long)value, aligned: false);
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
