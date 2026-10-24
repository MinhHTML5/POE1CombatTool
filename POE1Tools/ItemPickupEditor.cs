using System;
using System.Drawing;
using System.Windows.Forms;
using POE1Tools.Modules;
using POE1Tools.Utilities;

namespace POE1Tools
{
    // The part of the Automation window that edits the settings of an "Item pickup" macro preset
    public partial class ItemPickupEditor : UserControl
    {
        private AutomationModule _automationModule;
        private ItemPickupSettings _settings;
        // Set while the code itself fills the controls
        private bool _loading = false;

        // Something was changed by the user. The settings given to SetSettings are already updated
        public event Action SettingsChanged;

        public ItemPickupEditor()
        {
            InitializeComponent();
        }

        // The settings are edited in place
        public void SetSettings(AutomationModule automationModule, ItemPickupSettings settings)
        {
            _automationModule = automationModule;
            _settings = settings;

            _loading = true;
            SetNumber(numTolerance, settings.ColorTolerance);
            SetNumber(numDelayAfterClick, settings.DelayAfterClick);
            SetNumber(numScanInterval, settings.ScanInterval);
            SetNumber(numEmptyScans, settings.EmptyScansToEnd);
            SetNumber(numMaxClicks, settings.MaxClicks);
            SetNumber(numRegionLeft, (decimal)settings.RegionLeft);
            SetNumber(numRegionTop, (decimal)settings.RegionTop);
            SetNumber(numRegionRight, (decimal)settings.RegionRight);
            SetNumber(numRegionBottom, (decimal)settings.RegionBottom);
            _loading = false;

            RefreshLabels(0);
        }

        private static void SetNumber(NumericUpDown box, decimal value)
        {
            box.Value = Math.Max(box.Minimum, Math.Min(box.Maximum, value));
        }

        private void num_ValueChanged(object sender, EventArgs e)
        {
            if (_loading || _settings == null) return;

            _settings.ColorTolerance = (int)numTolerance.Value;
            _settings.DelayAfterClick = (int)numDelayAfterClick.Value;
            _settings.ScanInterval = (int)numScanInterval.Value;
            _settings.EmptyScansToEnd = (int)numEmptyScans.Value;
            _settings.MaxClicks = (int)numMaxClicks.Value;
            _settings.RegionLeft = (double)numRegionLeft.Value;
            _settings.RegionTop = (double)numRegionTop.Value;
            _settings.RegionRight = (double)numRegionRight.Value;
            _settings.RegionBottom = (double)numRegionBottom.Value;
            SettingsChanged?.Invoke();
        }

        private int GetSelectedLabelIndex()
        {
            return lstLabels.SelectedIndices.Count > 0 ? lstLabels.SelectedIndices[0] : -1;
        }

        // Rebuild the list of label colors, then select the given one
        private void RefreshLabels(int selectIndex)
        {
            lstLabels.BeginUpdate();
            lstLabels.Items.Clear();
            foreach (LabelColor label in _settings.Labels)
            {
                // Each row looks like the label it finds
                ListViewItem item = new ListViewItem("Item name");
                item.SubItems.Add(label.Background);
                item.SubItems.Add(label.Text);
                item.BackColor = label.BackgroundColor;
                item.ForeColor = label.TextColor;
                lstLabels.Items.Add(item);
            }
            if (lstLabels.Items.Count > 0)
            {
                selectIndex = Math.Max(0, Math.Min(selectIndex, lstLabels.Items.Count - 1));
                lstLabels.Items[selectIndex].Selected = true;
            }
            lstLabels.EndUpdate();
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            btnLabelEdit.Enabled = GetSelectedLabelIndex() >= 0;
            btnLabelRemove.Enabled = GetSelectedLabelIndex() >= 0;
        }

        private void lstLabels_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtons();
        }

        // Take the colors from a screenshot of the game: the user selects the inside of a label
        private void btnLabelCapture_Click(object sender, EventArgs e)
        {
            using (CaptureForm captureForm = new CaptureForm(_automationModule))
            {
                if (captureForm.ShowDialog(this) != DialogResult.OK) return;

                using (Bitmap sample = captureForm.Sample)
                {
                    // Let the user check what was guessed
                    LabelColor label = EditLabel(ItemLabelScanner.GuessLabelColor(sample));
                    if (label == null) return;

                    _settings.Labels.Add(label);
                }
            }
            RefreshLabels(_settings.Labels.Count - 1);
            SettingsChanged?.Invoke();
        }

        private void btnLabelEdit_Click(object sender, EventArgs e)
        {
            int index = GetSelectedLabelIndex();
            if (index < 0) return;

            LabelColor label = EditLabel(_settings.Labels[index]);
            if (label == null) return;

            _settings.Labels[index] = label;
            RefreshLabels(index);
            SettingsChanged?.Invoke();
        }

        private void lstLabels_DoubleClick(object sender, EventArgs e)
        {
            btnLabelEdit_Click(sender, e);
        }

        private void btnLabelRemove_Click(object sender, EventArgs e)
        {
            int index = GetSelectedLabelIndex();
            if (index < 0) return;

            _settings.Labels.RemoveAt(index);
            RefreshLabels(index);
            SettingsChanged?.Invoke();
        }

        // Returns the edited colors, or null if the user cancelled
        private LabelColor EditLabel(LabelColor label)
        {
            using (Form dialog = new Form())
            using (Label preview = new Label())
            using (Button backgroundButton = new Button())
            using (Button textButton = new Button())
            using (Button okButton = new Button())
            using (Button cancelButton = new Button())
            {
                dialog.Text = "Label colors";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.ClientSize = new Size(300, 125);
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;

                preview.Text = "Item name";
                preview.Font = new Font(Font.FontFamily, 12F, FontStyle.Bold);
                preview.TextAlign = ContentAlignment.MiddleCenter;
                preview.BorderStyle = BorderStyle.FixedSingle;
                preview.BackColor = label.BackgroundColor;
                preview.ForeColor = label.TextColor;
                preview.Location = new Point(12, 12);
                preview.Size = new Size(276, 36);

                backgroundButton.Text = "Background...";
                backgroundButton.Location = new Point(12, 56);
                backgroundButton.Size = new Size(135, 25);
                backgroundButton.Click += (s, e) => preview.BackColor = PickColor(dialog, preview.BackColor);

                textButton.Text = "Text...";
                textButton.Location = new Point(153, 56);
                textButton.Size = new Size(135, 25);
                textButton.Click += (s, e) => preview.ForeColor = PickColor(dialog, preview.ForeColor);

                okButton.Text = "OK";
                okButton.DialogResult = DialogResult.OK;
                okButton.Location = new Point(132, 91);
                okButton.Size = new Size(75, 25);

                cancelButton.Text = "Cancel";
                cancelButton.DialogResult = DialogResult.Cancel;
                cancelButton.Location = new Point(213, 91);
                cancelButton.Size = new Size(75, 25);

                dialog.Controls.Add(preview);
                dialog.Controls.Add(backgroundButton);
                dialog.Controls.Add(textButton);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                return dialog.ShowDialog(this) == DialogResult.OK ? new LabelColor(preview.BackColor, preview.ForeColor) : null;
            }
        }

        private static Color PickColor(Form owner, Color color)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.Color = color;
                colorDialog.FullOpen = true;
                return colorDialog.ShowDialog(owner) == DialogResult.OK ? colorDialog.Color : color;
            }
        }
    }
}
