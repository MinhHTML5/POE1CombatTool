namespace POE1Tools
{
    partial class CaptureForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblInstruction = new System.Windows.Forms.Label();
            picPreview = new System.Windows.Forms.PictureBox();
            lblX = new System.Windows.Forms.Label();
            numX = new System.Windows.Forms.NumericUpDown();
            lblY = new System.Windows.Forms.Label();
            numY = new System.Windows.Forms.NumericUpDown();
            lblWidth = new System.Windows.Forms.Label();
            numWidth = new System.Windows.Forms.NumericUpDown();
            lblHeight = new System.Windows.Forms.Label();
            numHeight = new System.Windows.Forms.NumericUpDown();
            btnSave = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).BeginInit();
            SuspendLayout();
            //
            // lblInstruction
            //
            lblInstruction.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblInstruction.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            lblInstruction.Location = new System.Drawing.Point(12, 9);
            lblInstruction.Name = "lblInstruction";
            lblInstruction.Size = new System.Drawing.Size(960, 20);
            lblInstruction.TabIndex = 0;
            lblInstruction.Text = "Go to the game, then press Ctrl + Right to take a screenshot of the main monitor.";
            //
            // picPreview
            //
            picPreview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            picPreview.BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
            picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picPreview.Cursor = System.Windows.Forms.Cursors.Cross;
            picPreview.Location = new System.Drawing.Point(12, 34);
            picPreview.Name = "picPreview";
            picPreview.Size = new System.Drawing.Size(960, 540);
            picPreview.TabIndex = 1;
            picPreview.TabStop = false;
            picPreview.SizeChanged += picPreview_SizeChanged;
            picPreview.Paint += picPreview_Paint;
            picPreview.MouseDown += picPreview_MouseDown;
            picPreview.MouseMove += picPreview_MouseMove;
            picPreview.MouseUp += picPreview_MouseUp;
            //
            // lblX
            //
            lblX.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblX.AutoSize = true;
            lblX.Location = new System.Drawing.Point(12, 588);
            lblX.Name = "lblX";
            lblX.Size = new System.Drawing.Size(17, 15);
            lblX.TabIndex = 2;
            lblX.Text = "X:";
            //
            // numX
            //
            numX.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            numX.Location = new System.Drawing.Point(35, 585);
            numX.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numX.Name = "numX";
            numX.Size = new System.Drawing.Size(65, 23);
            numX.TabIndex = 3;
            numX.ValueChanged += numSelection_ValueChanged;
            //
            // lblY
            //
            lblY.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblY.AutoSize = true;
            lblY.Location = new System.Drawing.Point(112, 588);
            lblY.Name = "lblY";
            lblY.Size = new System.Drawing.Size(17, 15);
            lblY.TabIndex = 4;
            lblY.Text = "Y:";
            //
            // numY
            //
            numY.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            numY.Location = new System.Drawing.Point(135, 585);
            numY.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numY.Name = "numY";
            numY.Size = new System.Drawing.Size(65, 23);
            numY.TabIndex = 5;
            numY.ValueChanged += numSelection_ValueChanged;
            //
            // lblWidth
            //
            lblWidth.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblWidth.AutoSize = true;
            lblWidth.Location = new System.Drawing.Point(212, 588);
            lblWidth.Name = "lblWidth";
            lblWidth.Size = new System.Drawing.Size(42, 15);
            lblWidth.TabIndex = 6;
            lblWidth.Text = "Width:";
            //
            // numWidth
            //
            numWidth.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            numWidth.Location = new System.Drawing.Point(260, 585);
            numWidth.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numWidth.Name = "numWidth";
            numWidth.Size = new System.Drawing.Size(65, 23);
            numWidth.TabIndex = 7;
            numWidth.ValueChanged += numSelection_ValueChanged;
            //
            // lblHeight
            //
            lblHeight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblHeight.AutoSize = true;
            lblHeight.Location = new System.Drawing.Point(337, 588);
            lblHeight.Name = "lblHeight";
            lblHeight.Size = new System.Drawing.Size(46, 15);
            lblHeight.TabIndex = 8;
            lblHeight.Text = "Height:";
            //
            // numHeight
            //
            numHeight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            numHeight.Location = new System.Drawing.Point(389, 585);
            numHeight.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numHeight.Name = "numHeight";
            numHeight.Size = new System.Drawing.Size(65, 23);
            numHeight.TabIndex = 9;
            numHeight.ValueChanged += numSelection_ValueChanged;
            //
            // btnSave
            //
            btnSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnSave.Enabled = false;
            btnSave.Location = new System.Drawing.Point(756, 583);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(120, 27);
            btnSave.TabIndex = 10;
            btnSave.Text = "Save as sample";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            //
            // btnCancel
            //
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(882, 583);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(90, 27);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // CaptureForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(984, 621);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(numHeight);
            Controls.Add(lblHeight);
            Controls.Add(numWidth);
            Controls.Add(lblWidth);
            Controls.Add(numY);
            Controls.Add(lblY);
            Controls.Add(numX);
            Controls.Add(lblX);
            Controls.Add(picPreview);
            Controls.Add(lblInstruction);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(640, 400);
            Name = "CaptureForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Capture sample";
            FormClosed += CaptureForm_FormClosed;
            Load += CaptureForm_Load;
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)numX).EndInit();
            ((System.ComponentModel.ISupportInitialize)numY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblInstruction;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Label lblX;
        private System.Windows.Forms.NumericUpDown numX;
        private System.Windows.Forms.Label lblY;
        private System.Windows.Forms.NumericUpDown numY;
        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.NumericUpDown numWidth;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.NumericUpDown numHeight;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}
