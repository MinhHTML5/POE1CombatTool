using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using POE1Tools.Modules;
using POE1Tools.Utilities;

namespace POE1Tools
{
    // Ctrl + Right takes a screenshot of the main monitor, then the user drags a rectangle
    // on the preview to crop the sample a screen condition must look for.
    public partial class CaptureForm : Form
    {
        private AutomationModule _automationModule;
        private Bitmap _screenshot;
        // The screenshot scaled to the preview, so dragging the selection doesn't rescale it every time
        private Bitmap _scaledScreenshot;
        // In screenshot pixels, which are also screen pixels as the main monitor starts at (0, 0)
        private Rectangle _selection;
        private bool _dragging = false;
        private Point _dragStart;
        private bool _updatingNumbers = false;

        // The results, valid when the dialog returns OK. The caller owns the bitmap.
        public Bitmap Sample { get; private set; }
        public Rectangle SampleRegion { get; private set; }

        public CaptureForm(AutomationModule automationModule)
        {
            _automationModule = automationModule;
            InitializeComponent();
        }

        private void CaptureForm_Load(object sender, EventArgs e)
        {
            _automationModule.CaptureRequested += OnCaptureRequested;
        }

        private void CaptureForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _automationModule.CaptureRequested -= OnCaptureRequested;
            _screenshot?.Dispose();
            _scaledScreenshot?.Dispose();
        }

        private void OnCaptureRequested()
        {
            Bitmap screenshot;
            try
            {
                screenshot = ScreenMatcher.CapturePrimaryScreen();
            }
            catch (Exception ex)
            {
                lblInstruction.Text = "Screenshot failed: " + ex.Message;
                return;
            }

            _screenshot?.Dispose();
            _screenshot = screenshot;
            _selection = Rectangle.Empty;
            RebuildScaledScreenshot();
            UpdateSelectionControls();

            lblInstruction.Text = "Drag on the picture to select what to look for, then save. Ctrl + Right takes a new screenshot.";
            Activate();
        }



        // Where the screenshot is drawn inside the preview, keeping its proportions
        private Rectangle GetDisplayRectangle()
        {
            if (_screenshot == null) return Rectangle.Empty;

            Size area = picPreview.ClientSize;
            float scale = Math.Min((float)area.Width / _screenshot.Width, (float)area.Height / _screenshot.Height);
            int width = Math.Max(1, (int)(_screenshot.Width * scale));
            int height = Math.Max(1, (int)(_screenshot.Height * scale));
            return new Rectangle((area.Width - width) / 2, (area.Height - height) / 2, width, height);
        }

        private Point PreviewToImage(Point point)
        {
            Rectangle display = GetDisplayRectangle();
            int x = (int)Math.Round((point.X - display.X) * (double)_screenshot.Width / display.Width);
            int y = (int)Math.Round((point.Y - display.Y) * (double)_screenshot.Height / display.Height);
            return new Point(Math.Max(0, Math.Min(_screenshot.Width, x)), Math.Max(0, Math.Min(_screenshot.Height, y)));
        }

        private Rectangle ImageToPreview(Rectangle rect)
        {
            Rectangle display = GetDisplayRectangle();
            double scaleX = (double)display.Width / _screenshot.Width;
            double scaleY = (double)display.Height / _screenshot.Height;
            return new Rectangle(
                display.X + (int)(rect.X * scaleX),
                display.Y + (int)(rect.Y * scaleY),
                Math.Max(1, (int)(rect.Width * scaleX)),
                Math.Max(1, (int)(rect.Height * scaleY)));
        }

        private void RebuildScaledScreenshot()
        {
            _scaledScreenshot?.Dispose();
            _scaledScreenshot = null;

            Rectangle display = GetDisplayRectangle();
            if (_screenshot != null && display.Width > 0 && display.Height > 0)
            {
                _scaledScreenshot = new Bitmap(display.Width, display.Height, PixelFormat.Format32bppArgb);
                using (Graphics graphics = Graphics.FromImage(_scaledScreenshot))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    graphics.DrawImage(_screenshot, 0, 0, display.Width, display.Height);
                }
            }
            picPreview.Invalidate();
        }

        private void picPreview_SizeChanged(object sender, EventArgs e)
        {
            RebuildScaledScreenshot();
        }

        private void picPreview_Paint(object sender, PaintEventArgs e)
        {
            if (_scaledScreenshot == null)
            {
                TextRenderer.DrawText(e.Graphics, "No screenshot yet. Press Ctrl + Right.", Font, picPreview.ClientRectangle, Color.Silver,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            Rectangle display = GetDisplayRectangle();
            e.Graphics.DrawImageUnscaled(_scaledScreenshot, display.Location);

            if (_selection.Width > 0 && _selection.Height > 0)
            {
                Rectangle rect = ImageToPreview(_selection);
                // Darken everything but the selection
                using (Region outside = new Region(display))
                using (Brush shade = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
                using (Pen pen = new Pen(Color.Red, 1))
                {
                    outside.Exclude(rect);
                    e.Graphics.FillRegion(shade, outside);
                    e.Graphics.DrawRectangle(pen, rect.X - 1, rect.Y - 1, rect.Width + 1, rect.Height + 1);
                }
            }
        }

        private void picPreview_MouseDown(object sender, MouseEventArgs e)
        {
            if (_screenshot == null || e.Button != MouseButtons.Left) return;
            _dragging = true;
            _dragStart = PreviewToImage(e.Location);
        }

        private void picPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;

            Point current = PreviewToImage(e.Location);
            _selection = Rectangle.FromLTRB(
                Math.Min(_dragStart.X, current.X), Math.Min(_dragStart.Y, current.Y),
                Math.Max(_dragStart.X, current.X), Math.Max(_dragStart.Y, current.Y));
            UpdateSelectionControls();
        }

        private void picPreview_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        // Show the selection in the number boxes, they can then be used to fine tune it
        private void UpdateSelectionControls()
        {
            _updatingNumbers = true;
            numX.Value = _selection.X;
            numY.Value = _selection.Y;
            numWidth.Value = _selection.Width;
            numHeight.Value = _selection.Height;
            _updatingNumbers = false;

            btnSave.Enabled = _screenshot != null && _selection.Width > 0 && _selection.Height > 0;
            picPreview.Invalidate();
        }

        private void numSelection_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingNumbers || _screenshot == null) return;

            Rectangle selection = new Rectangle((int)numX.Value, (int)numY.Value, (int)numWidth.Value, (int)numHeight.Value);
            selection.Intersect(new Rectangle(Point.Empty, _screenshot.Size));
            _selection = selection;
            UpdateSelectionControls();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_screenshot == null || _selection.Width <= 0 || _selection.Height <= 0) return;

            Sample = _screenshot.Clone(_selection, PixelFormat.Format32bppArgb);
            SampleRegion = _selection;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
