using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace POE1Tools.Utilities
{
    // Compares an area of the screen with a sample picture. Only that area is captured,
    // and the bitmap / buffers are reused between checks to keep it cheap.
    public class ScreenMatcher : IDisposable
    {
        private readonly Rectangle _region;
        private readonly int[] _samplePixels;
        private readonly int[] _screenPixels;
        private readonly Bitmap _bitmap;
        private readonly Graphics _graphics;

        public ScreenMatcher(Bitmap sample, Rectangle region)
        {
            // The sample decides the size, the region only says where to look
            _region = new Rectangle(region.X, region.Y, sample.Width, sample.Height);
            _samplePixels = GetPixels(sample);
            _screenPixels = new int[_samplePixels.Length];
            _bitmap = new Bitmap(sample.Width, sample.Height, PixelFormat.Format32bppArgb);
            _graphics = Graphics.FromImage(_bitmap);
        }

        // Capture the area, and return how many percent of its pixels are similar to the sample
        public double GetMatchPercent(int colorTolerance)
        {
            _graphics.CopyFromScreen(_region.X, _region.Y, 0, 0, _region.Size);
            ReadPixels(_bitmap, _screenPixels);

            int matched = 0;
            for (int i = 0; i < _samplePixels.Length; i++)
            {
                int a = _samplePixels[i];
                int b = _screenPixels[i];
                if (Math.Abs(((a >> 16) & 0xFF) - ((b >> 16) & 0xFF)) <= colorTolerance
                 && Math.Abs(((a >> 8) & 0xFF) - ((b >> 8) & 0xFF)) <= colorTolerance
                 && Math.Abs((a & 0xFF) - (b & 0xFF)) <= colorTolerance)
                {
                    matched++;
                }
            }
            return _samplePixels.Length == 0 ? 0 : matched * 100.0 / _samplePixels.Length;
        }

        public void Dispose()
        {
            _graphics.Dispose();
            _bitmap.Dispose();
        }



        public static Bitmap CapturePrimaryScreen()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
            }
            return bitmap;
        }

        public static string ToBase64(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        // Returns null if the text is not a valid picture
        public static Bitmap FromBase64(string base64)
        {
            if (string.IsNullOrEmpty(base64)) return null;
            try
            {
                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(base64)))
                using (Bitmap loaded = new Bitmap(stream))
                {
                    // Copy it, a Bitmap made from a stream needs that stream alive forever otherwise
                    return loaded.Clone(new Rectangle(0, 0, loaded.Width, loaded.Height), PixelFormat.Format32bppArgb);
                }
            }
            catch
            {
                return null;
            }
        }

        private static int[] GetPixels(Bitmap bitmap)
        {
            int[] pixels = new int[bitmap.Width * bitmap.Height];
            if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
            {
                ReadPixels(bitmap, pixels);
            }
            else
            {
                using (Bitmap converted = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.Format32bppArgb))
                {
                    ReadPixels(converted, pixels);
                }
            }
            return pixels;
        }

        private static void ReadPixels(Bitmap bitmap, int[] pixels)
        {
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                // 32bpp rows have no padding, so the whole picture is one block of ints
                Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }
    }
}
