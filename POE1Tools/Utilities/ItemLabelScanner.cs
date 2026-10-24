using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace POE1Tools.Utilities
{
    // Finds the item labels of the loot filter on the screen: boxes of a flat background color with text of another color.
    // The game world almost never has a long horizontal run of one exact color, so only one row every few pixels
    // is searched for such runs, and the few candidates are then checked more carefully.
    // The bitmap / buffer are reused between scans to keep it cheap.
    public class ItemLabelScanner : IDisposable
    {
        // All sizes are for a 1080p screen, they are scaled to the real one
        private const int REFERENCE_HEIGHT = 1080;
        // Smaller than the height of a label, so every label is crossed by several searched rows
        private const int ROW_STEP = 8;
        private const int MIN_RUN_WIDTH = 50;
        // The letters cut the background, a run goes on if the background is back within this many pixels
        private const int MAX_GAP = 24;
        // How much of a run must be the background color, the rest being the letters
        private const double MIN_RUN_FILL = 0.5;
        private const int MIN_LABEL_HEIGHT = 14;
        // Two lines of text. Labels of the same color can touch each other, those are still accepted up to 3 times that
        private const int MAX_LABEL_HEIGHT = 110;
        // The text is looked for in the rows this close to the run
        private const int TEXT_CHECK_HALF_HEIGHT = 16;
        private const double MIN_TEXT_FILL = 0.03;

        private struct Rule
        {
            public int BackR, BackG, BackB;
            public int TextR, TextG, TextB;
        }

        private readonly Rule[] _rules;
        private readonly int _tolerance;
        private readonly int _textTolerance;
        private readonly Rectangle _region;
        private readonly Point _screenCenter;
        private readonly int _rowStep, _minRunWidth, _maxGap, _minHeight, _maxHeight, _textHalfHeight;
        private readonly int[] _pixels;
        private readonly Bitmap _bitmap;
        private readonly Graphics _graphics;

        // How many labels the last scan found. The same label can be counted once per searched row crossing it
        public int LastFoundCount { get; private set; }

        public ItemLabelScanner(ItemPickupSettings settings)
        {
            Rectangle screen = Screen.PrimaryScreen.Bounds;
            _screenCenter = new Point(screen.Width / 2, screen.Height / 2);

            int left = InputHook.PercentToPixel(Math.Min(settings.RegionLeft, settings.RegionRight), screen.Width);
            int right = InputHook.PercentToPixel(Math.Max(settings.RegionLeft, settings.RegionRight), screen.Width);
            int top = InputHook.PercentToPixel(Math.Min(settings.RegionTop, settings.RegionBottom), screen.Height);
            int bottom = InputHook.PercentToPixel(Math.Max(settings.RegionTop, settings.RegionBottom), screen.Height);
            _region = Rectangle.FromLTRB(left, top, right, bottom);
            _region.Intersect(new Rectangle(Point.Empty, screen.Size));
            if (_region.Width <= 0 || _region.Height <= 0) _region = new Rectangle(Point.Empty, screen.Size);

            double scale = Math.Max(0.5, (double)screen.Height / REFERENCE_HEIGHT);
            _rowStep = Math.Max(2, (int)(ROW_STEP * scale));
            _minRunWidth = (int)(MIN_RUN_WIDTH * scale);
            _maxGap = (int)(MAX_GAP * scale);
            _minHeight = (int)(MIN_LABEL_HEIGHT * scale);
            _maxHeight = (int)(MAX_LABEL_HEIGHT * scale);
            _textHalfHeight = (int)(TEXT_CHECK_HALF_HEIGHT * scale);

            _tolerance = Math.Max(0, settings.ColorTolerance);
            _textTolerance = Math.Min(255, _tolerance * 2);
            _rules = new Rule[settings.Labels.Count];
            for (int i = 0; i < _rules.Length; i++)
            {
                Color back = settings.Labels[i].BackgroundColor;
                Color text = settings.Labels[i].TextColor;
                _rules[i] = new Rule { BackR = back.R, BackG = back.G, BackB = back.B, TextR = text.R, TextG = text.G, TextB = text.B };
            }

            _pixels = new int[_region.Width * _region.Height];
            _bitmap = new Bitmap(_region.Width, _region.Height, PixelFormat.Format32bppArgb);
            _graphics = Graphics.FromImage(_bitmap);
        }

        public void Dispose()
        {
            _graphics.Dispose();
            _bitmap.Dispose();
        }

        // Capture the screen, and find the label that is the closest to the center of the screen, where the character is.
        // Returns false if there is no label. The point is in screen pixels, and is inside the label.
        public bool FindNearestLabel(out Point clickPoint)
        {
            _graphics.CopyFromScreen(_region.X, _region.Y, 0, 0, _region.Size);
            BitmapData data = _bitmap.LockBits(new Rectangle(Point.Empty, _region.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                Marshal.Copy(data.Scan0, _pixels, 0, _pixels.Length);
            }
            finally
            {
                _bitmap.UnlockBits(data);
            }
            return FindNearestLabelInPixels(out clickPoint);
        }

        private bool FindNearestLabelInPixels(out Point clickPoint)
        {
            clickPoint = Point.Empty;
            LastFoundCount = 0;
            long bestDistance = long.MaxValue;
            int width = _region.Width;

            for (int y = _rowStep / 2; y < _region.Height; y += _rowStep)
            {
                int rowStart = y * width;
                int runRule = -1, runStart = 0, lastMatch = 0, matchCount = 0;

                // One step past the end of the row, to close the run that touches the right edge
                for (int x = 0; x <= width; x++)
                {
                    int rule = x < width ? GetBackgroundRule(_pixels[rowStart + x]) : -1;
                    if (rule >= 0 && rule == runRule)
                    {
                        lastMatch = x;
                        matchCount++;
                        continue;
                    }

                    // Anything else is a gap in the run, even the background of another label: the white text
                    // of an orange label must not be taken for the start of a white label
                    if (runRule >= 0 && (x - lastMatch > _maxGap || x == width))
                    {
                        if (CheckRun(runRule, y, runStart, lastMatch, matchCount, out Point point))
                        {
                            LastFoundCount++;
                            long dx = point.X - _screenCenter.X;
                            long dy = point.Y - _screenCenter.Y;
                            if (dx * dx + dy * dy < bestDistance)
                            {
                                bestDistance = dx * dx + dy * dy;
                                clickPoint = point;
                            }
                        }
                        runRule = -1;
                    }
                    if (runRule < 0 && rule >= 0)
                    {
                        runRule = rule;
                        runStart = x;
                        lastMatch = x;
                        matchCount = 1;
                    }
                }
            }
            return LastFoundCount > 0;
        }

        // Is this run of background color really crossing a label. If so, also returns where to click, in screen pixels
        private bool CheckRun(int ruleIndex, int y, int runStart, int runEnd, int matchCount, out Point clickPoint)
        {
            clickPoint = Point.Empty;
            int runWidth = runEnd - runStart + 1;
            if (runWidth < _minRunWidth || matchCount < runWidth * MIN_RUN_FILL) return false;

            // The first pixels of the run are the padding at the left of the text, the background goes
            // from the top to the bottom of the label there
            Rule rule = _rules[ruleIndex];
            int width = _region.Width;
            int column = Math.Min(runStart + 2, runEnd);
            int heightLimit = _maxHeight * 3;
            int top = y, bottom = y;
            while (top > 0 && y - top <= heightLimit && IsBackground(_pixels[(top - 1) * width + column], rule)) top--;
            while (bottom < _region.Height - 1 && bottom - y <= heightLimit && IsBackground(_pixels[(bottom + 1) * width + column], rule)) bottom++;

            int height = bottom - top + 1;
            if (height < _minHeight || height > heightLimit) return false;

            // A flat surface with nothing written on it is not a label
            int textTop = Math.Max(top, y - _textHalfHeight);
            int textBottom = Math.Min(bottom, y + _textHalfHeight);
            int textCount = 0, checkedCount = 0;
            for (int textY = textTop; textY <= textBottom; textY += 2)
            {
                int rowStart = textY * width;
                for (int x = runStart; x <= runEnd; x++)
                {
                    int pixel = _pixels[rowStart + x];
                    if (Math.Abs(((pixel >> 16) & 0xFF) - rule.TextR) <= _textTolerance
                     && Math.Abs(((pixel >> 8) & 0xFF) - rule.TextG) <= _textTolerance
                     && Math.Abs((pixel & 0xFF) - rule.TextB) <= _textTolerance)
                    {
                        textCount++;
                    }
                    checkedCount++;
                }
            }
            if (textCount < checkedCount * MIN_TEXT_FILL) return false;

            // The middle of the label, unless it is several labels touching each other: the searched row is then the safe place
            int clickY = height <= _maxHeight ? (top + bottom) / 2 : y;
            clickPoint = new Point(_region.X + (runStart + runEnd) / 2, _region.Y + clickY);
            return true;
        }

        // Returns the index of the rule this pixel is the background of, -1 if none
        private int GetBackgroundRule(int pixel)
        {
            for (int i = 0; i < _rules.Length; i++)
            {
                if (IsBackground(pixel, _rules[i])) return i;
            }
            return -1;
        }

        private bool IsBackground(int pixel, Rule rule)
        {
            return Math.Abs(((pixel >> 16) & 0xFF) - rule.BackR) <= _tolerance
                && Math.Abs(((pixel >> 8) & 0xFF) - rule.BackG) <= _tolerance
                && Math.Abs((pixel & 0xFF) - rule.BackB) <= _tolerance;
        }



        // Guess the colors of a label from a picture of it: the background is the most common color,
        // the text is the most common color among those that are far from the background
        public static LabelColor GuessLabelColor(Bitmap sample)
        {
            int[] pixels = new int[sample.Width * sample.Height];
            using (Bitmap converted = sample.Clone(new Rectangle(0, 0, sample.Width, sample.Height), PixelFormat.Format32bppArgb))
            {
                BitmapData data = converted.LockBits(new Rectangle(0, 0, converted.Width, converted.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                }
                finally
                {
                    converted.UnlockBits(data);
                }
            }

            Color background = GetMostCommonColor(pixels, null);
            Color text = GetMostCommonColor(pixels, background);
            return new LabelColor(background, text);
        }

        // Colors are grouped in cubes of 16 levels, the result is the average of the biggest group
        private static Color GetMostCommonColor(int[] pixels, Color? farFrom)
        {
            const int MIN_DISTANCE = 120;
            Dictionary<int, long[]> groups = new Dictionary<int, long[]>();
            long[] best = null;
            foreach (int pixel in pixels)
            {
                int r = (pixel >> 16) & 0xFF, g = (pixel >> 8) & 0xFF, b = pixel & 0xFF;
                if (farFrom != null && Math.Abs(r - farFrom.Value.R) + Math.Abs(g - farFrom.Value.G) + Math.Abs(b - farFrom.Value.B) < MIN_DISTANCE) continue;

                int key = ((r >> 4) << 8) | ((g >> 4) << 4) | (b >> 4);
                if (!groups.TryGetValue(key, out long[] group))
                {
                    group = new long[4];
                    groups[key] = group;
                }
                group[0]++;
                group[1] += r;
                group[2] += g;
                group[3] += b;
                if (best == null || group[0] > best[0]) best = group;
            }
            if (best == null) return Color.Black;
            return Color.FromArgb((int)(best[1] / best[0]), (int)(best[2] / best[0]), (int)(best[3] / best[0]));
        }
    }
}
