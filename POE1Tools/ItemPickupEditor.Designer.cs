namespace POE1Tools
{
    partial class ItemPickupEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblHelp = new System.Windows.Forms.Label();
            lstLabels = new System.Windows.Forms.ListView();
            colLabelPreview = new System.Windows.Forms.ColumnHeader();
            colLabelBackground = new System.Windows.Forms.ColumnHeader();
            colLabelText = new System.Windows.Forms.ColumnHeader();
            btnLabelCapture = new System.Windows.Forms.Button();
            btnLabelEdit = new System.Windows.Forms.Button();
            btnLabelRemove = new System.Windows.Forms.Button();
            lblTolerance = new System.Windows.Forms.Label();
            numTolerance = new System.Windows.Forms.NumericUpDown();
            lblToleranceUnit = new System.Windows.Forms.Label();
            lblDelayAfterClick = new System.Windows.Forms.Label();
            numDelayAfterClick = new System.Windows.Forms.NumericUpDown();
            lblDelayAfterClickUnit = new System.Windows.Forms.Label();
            lblScanInterval = new System.Windows.Forms.Label();
            numScanInterval = new System.Windows.Forms.NumericUpDown();
            lblScanIntervalUnit = new System.Windows.Forms.Label();
            lblEmptyScans = new System.Windows.Forms.Label();
            numEmptyScans = new System.Windows.Forms.NumericUpDown();
            lblEmptyScansUnit = new System.Windows.Forms.Label();
            lblMaxClicks = new System.Windows.Forms.Label();
            numMaxClicks = new System.Windows.Forms.NumericUpDown();
            lblMaxClicksUnit = new System.Windows.Forms.Label();
            lblRegion = new System.Windows.Forms.Label();
            lblRegionLeft = new System.Windows.Forms.Label();
            numRegionLeft = new System.Windows.Forms.NumericUpDown();
            lblRegionTop = new System.Windows.Forms.Label();
            numRegionTop = new System.Windows.Forms.NumericUpDown();
            lblRegionRight = new System.Windows.Forms.Label();
            numRegionRight = new System.Windows.Forms.NumericUpDown();
            lblRegionBottom = new System.Windows.Forms.Label();
            numRegionBottom = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)numTolerance).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDelayAfterClick).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numScanInterval).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numEmptyScans).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMaxClicks).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRegionLeft).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRegionTop).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRegionRight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRegionBottom).BeginInit();
            SuspendLayout();
            // 
            // lblHelp
            // 
            lblHelp.Location = new System.Drawing.Point(0, 2);
            lblHelp.Size = new System.Drawing.Size(467, 32);
            lblHelp.Text = "Clicks the item labels having those colors, the closest to the character first, until there is none left.\r\nCapture: select the inside of a label on a screenshot.   Ctrl + Down: run it alone, if there is no action.";
            lblHelp.Name = "lblHelp";
            lblHelp.TabIndex = 0;
            // 
            // lstLabels
            // 
            lstLabels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colLabelPreview, colLabelBackground, colLabelText });
            lstLabels.FullRowSelect = true;
            lstLabels.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            lstLabels.HideSelection = false;
            lstLabels.Location = new System.Drawing.Point(0, 38);
            lstLabels.MultiSelect = false;
            lstLabels.Size = new System.Drawing.Size(385, 140);
            lstLabels.UseCompatibleStateImageBehavior = false;
            lstLabels.View = System.Windows.Forms.View.Details;
            lstLabels.SelectedIndexChanged += lstLabels_SelectedIndexChanged;
            lstLabels.DoubleClick += lstLabels_DoubleClick;
            lstLabels.Name = "lstLabels";
            lstLabels.TabIndex = 1;
            // 
            // colLabelPreview
            // 
            colLabelPreview.Text = "Looks like";
            colLabelPreview.Width = 150;
            // 
            // colLabelBackground
            // 
            colLabelBackground.Text = "Background";
            colLabelBackground.Width = 110;
            // 
            // colLabelText
            // 
            colLabelText.Text = "Text";
            colLabelText.Width = 110;
            // 
            // btnLabelCapture
            // 
            btnLabelCapture.Location = new System.Drawing.Point(391, 38);
            btnLabelCapture.Size = new System.Drawing.Size(76, 25);
            btnLabelCapture.Text = "Capture...";
            btnLabelCapture.UseVisualStyleBackColor = true;
            btnLabelCapture.Click += btnLabelCapture_Click;
            btnLabelCapture.Name = "btnLabelCapture";
            btnLabelCapture.TabIndex = 2;
            // 
            // btnLabelEdit
            // 
            btnLabelEdit.Location = new System.Drawing.Point(391, 68);
            btnLabelEdit.Size = new System.Drawing.Size(76, 25);
            btnLabelEdit.Text = "Edit...";
            btnLabelEdit.UseVisualStyleBackColor = true;
            btnLabelEdit.Click += btnLabelEdit_Click;
            btnLabelEdit.Name = "btnLabelEdit";
            btnLabelEdit.TabIndex = 3;
            // 
            // btnLabelRemove
            // 
            btnLabelRemove.Location = new System.Drawing.Point(391, 98);
            btnLabelRemove.Size = new System.Drawing.Size(76, 25);
            btnLabelRemove.Text = "Remove";
            btnLabelRemove.UseVisualStyleBackColor = true;
            btnLabelRemove.Click += btnLabelRemove_Click;
            btnLabelRemove.Name = "btnLabelRemove";
            btnLabelRemove.TabIndex = 4;
            // 
            // lblTolerance
            // 
            lblTolerance.AutoSize = true;
            lblTolerance.Location = new System.Drawing.Point(0, 193);
            lblTolerance.Size = new System.Drawing.Size(100, 15);
            lblTolerance.Text = "Color tolerance:";
            lblTolerance.Name = "lblTolerance";
            lblTolerance.TabIndex = 5;
            // 
            // numTolerance
            // 
            numTolerance.Location = new System.Drawing.Point(130, 190);
            numTolerance.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numTolerance.Size = new System.Drawing.Size(70, 23);
            numTolerance.ValueChanged += num_ValueChanged;
            numTolerance.Name = "numTolerance";
            numTolerance.TabIndex = 6;
            // 
            // lblToleranceUnit
            // 
            lblToleranceUnit.AutoSize = true;
            lblToleranceUnit.Location = new System.Drawing.Point(206, 193);
            lblToleranceUnit.Size = new System.Drawing.Size(100, 15);
            lblToleranceUnit.Text = "for each of red, green, blue (0 - 255)";
            lblToleranceUnit.Name = "lblToleranceUnit";
            lblToleranceUnit.TabIndex = 7;
            // 
            // lblDelayAfterClick
            // 
            lblDelayAfterClick.AutoSize = true;
            lblDelayAfterClick.Location = new System.Drawing.Point(0, 222);
            lblDelayAfterClick.Size = new System.Drawing.Size(100, 15);
            lblDelayAfterClick.Text = "Wait after a click:";
            lblDelayAfterClick.Name = "lblDelayAfterClick";
            lblDelayAfterClick.TabIndex = 8;
            // 
            // numDelayAfterClick
            // 
            numDelayAfterClick.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            numDelayAfterClick.Location = new System.Drawing.Point(130, 219);
            numDelayAfterClick.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            numDelayAfterClick.Size = new System.Drawing.Size(70, 23);
            numDelayAfterClick.ValueChanged += num_ValueChanged;
            numDelayAfterClick.Name = "numDelayAfterClick";
            numDelayAfterClick.TabIndex = 9;
            // 
            // lblDelayAfterClickUnit
            // 
            lblDelayAfterClickUnit.AutoSize = true;
            lblDelayAfterClickUnit.Location = new System.Drawing.Point(206, 222);
            lblDelayAfterClickUnit.Size = new System.Drawing.Size(100, 15);
            lblDelayAfterClickUnit.Text = "ms, the time to walk to the item";
            lblDelayAfterClickUnit.Name = "lblDelayAfterClickUnit";
            lblDelayAfterClickUnit.TabIndex = 10;
            // 
            // lblScanInterval
            // 
            lblScanInterval.AutoSize = true;
            lblScanInterval.Location = new System.Drawing.Point(0, 251);
            lblScanInterval.Size = new System.Drawing.Size(100, 15);
            lblScanInterval.Text = "Wait after an empty scan:";
            lblScanInterval.Name = "lblScanInterval";
            lblScanInterval.TabIndex = 11;
            // 
            // numScanInterval
            // 
            numScanInterval.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            numScanInterval.Location = new System.Drawing.Point(150, 248);
            numScanInterval.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            numScanInterval.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
            numScanInterval.Size = new System.Drawing.Size(70, 23);
            numScanInterval.Value = new decimal(new int[] { 50, 0, 0, 0 });
            numScanInterval.ValueChanged += num_ValueChanged;
            numScanInterval.Name = "numScanInterval";
            numScanInterval.TabIndex = 12;
            // 
            // lblScanIntervalUnit
            // 
            lblScanIntervalUnit.AutoSize = true;
            lblScanIntervalUnit.Location = new System.Drawing.Point(226, 251);
            lblScanIntervalUnit.Size = new System.Drawing.Size(100, 15);
            lblScanIntervalUnit.Text = "ms";
            lblScanIntervalUnit.Name = "lblScanIntervalUnit";
            lblScanIntervalUnit.TabIndex = 13;
            // 
            // lblEmptyScans
            // 
            lblEmptyScans.AutoSize = true;
            lblEmptyScans.Location = new System.Drawing.Point(0, 280);
            lblEmptyScans.Size = new System.Drawing.Size(100, 15);
            lblEmptyScans.Text = "The macro ends after:";
            lblEmptyScans.Name = "lblEmptyScans";
            lblEmptyScans.TabIndex = 14;
            // 
            // numEmptyScans
            // 
            numEmptyScans.Location = new System.Drawing.Point(130, 277);
            numEmptyScans.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numEmptyScans.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numEmptyScans.Size = new System.Drawing.Size(70, 23);
            numEmptyScans.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numEmptyScans.ValueChanged += num_ValueChanged;
            numEmptyScans.Name = "numEmptyScans";
            numEmptyScans.TabIndex = 15;
            // 
            // lblEmptyScansUnit
            // 
            lblEmptyScansUnit.AutoSize = true;
            lblEmptyScansUnit.Location = new System.Drawing.Point(206, 280);
            lblEmptyScansUnit.Size = new System.Drawing.Size(100, 15);
            lblEmptyScansUnit.Text = "empty scans in a row";
            lblEmptyScansUnit.Name = "lblEmptyScansUnit";
            lblEmptyScansUnit.TabIndex = 16;
            // 
            // lblMaxClicks
            // 
            lblMaxClicks.AutoSize = true;
            lblMaxClicks.Location = new System.Drawing.Point(0, 309);
            lblMaxClicks.Size = new System.Drawing.Size(100, 15);
            lblMaxClicks.Text = "or after:";
            lblMaxClicks.Name = "lblMaxClicks";
            lblMaxClicks.TabIndex = 17;
            // 
            // numMaxClicks
            // 
            numMaxClicks.Location = new System.Drawing.Point(130, 306);
            numMaxClicks.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numMaxClicks.Size = new System.Drawing.Size(70, 23);
            numMaxClicks.ValueChanged += num_ValueChanged;
            numMaxClicks.Name = "numMaxClicks";
            numMaxClicks.TabIndex = 18;
            // 
            // lblMaxClicksUnit
            // 
            lblMaxClicksUnit.AutoSize = true;
            lblMaxClicksUnit.Location = new System.Drawing.Point(206, 309);
            lblMaxClicksUnit.Size = new System.Drawing.Size(100, 15);
            lblMaxClicksUnit.Text = "clicks (0 = no limit)";
            lblMaxClicksUnit.Name = "lblMaxClicksUnit";
            lblMaxClicksUnit.TabIndex = 19;
            // 
            // lblRegion
            // 
            lblRegion.AutoSize = true;
            lblRegion.Location = new System.Drawing.Point(0, 338);
            lblRegion.Size = new System.Drawing.Size(100, 15);
            lblRegion.Text = "Scan region (%):";
            lblRegion.Name = "lblRegion";
            lblRegion.TabIndex = 20;
            // 
            // lblRegionLeft
            // 
            lblRegionLeft.AutoSize = true;
            lblRegionLeft.Location = new System.Drawing.Point(105, 338);
            lblRegionLeft.Size = new System.Drawing.Size(100, 15);
            lblRegionLeft.Text = "Left";
            lblRegionLeft.Name = "lblRegionLeft";
            lblRegionLeft.TabIndex = 21;
            // 
            // numRegionLeft
            // 
            numRegionLeft.DecimalPlaces = 1;
            numRegionLeft.Location = new System.Drawing.Point(135, 335);
            numRegionLeft.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numRegionLeft.Size = new System.Drawing.Size(52, 23);
            numRegionLeft.ValueChanged += num_ValueChanged;
            numRegionLeft.Name = "numRegionLeft";
            numRegionLeft.TabIndex = 22;
            // 
            // lblRegionTop
            // 
            lblRegionTop.AutoSize = true;
            lblRegionTop.Location = new System.Drawing.Point(195, 338);
            lblRegionTop.Size = new System.Drawing.Size(100, 15);
            lblRegionTop.Text = "Top";
            lblRegionTop.Name = "lblRegionTop";
            lblRegionTop.TabIndex = 23;
            // 
            // numRegionTop
            // 
            numRegionTop.DecimalPlaces = 1;
            numRegionTop.Location = new System.Drawing.Point(222, 335);
            numRegionTop.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numRegionTop.Size = new System.Drawing.Size(52, 23);
            numRegionTop.ValueChanged += num_ValueChanged;
            numRegionTop.Name = "numRegionTop";
            numRegionTop.TabIndex = 24;
            // 
            // lblRegionRight
            // 
            lblRegionRight.AutoSize = true;
            lblRegionRight.Location = new System.Drawing.Point(282, 338);
            lblRegionRight.Size = new System.Drawing.Size(100, 15);
            lblRegionRight.Text = "Right";
            lblRegionRight.Name = "lblRegionRight";
            lblRegionRight.TabIndex = 25;
            // 
            // numRegionRight
            // 
            numRegionRight.DecimalPlaces = 1;
            numRegionRight.Location = new System.Drawing.Point(317, 335);
            numRegionRight.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numRegionRight.Size = new System.Drawing.Size(52, 23);
            numRegionRight.ValueChanged += num_ValueChanged;
            numRegionRight.Name = "numRegionRight";
            numRegionRight.TabIndex = 26;
            // 
            // lblRegionBottom
            // 
            lblRegionBottom.AutoSize = true;
            lblRegionBottom.Location = new System.Drawing.Point(370, 338);
            lblRegionBottom.Size = new System.Drawing.Size(100, 15);
            lblRegionBottom.Text = "Bottom";
            lblRegionBottom.Name = "lblRegionBottom";
            lblRegionBottom.TabIndex = 27;
            // 
            // numRegionBottom
            // 
            numRegionBottom.DecimalPlaces = 1;
            numRegionBottom.Location = new System.Drawing.Point(415, 335);
            numRegionBottom.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numRegionBottom.Size = new System.Drawing.Size(52, 23);
            numRegionBottom.ValueChanged += num_ValueChanged;
            numRegionBottom.Name = "numRegionBottom";
            numRegionBottom.TabIndex = 28;
            // 
            // ItemPickupEditor
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(lblHelp);
            Controls.Add(lstLabels);
            Controls.Add(btnLabelCapture);
            Controls.Add(btnLabelEdit);
            Controls.Add(btnLabelRemove);
            Controls.Add(lblTolerance);
            Controls.Add(numTolerance);
            Controls.Add(lblToleranceUnit);
            Controls.Add(lblDelayAfterClick);
            Controls.Add(numDelayAfterClick);
            Controls.Add(lblDelayAfterClickUnit);
            Controls.Add(lblScanInterval);
            Controls.Add(numScanInterval);
            Controls.Add(lblScanIntervalUnit);
            Controls.Add(lblEmptyScans);
            Controls.Add(numEmptyScans);
            Controls.Add(lblEmptyScansUnit);
            Controls.Add(lblMaxClicks);
            Controls.Add(numMaxClicks);
            Controls.Add(lblMaxClicksUnit);
            Controls.Add(lblRegion);
            Controls.Add(lblRegionLeft);
            Controls.Add(numRegionLeft);
            Controls.Add(lblRegionTop);
            Controls.Add(numRegionTop);
            Controls.Add(lblRegionRight);
            Controls.Add(numRegionRight);
            Controls.Add(lblRegionBottom);
            Controls.Add(numRegionBottom);
            Name = "ItemPickupEditor";
            Size = new System.Drawing.Size(467, 365);
            ((System.ComponentModel.ISupportInitialize)numTolerance).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDelayAfterClick).EndInit();
            ((System.ComponentModel.ISupportInitialize)numScanInterval).EndInit();
            ((System.ComponentModel.ISupportInitialize)numEmptyScans).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMaxClicks).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRegionLeft).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRegionTop).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRegionRight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRegionBottom).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.ListView lstLabels;
        private System.Windows.Forms.ColumnHeader colLabelPreview;
        private System.Windows.Forms.ColumnHeader colLabelBackground;
        private System.Windows.Forms.ColumnHeader colLabelText;
        private System.Windows.Forms.Button btnLabelCapture;
        private System.Windows.Forms.Button btnLabelEdit;
        private System.Windows.Forms.Button btnLabelRemove;
        private System.Windows.Forms.Label lblTolerance;
        private System.Windows.Forms.NumericUpDown numTolerance;
        private System.Windows.Forms.Label lblToleranceUnit;
        private System.Windows.Forms.Label lblDelayAfterClick;
        private System.Windows.Forms.NumericUpDown numDelayAfterClick;
        private System.Windows.Forms.Label lblDelayAfterClickUnit;
        private System.Windows.Forms.Label lblScanInterval;
        private System.Windows.Forms.NumericUpDown numScanInterval;
        private System.Windows.Forms.Label lblScanIntervalUnit;
        private System.Windows.Forms.Label lblEmptyScans;
        private System.Windows.Forms.NumericUpDown numEmptyScans;
        private System.Windows.Forms.Label lblEmptyScansUnit;
        private System.Windows.Forms.Label lblMaxClicks;
        private System.Windows.Forms.NumericUpDown numMaxClicks;
        private System.Windows.Forms.Label lblMaxClicksUnit;
        private System.Windows.Forms.Label lblRegion;
        private System.Windows.Forms.Label lblRegionLeft;
        private System.Windows.Forms.NumericUpDown numRegionLeft;
        private System.Windows.Forms.Label lblRegionTop;
        private System.Windows.Forms.NumericUpDown numRegionTop;
        private System.Windows.Forms.Label lblRegionRight;
        private System.Windows.Forms.NumericUpDown numRegionRight;
        private System.Windows.Forms.Label lblRegionBottom;
        private System.Windows.Forms.NumericUpDown numRegionBottom;
    }
}
