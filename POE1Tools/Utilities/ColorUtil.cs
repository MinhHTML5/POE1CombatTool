using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POE1Tools.Utilities
{
    public static class ScreenHelper
    {
        private static Screen _current;

        /// <summary>The screen the game is on. Defaults to the primary screen.</summary>
        public static Screen Current
        {
            get
            {
                if (_current == null || !Screen.AllScreens.Any(s => s.DeviceName == _current.DeviceName))
                    _current = Screen.PrimaryScreen;
                return _current;
            }
            set { _current = value; }
        }
    }

    public class ColorUtil
    {
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(
        IntPtr hdcDest, int nXDest, int nYDest,
        int nWidth, int nHeight,
        IntPtr hdcSrc, int nXSrc, int nYSrc,
        int dwRop);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private const int SRCCOPY = 0x00CC0020;

        private readonly Bitmap _bitmap;
        private readonly Graphics _graphics;

        public ColorUtil()
        {
            _bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            _graphics = Graphics.FromImage(_bitmap);
        }

        /// <summary>Absolute desktop coordinates of a ratio position on the selected screen.</summary>
        public Point GetPixelPosition(float xRatio, float yRatio)
        {
            Rectangle b = ScreenHelper.Current.Bounds;
            return new Point(b.X + (int)(b.Width * xRatio), b.Y + (int)(b.Height * yRatio));
        }

        public Color GetColorAt(Point position)
        {
            IntPtr hdcDest = _graphics.GetHdc();
            IntPtr hdcSrc = GetWindowDC(IntPtr.Zero); // Get full desktop DC
            BitBlt(hdcDest, 0, 0, 1, 1, hdcSrc, position.X, position.Y, SRCCOPY);
            ReleaseDC(IntPtr.Zero, hdcSrc);
            _graphics.ReleaseHdc(hdcDest);
            _graphics.Flush();              // Ensure update if needed
            Color temp = _bitmap.GetPixel(0, 0);
            return _bitmap.GetPixel(0, 0);
        }

        public bool IsColorSimilar(Color color1, Color color2, int tolerance)
        {
            bool difference = false;
            if (Math.Abs(color1.R - color2.R) > tolerance)
            {
                difference = true;
            }
            else if (Math.Abs(color1.G - color2.G) > tolerance)
            {
                difference = true;
            }
            if (Math.Abs(color1.B - color2.B) > tolerance)
            {
                difference = true;
            }
            return !difference;
        }

        ~ColorUtil()
        {
            _graphics.Dispose();
            _bitmap.Dispose();
        }
    }
}
