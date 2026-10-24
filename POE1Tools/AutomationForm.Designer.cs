namespace POE1Tools
{
    partial class AutomationForm
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
            components = new System.ComponentModel.Container();
            pnlToolbar = new System.Windows.Forms.Panel();
            lblActionPreset = new System.Windows.Forms.Label();
            cboActionPreset = new System.Windows.Forms.ComboBox();
            btnActionNew = new System.Windows.Forms.Button();
            btnActionSave = new System.Windows.Forms.Button();
            btnActionDelete = new System.Windows.Forms.Button();
            btnNewMacro = new System.Windows.Forms.Button();
            btnLinkMacro = new System.Windows.Forms.Button();
            btnDeleteSelected = new System.Windows.Forms.Button();
            btnFit = new System.Windows.Forms.Button();
            lblHint = new System.Windows.Forms.Label();
            canvas = new NodeCanvas();
            pnlProps = new System.Windows.Forms.Panel();
            grpInfo = new System.Windows.Forms.GroupBox();
            lblInfo = new System.Windows.Forms.Label();
            grpNode = new System.Windows.Forms.GroupBox();
            lblNodeName = new System.Windows.Forms.Label();
            txtNodeName = new System.Windows.Forms.TextBox();
            chkEnabled = new System.Windows.Forms.CheckBox();
            chkFinal = new System.Windows.Forms.CheckBox();
            lblNodeType = new System.Windows.Forms.Label();
            cboNodeType = new System.Windows.Forms.ComboBox();
            lblGroupHint = new System.Windows.Forms.Label();
            lblDelay = new System.Windows.Forms.Label();
            txtDelay = new System.Windows.Forms.TextBox();
            lblDelayHelp = new System.Windows.Forms.Label();
            pickupEditor = new ItemPickupEditor();
            lblHotkeys = new System.Windows.Forms.Label();
            lstActions = new System.Windows.Forms.ListView();
            colIndex = new System.Windows.Forms.ColumnHeader();
            colTime = new System.Windows.Forms.ColumnHeader();
            colDelta = new System.Windows.Forms.ColumnHeader();
            colAction = new System.Windows.Forms.ColumnHeader();
            colDetail = new System.Windows.Forms.ColumnHeader();
            btnMacroStepDelete = new System.Windows.Forms.Button();
            grpLinks = new System.Windows.Forms.GroupBox();
            lstLinks = new System.Windows.Forms.ListView();
            colLinkWhen = new System.Windows.Forms.ColumnHeader();
            colLinkTarget = new System.Windows.Forms.ColumnHeader();
            btnLinkEdit = new System.Windows.Forms.Button();
            btnLinkUp = new System.Windows.Forms.Button();
            btnLinkDown = new System.Windows.Forms.Button();
            btnLinkRemove = new System.Windows.Forms.Button();
            lblLinksHelp = new System.Windows.Forms.Label();
            grpLink = new System.Windows.Forms.GroupBox();
            lblLinkFromTo = new System.Windows.Forms.Label();
            lblType = new System.Windows.Forms.Label();
            cboType = new System.Windows.Forms.ComboBox();
            lblValue = new System.Windows.Forms.Label();
            numValue = new System.Windows.Forms.NumericUpDown();
            lblValueUnit = new System.Windows.Forms.Label();
            grpScreen = new System.Windows.Forms.GroupBox();
            lblInterval = new System.Windows.Forms.Label();
            numInterval = new System.Windows.Forms.NumericUpDown();
            lblIntervalUnit = new System.Windows.Forms.Label();
            lblTolerance = new System.Windows.Forms.Label();
            numTolerance = new System.Windows.Forms.NumericUpDown();
            lblToleranceUnit = new System.Windows.Forms.Label();
            lblPercent = new System.Windows.Forms.Label();
            numPercent = new System.Windows.Forms.NumericUpDown();
            lblPercentUnit = new System.Windows.Forms.Label();
            lblSample = new System.Windows.Forms.Label();
            lblRegion = new System.Windows.Forms.Label();
            picSample = new System.Windows.Forms.PictureBox();
            btnCapture = new System.Windows.Forms.Button();
            btnTest = new System.Windows.Forms.Button();
            lblTestResult = new System.Windows.Forms.Label();
            btnLinkDelete = new System.Windows.Forms.Button();
            btnLinkGoTarget = new System.Windows.Forms.Button();
            lblLinkHelp = new System.Windows.Forms.Label();
            pnlStatus = new System.Windows.Forms.Panel();
            lblStatus = new System.Windows.Forms.Label();
            chkStopAfter = new System.Windows.Forms.CheckBox();
            txtStopAfter = new System.Windows.Forms.TextBox();
            lblStopAfterUnit = new System.Windows.Forms.Label();
            chkSleepWhenDone = new System.Windows.Forms.CheckBox();
            mnuCanvas = new System.Windows.Forms.ContextMenuStrip(components);
            mnuNewMacro = new System.Windows.Forms.ToolStripMenuItem();
            mnuPasteMacro = new System.Windows.Forms.ToolStripMenuItem();
            mnuSeparator = new System.Windows.Forms.ToolStripSeparator();
            mnuLinkMacro = new System.Windows.Forms.ToolStripMenuItem();
            mnuCopyMacro = new System.Windows.Forms.ToolStripMenuItem();
            mnuFinal = new System.Windows.Forms.ToolStripMenuItem();
            mnuDisabled = new System.Windows.Forms.ToolStripMenuItem();
            mnuDelete = new System.Windows.Forms.ToolStripMenuItem();
            tmrStatus = new System.Windows.Forms.Timer(components);
            pnlToolbar.SuspendLayout();
            pnlProps.SuspendLayout();
            grpInfo.SuspendLayout();
            grpNode.SuspendLayout();
            grpLinks.SuspendLayout();
            grpLink.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numValue).BeginInit();
            grpScreen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numInterval).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numTolerance).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPercent).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSample).BeginInit();
            pnlStatus.SuspendLayout();
            mnuCanvas.SuspendLayout();
            SuspendLayout();
            //
            // pnlToolbar
            //
            pnlToolbar.Controls.Add(lblActionPreset);
            pnlToolbar.Controls.Add(cboActionPreset);
            pnlToolbar.Controls.Add(btnActionNew);
            pnlToolbar.Controls.Add(btnActionSave);
            pnlToolbar.Controls.Add(btnActionDelete);
            pnlToolbar.Controls.Add(btnNewMacro);
            pnlToolbar.Controls.Add(btnLinkMacro);
            pnlToolbar.Controls.Add(btnDeleteSelected);
            pnlToolbar.Controls.Add(btnFit);
            pnlToolbar.Controls.Add(lblHint);
            pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            pnlToolbar.Location = new System.Drawing.Point(0, 0);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Size = new System.Drawing.Size(1400, 42);
            pnlToolbar.TabIndex = 0;
            //
            // lblActionPreset
            //
            lblActionPreset.AutoSize = true;
            lblActionPreset.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            lblActionPreset.Location = new System.Drawing.Point(10, 13);
            lblActionPreset.Name = "lblActionPreset";
            lblActionPreset.Size = new System.Drawing.Size(46, 15);
            lblActionPreset.TabIndex = 0;
            lblActionPreset.Text = "Action:";
            //
            // cboActionPreset
            //
            cboActionPreset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboActionPreset.FormattingEnabled = true;
            cboActionPreset.Location = new System.Drawing.Point(62, 9);
            cboActionPreset.Name = "cboActionPreset";
            cboActionPreset.Size = new System.Drawing.Size(220, 23);
            cboActionPreset.TabIndex = 1;
            cboActionPreset.SelectedIndexChanged += cboActionPreset_SelectedIndexChanged;
            //
            // btnActionNew
            //
            btnActionNew.Location = new System.Drawing.Point(288, 8);
            btnActionNew.Name = "btnActionNew";
            btnActionNew.Size = new System.Drawing.Size(70, 25);
            btnActionNew.TabIndex = 2;
            btnActionNew.Text = "New";
            btnActionNew.UseVisualStyleBackColor = true;
            btnActionNew.Click += btnActionNew_Click;
            //
            // btnActionSave
            //
            btnActionSave.Enabled = false;
            btnActionSave.Location = new System.Drawing.Point(362, 8);
            btnActionSave.Name = "btnActionSave";
            btnActionSave.Size = new System.Drawing.Size(70, 25);
            btnActionSave.TabIndex = 3;
            btnActionSave.Text = "Save";
            btnActionSave.UseVisualStyleBackColor = true;
            btnActionSave.Click += btnActionSave_Click;
            //
            // btnActionDelete
            //
            btnActionDelete.Enabled = false;
            btnActionDelete.Location = new System.Drawing.Point(436, 8);
            btnActionDelete.Name = "btnActionDelete";
            btnActionDelete.Size = new System.Drawing.Size(70, 25);
            btnActionDelete.TabIndex = 4;
            btnActionDelete.Text = "Delete";
            btnActionDelete.UseVisualStyleBackColor = true;
            btnActionDelete.Click += btnActionDelete_Click;
            //
            // btnNewMacro
            //
            btnNewMacro.Location = new System.Drawing.Point(536, 8);
            btnNewMacro.Name = "btnNewMacro";
            btnNewMacro.Size = new System.Drawing.Size(95, 25);
            btnNewMacro.TabIndex = 5;
            btnNewMacro.Text = "New macro";
            btnNewMacro.UseVisualStyleBackColor = true;
            btnNewMacro.Click += btnNewMacro_Click;
            //
            // btnLinkMacro
            //
            btnLinkMacro.Location = new System.Drawing.Point(635, 8);
            btnLinkMacro.Name = "btnLinkMacro";
            btnLinkMacro.Size = new System.Drawing.Size(95, 25);
            btnLinkMacro.TabIndex = 6;
            btnLinkMacro.Text = "Link macro";
            btnLinkMacro.UseVisualStyleBackColor = true;
            btnLinkMacro.Visible = false;
            btnLinkMacro.Click += btnLinkMacro_Click;
            //
            // btnDeleteSelected
            //
            btnDeleteSelected.Location = new System.Drawing.Point(734, 8);
            btnDeleteSelected.Name = "btnDeleteSelected";
            btnDeleteSelected.Size = new System.Drawing.Size(95, 25);
            btnDeleteSelected.TabIndex = 7;
            btnDeleteSelected.Text = "Delete macro";
            btnDeleteSelected.UseVisualStyleBackColor = true;
            btnDeleteSelected.Visible = false;
            btnDeleteSelected.Click += btnDeleteSelected_Click;
            //
            // btnFit
            //
            btnFit.Location = new System.Drawing.Point(833, 8);
            btnFit.Name = "btnFit";
            btnFit.Size = new System.Drawing.Size(70, 25);
            btnFit.TabIndex = 8;
            btnFit.Text = "Fit view";
            btnFit.UseVisualStyleBackColor = true;
            btnFit.Click += btnFit_Click;
            //
            // lblHint
            //
            lblHint.AutoSize = true;
            lblHint.ForeColor = System.Drawing.SystemColors.GrayText;
            lblHint.Location = new System.Drawing.Point(912, 13);
            lblHint.Name = "lblHint";
            lblHint.Size = new System.Drawing.Size(100, 15);
            lblHint.TabIndex = 9;
            lblHint.Text = "Hint";
            //
            // canvas
            //
            canvas.Dock = System.Windows.Forms.DockStyle.Fill;
            canvas.Location = new System.Drawing.Point(0, 42);
            canvas.Name = "canvas";
            canvas.Size = new System.Drawing.Size(888, 716);
            canvas.TabIndex = 1;
            //
            // pnlProps
            //
            pnlProps.AutoScroll = true;
            pnlProps.BackColor = System.Drawing.SystemColors.Control;
            pnlProps.Controls.Add(grpInfo);
            pnlProps.Controls.Add(grpNode);
            pnlProps.Controls.Add(grpLinks);
            pnlProps.Controls.Add(grpLink);
            pnlProps.Dock = System.Windows.Forms.DockStyle.Right;
            pnlProps.Location = new System.Drawing.Point(888, 42);
            pnlProps.Name = "pnlProps";
            pnlProps.Size = new System.Drawing.Size(512, 716);
            pnlProps.TabIndex = 2;
            //
            // grpInfo
            //
            grpInfo.Controls.Add(lblInfo);
            grpInfo.Location = new System.Drawing.Point(8, 8);
            grpInfo.Name = "grpInfo";
            grpInfo.Size = new System.Drawing.Size(490, 130);
            grpInfo.TabIndex = 0;
            grpInfo.TabStop = false;
            grpInfo.Text = "Selection";
            //
            // lblInfo
            //
            lblInfo.Location = new System.Drawing.Point(10, 22);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new System.Drawing.Size(470, 100);
            lblInfo.TabIndex = 0;
            lblInfo.Text = "Select a macro or a link on the canvas.";
            //
            // grpNode
            //
            grpNode.Controls.Add(lblNodeName);
            grpNode.Controls.Add(txtNodeName);
            grpNode.Controls.Add(chkEnabled);
            grpNode.Controls.Add(chkFinal);
            grpNode.Controls.Add(lblNodeType);
            grpNode.Controls.Add(cboNodeType);
            grpNode.Controls.Add(lblGroupHint);
            grpNode.Controls.Add(lblDelay);
            grpNode.Controls.Add(txtDelay);
            grpNode.Controls.Add(lblDelayHelp);
            grpNode.Controls.Add(pickupEditor);
            grpNode.Controls.Add(lblHotkeys);
            grpNode.Controls.Add(lstActions);
            grpNode.Controls.Add(btnMacroStepDelete);
            grpNode.Location = new System.Drawing.Point(8, 8);
            grpNode.Name = "grpNode";
            grpNode.Size = new System.Drawing.Size(490, 468);
            grpNode.TabIndex = 1;
            grpNode.TabStop = false;
            grpNode.Text = "Macro";
            grpNode.Visible = false;
            //
            // lblNodeName
            //
            lblNodeName.AutoSize = true;
            lblNodeName.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            lblNodeName.Location = new System.Drawing.Point(10, 27);
            lblNodeName.Name = "lblNodeName";
            lblNodeName.Size = new System.Drawing.Size(42, 15);
            lblNodeName.TabIndex = 0;
            lblNodeName.Text = "Name:";
            //
            // txtNodeName
            //
            txtNodeName.Location = new System.Drawing.Point(70, 24);
            txtNodeName.MaxLength = 50;
            txtNodeName.Name = "txtNodeName";
            txtNodeName.Size = new System.Drawing.Size(180, 23);
            txtNodeName.TabIndex = 1;
            txtNodeName.TextChanged += txtNodeName_TextChanged;
            //
            // chkEnabled
            //
            chkEnabled.AutoSize = true;
            chkEnabled.Checked = true;
            chkEnabled.Location = new System.Drawing.Point(260, 26);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new System.Drawing.Size(68, 19);
            chkEnabled.TabIndex = 14;
            chkEnabled.Text = "Enabled";
            chkEnabled.UseVisualStyleBackColor = true;
            chkEnabled.CheckedChanged += chkEnabled_CheckedChanged;
            //
            // chkFinal
            //
            chkFinal.AutoSize = true;
            chkFinal.Location = new System.Drawing.Point(338, 26);
            chkFinal.Name = "chkFinal";
            chkFinal.Size = new System.Drawing.Size(140, 19);
            chkFinal.TabIndex = 2;
            chkFinal.Text = "Final (for \"Stop after\")";
            chkFinal.UseVisualStyleBackColor = true;
            chkFinal.CheckedChanged += chkFinal_CheckedChanged;
            //
            // lblNodeType
            //
            lblNodeType.AutoSize = true;
            lblNodeType.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            lblNodeType.Location = new System.Drawing.Point(10, 56);
            lblNodeType.Name = "lblNodeType";
            lblNodeType.Size = new System.Drawing.Size(35, 15);
            lblNodeType.TabIndex = 3;
            lblNodeType.Text = "Type:";
            //
            // cboNodeType
            //
            cboNodeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboNodeType.FormattingEnabled = true;
            cboNodeType.Location = new System.Drawing.Point(70, 53);
            cboNodeType.Name = "cboNodeType";
            cboNodeType.Size = new System.Drawing.Size(230, 23);
            cboNodeType.TabIndex = 4;
            cboNodeType.SelectedIndexChanged += cboNodeType_SelectedIndexChanged;
            //
            // lblGroupHint
            //
            lblGroupHint.ForeColor = System.Drawing.Color.FromArgb(200, 50, 50);
            lblGroupHint.Location = new System.Drawing.Point(312, 48);
            lblGroupHint.Name = "lblGroupHint";
            lblGroupHint.Size = new System.Drawing.Size(170, 34);
            lblGroupHint.TabIndex = 13;
            lblGroupHint.Text = "";
            lblGroupHint.Visible = false;
            //
            // lblDelay
            //
            lblDelay.AutoSize = true;
            lblDelay.Location = new System.Drawing.Point(10, 88);
            lblDelay.Name = "lblDelay";
            lblDelay.Size = new System.Drawing.Size(66, 15);
            lblDelay.TabIndex = 5;
            lblDelay.Text = "Delay (ms):";
            lblDelay.Visible = false;
            //
            // txtDelay
            //
            txtDelay.Location = new System.Drawing.Point(82, 85);
            txtDelay.MaxLength = 8;
            txtDelay.Name = "txtDelay";
            txtDelay.Size = new System.Drawing.Size(80, 23);
            txtDelay.TabIndex = 6;
            txtDelay.Visible = false;
            txtDelay.WordWrap = false;
            txtDelay.TextChanged += txtDelay_TextChanged;
            //
            // lblDelayHelp
            //
            lblDelayHelp.AutoSize = true;
            lblDelayHelp.Location = new System.Drawing.Point(10, 117);
            lblDelayHelp.Name = "lblDelayHelp";
            lblDelayHelp.Size = new System.Drawing.Size(100, 15);
            lblDelayHelp.TabIndex = 7;
            lblDelayHelp.Text = "Does nothing for this long, then it is finished.";
            lblDelayHelp.Visible = false;
            //
            // pickupEditor
            //
            pickupEditor.Location = new System.Drawing.Point(10, 85);
            pickupEditor.Name = "pickupEditor";
            pickupEditor.Size = new System.Drawing.Size(467, 365);
            pickupEditor.TabIndex = 8;
            pickupEditor.Visible = false;
            //
            // lblHotkeys
            //
            lblHotkeys.AutoSize = true;
            lblHotkeys.Location = new System.Drawing.Point(10, 85);
            lblHotkeys.Name = "lblHotkeys";
            lblHotkeys.Size = new System.Drawing.Size(100, 15);
            lblHotkeys.TabIndex = 9;
            lblHotkeys.Text = "Ctrl + Up: start / stop recording into this macro.\r\nDouble click a time, a key or a click position to change it.";
            //
            // lstActions
            //
            lstActions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colIndex, colTime, colDelta, colAction, colDetail });
            lstActions.FullRowSelect = true;
            lstActions.GridLines = true;
            lstActions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            lstActions.HideSelection = false;
            lstActions.Location = new System.Drawing.Point(10, 122);
            lstActions.MultiSelect = false;
            lstActions.Name = "lstActions";
            lstActions.Size = new System.Drawing.Size(470, 300);
            lstActions.TabIndex = 10;
            lstActions.UseCompatibleStateImageBehavior = false;
            lstActions.View = System.Windows.Forms.View.Details;
            lstActions.SelectedIndexChanged += lstActions_SelectedIndexChanged;
            lstActions.MouseDoubleClick += lstActions_MouseDoubleClick;
            //
            // colIndex
            //
            colIndex.Text = "#";
            colIndex.Width = 40;
            //
            // colTime
            //
            colTime.Text = "Time from start";
            colTime.Width = 95;
            //
            // colDelta
            //
            colDelta.Text = "Time from last step";
            colDelta.Width = 120;
            //
            // colAction
            //
            colAction.Text = "Action";
            colAction.Width = 85;
            //
            // colDetail
            //
            colDetail.Text = "Detail";
            colDetail.Width = 106;
            //
            // btnMacroStepDelete
            //
            btnMacroStepDelete.Enabled = false;
            btnMacroStepDelete.Location = new System.Drawing.Point(10, 428);
            btnMacroStepDelete.Name = "btnMacroStepDelete";
            btnMacroStepDelete.Size = new System.Drawing.Size(90, 25);
            btnMacroStepDelete.TabIndex = 11;
            btnMacroStepDelete.Text = "Delete step";
            btnMacroStepDelete.UseVisualStyleBackColor = true;
            btnMacroStepDelete.Click += btnMacroStepDelete_Click;
            //
            // grpLinks
            //
            grpLinks.Controls.Add(lstLinks);
            grpLinks.Controls.Add(btnLinkEdit);
            grpLinks.Controls.Add(btnLinkUp);
            grpLinks.Controls.Add(btnLinkDown);
            grpLinks.Controls.Add(btnLinkRemove);
            grpLinks.Controls.Add(lblLinksHelp);
            grpLinks.Location = new System.Drawing.Point(8, 484);
            grpLinks.Name = "grpLinks";
            grpLinks.Size = new System.Drawing.Size(490, 228);
            grpLinks.TabIndex = 2;
            grpLinks.TabStop = false;
            grpLinks.Text = "Links from this macro";
            grpLinks.Visible = false;
            //
            // lstLinks
            //
            lstLinks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colLinkWhen, colLinkTarget });
            lstLinks.FullRowSelect = true;
            lstLinks.GridLines = true;
            lstLinks.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            lstLinks.HideSelection = false;
            lstLinks.Location = new System.Drawing.Point(10, 22);
            lstLinks.MultiSelect = false;
            lstLinks.Name = "lstLinks";
            lstLinks.Size = new System.Drawing.Size(380, 150);
            lstLinks.TabIndex = 0;
            lstLinks.UseCompatibleStateImageBehavior = false;
            lstLinks.View = System.Windows.Forms.View.Details;
            lstLinks.SelectedIndexChanged += lstLinks_SelectedIndexChanged;
            lstLinks.DoubleClick += lstLinks_DoubleClick;
            //
            // colLinkWhen
            //
            colLinkWhen.Text = "When";
            colLinkWhen.Width = 235;
            //
            // colLinkTarget
            //
            colLinkTarget.Text = "Then go to";
            colLinkTarget.Width = 140;
            //
            // btnLinkEdit
            //
            btnLinkEdit.Location = new System.Drawing.Point(396, 22);
            btnLinkEdit.Name = "btnLinkEdit";
            btnLinkEdit.Size = new System.Drawing.Size(84, 25);
            btnLinkEdit.TabIndex = 1;
            btnLinkEdit.Text = "Edit";
            btnLinkEdit.UseVisualStyleBackColor = true;
            btnLinkEdit.Click += btnLinkEdit_Click;
            //
            // btnLinkUp
            //
            btnLinkUp.Location = new System.Drawing.Point(396, 52);
            btnLinkUp.Name = "btnLinkUp";
            btnLinkUp.Size = new System.Drawing.Size(84, 25);
            btnLinkUp.TabIndex = 2;
            btnLinkUp.Text = "Move up";
            btnLinkUp.UseVisualStyleBackColor = true;
            btnLinkUp.Click += btnLinkUp_Click;
            //
            // btnLinkDown
            //
            btnLinkDown.Location = new System.Drawing.Point(396, 82);
            btnLinkDown.Name = "btnLinkDown";
            btnLinkDown.Size = new System.Drawing.Size(84, 25);
            btnLinkDown.TabIndex = 3;
            btnLinkDown.Text = "Move down";
            btnLinkDown.UseVisualStyleBackColor = true;
            btnLinkDown.Click += btnLinkDown_Click;
            //
            // btnLinkRemove
            //
            btnLinkRemove.Location = new System.Drawing.Point(396, 117);
            btnLinkRemove.Name = "btnLinkRemove";
            btnLinkRemove.Size = new System.Drawing.Size(84, 25);
            btnLinkRemove.TabIndex = 4;
            btnLinkRemove.Text = "Remove";
            btnLinkRemove.UseVisualStyleBackColor = true;
            btnLinkRemove.Click += btnLinkRemove_Click;
            //
            // lblLinksHelp
            //
            lblLinksHelp.Location = new System.Drawing.Point(10, 178);
            lblLinksHelp.Name = "lblLinksHelp";
            lblLinksHelp.Size = new System.Drawing.Size(470, 36);
            lblLinksHelp.TabIndex = 5;
            lblLinksHelp.Text = "Checked in this order while the macro plays. \"Link macro\" adds one.\r\nWithout any link, the macro loops forever.";
            //
            // grpLink
            //
            grpLink.Controls.Add(lblLinkFromTo);
            grpLink.Controls.Add(lblType);
            grpLink.Controls.Add(cboType);
            grpLink.Controls.Add(lblValue);
            grpLink.Controls.Add(numValue);
            grpLink.Controls.Add(lblValueUnit);
            grpLink.Controls.Add(grpScreen);
            grpLink.Controls.Add(btnLinkDelete);
            grpLink.Controls.Add(btnLinkGoTarget);
            grpLink.Controls.Add(lblLinkHelp);
            grpLink.Location = new System.Drawing.Point(8, 8);
            grpLink.Name = "grpLink";
            grpLink.Size = new System.Drawing.Size(490, 520);
            grpLink.TabIndex = 3;
            grpLink.TabStop = false;
            grpLink.Text = "Link";
            grpLink.Visible = false;
            //
            // lblLinkFromTo
            //
            lblLinkFromTo.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            lblLinkFromTo.Location = new System.Drawing.Point(10, 22);
            lblLinkFromTo.Name = "lblLinkFromTo";
            lblLinkFromTo.Size = new System.Drawing.Size(470, 18);
            lblLinkFromTo.TabIndex = 0;
            lblLinkFromTo.Text = "From ... to ...";
            //
            // lblType
            //
            lblType.AutoSize = true;
            lblType.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            lblType.Location = new System.Drawing.Point(10, 52);
            lblType.Name = "lblType";
            lblType.Size = new System.Drawing.Size(41, 15);
            lblType.TabIndex = 1;
            lblType.Text = "When:";
            //
            // cboType
            //
            cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboType.FormattingEnabled = true;
            cboType.Location = new System.Drawing.Point(70, 49);
            cboType.Name = "cboType";
            cboType.Size = new System.Drawing.Size(410, 23);
            cboType.TabIndex = 2;
            cboType.SelectedIndexChanged += cboType_SelectedIndexChanged;
            //
            // lblValue
            //
            lblValue.AutoSize = true;
            lblValue.Location = new System.Drawing.Point(22, 82);
            lblValue.Name = "lblValue";
            lblValue.Size = new System.Drawing.Size(100, 15);
            lblValue.TabIndex = 3;
            lblValue.Text = "Number of loops:";
            //
            // numValue
            //
            numValue.Location = new System.Drawing.Point(192, 79);
            numValue.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numValue.Name = "numValue";
            numValue.Size = new System.Drawing.Size(80, 23);
            numValue.TabIndex = 4;
            numValue.Value = new decimal(new int[] { 5, 0, 0, 0 });
            numValue.ValueChanged += numValue_ValueChanged;
            //
            // lblValueUnit
            //
            lblValueUnit.AutoSize = true;
            lblValueUnit.Location = new System.Drawing.Point(278, 82);
            lblValueUnit.Name = "lblValueUnit";
            lblValueUnit.Size = new System.Drawing.Size(36, 15);
            lblValueUnit.TabIndex = 5;
            lblValueUnit.Text = "times";
            //
            // grpScreen
            //
            grpScreen.Controls.Add(lblInterval);
            grpScreen.Controls.Add(numInterval);
            grpScreen.Controls.Add(lblIntervalUnit);
            grpScreen.Controls.Add(lblTolerance);
            grpScreen.Controls.Add(numTolerance);
            grpScreen.Controls.Add(lblToleranceUnit);
            grpScreen.Controls.Add(lblPercent);
            grpScreen.Controls.Add(numPercent);
            grpScreen.Controls.Add(lblPercentUnit);
            grpScreen.Controls.Add(lblSample);
            grpScreen.Controls.Add(lblRegion);
            grpScreen.Controls.Add(picSample);
            grpScreen.Controls.Add(btnCapture);
            grpScreen.Controls.Add(btnTest);
            grpScreen.Controls.Add(lblTestResult);
            grpScreen.Location = new System.Drawing.Point(10, 110);
            grpScreen.Name = "grpScreen";
            grpScreen.Size = new System.Drawing.Size(470, 318);
            grpScreen.TabIndex = 6;
            grpScreen.TabStop = false;
            grpScreen.Text = "Screen capture";
            //
            // lblInterval
            //
            lblInterval.AutoSize = true;
            lblInterval.Location = new System.Drawing.Point(10, 25);
            lblInterval.Name = "lblInterval";
            lblInterval.Size = new System.Drawing.Size(126, 15);
            lblInterval.TabIndex = 0;
            lblInterval.Text = "Capture the area every:";
            //
            // numInterval
            //
            numInterval.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            numInterval.Location = new System.Drawing.Point(180, 22);
            numInterval.Maximum = new decimal(new int[] { 3600000, 0, 0, 0 });
            numInterval.Minimum = new decimal(new int[] { 20, 0, 0, 0 });
            numInterval.Name = "numInterval";
            numInterval.Size = new System.Drawing.Size(80, 23);
            numInterval.TabIndex = 1;
            numInterval.Value = new decimal(new int[] { 500, 0, 0, 0 });
            numInterval.ValueChanged += numScreen_ValueChanged;
            //
            // lblIntervalUnit
            //
            lblIntervalUnit.AutoSize = true;
            lblIntervalUnit.Location = new System.Drawing.Point(266, 25);
            lblIntervalUnit.Name = "lblIntervalUnit";
            lblIntervalUnit.Size = new System.Drawing.Size(23, 15);
            lblIntervalUnit.TabIndex = 2;
            lblIntervalUnit.Text = "ms";
            //
            // lblTolerance
            //
            lblTolerance.AutoSize = true;
            lblTolerance.Location = new System.Drawing.Point(10, 54);
            lblTolerance.Name = "lblTolerance";
            lblTolerance.Size = new System.Drawing.Size(164, 15);
            lblTolerance.TabIndex = 3;
            lblTolerance.Text = "Color tolerance of a pixel:";
            //
            // numTolerance
            //
            numTolerance.Location = new System.Drawing.Point(180, 51);
            numTolerance.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numTolerance.Name = "numTolerance";
            numTolerance.Size = new System.Drawing.Size(80, 23);
            numTolerance.TabIndex = 4;
            numTolerance.Value = new decimal(new int[] { 20, 0, 0, 0 });
            numTolerance.ValueChanged += numScreen_ValueChanged;
            //
            // lblToleranceUnit
            //
            lblToleranceUnit.AutoSize = true;
            lblToleranceUnit.Location = new System.Drawing.Point(266, 54);
            lblToleranceUnit.Name = "lblToleranceUnit";
            lblToleranceUnit.Size = new System.Drawing.Size(129, 15);
            lblToleranceUnit.TabIndex = 5;
            lblToleranceUnit.Text = "per R, G, B  (0 - 255)";
            //
            // lblPercent
            //
            lblPercent.AutoSize = true;
            lblPercent.Location = new System.Drawing.Point(10, 83);
            lblPercent.Name = "lblPercent";
            lblPercent.Size = new System.Drawing.Size(115, 15);
            lblPercent.TabIndex = 6;
            lblPercent.Text = "It matches at least:";
            //
            // numPercent
            //
            numPercent.Location = new System.Drawing.Point(180, 80);
            numPercent.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPercent.Name = "numPercent";
            numPercent.Size = new System.Drawing.Size(80, 23);
            numPercent.TabIndex = 7;
            numPercent.Value = new decimal(new int[] { 90, 0, 0, 0 });
            numPercent.ValueChanged += numScreen_ValueChanged;
            //
            // lblPercentUnit
            //
            lblPercentUnit.AutoSize = true;
            lblPercentUnit.Location = new System.Drawing.Point(266, 83);
            lblPercentUnit.Name = "lblPercentUnit";
            lblPercentUnit.Size = new System.Drawing.Size(98, 15);
            lblPercentUnit.TabIndex = 8;
            lblPercentUnit.Text = "% of the pixels";
            //
            // lblSample
            //
            lblSample.AutoSize = true;
            lblSample.Location = new System.Drawing.Point(10, 112);
            lblSample.Name = "lblSample";
            lblSample.Size = new System.Drawing.Size(49, 15);
            lblSample.TabIndex = 9;
            lblSample.Text = "Sample:";
            //
            // lblRegion
            //
            lblRegion.Location = new System.Drawing.Point(122, 112);
            lblRegion.Name = "lblRegion";
            lblRegion.Size = new System.Drawing.Size(338, 15);
            lblRegion.TabIndex = 10;
            lblRegion.Text = "No sample yet";
            lblRegion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            //
            // picSample
            //
            picSample.BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
            picSample.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picSample.Location = new System.Drawing.Point(10, 132);
            picSample.Name = "picSample";
            picSample.Size = new System.Drawing.Size(450, 142);
            picSample.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picSample.TabIndex = 11;
            picSample.TabStop = false;
            //
            // btnCapture
            //
            btnCapture.Location = new System.Drawing.Point(10, 282);
            btnCapture.Name = "btnCapture";
            btnCapture.Size = new System.Drawing.Size(106, 25);
            btnCapture.TabIndex = 12;
            btnCapture.Text = "Capture sample...";
            btnCapture.UseVisualStyleBackColor = true;
            btnCapture.Click += btnCapture_Click;
            //
            // btnTest
            //
            btnTest.Location = new System.Drawing.Point(122, 282);
            btnTest.Name = "btnTest";
            btnTest.Size = new System.Drawing.Size(90, 25);
            btnTest.TabIndex = 13;
            btnTest.Text = "Test now";
            btnTest.UseVisualStyleBackColor = true;
            btnTest.Click += btnTest_Click;
            //
            // lblTestResult
            //
            lblTestResult.Location = new System.Drawing.Point(218, 287);
            lblTestResult.Name = "lblTestResult";
            lblTestResult.Size = new System.Drawing.Size(242, 15);
            lblTestResult.TabIndex = 14;
            //
            // btnLinkDelete
            //
            btnLinkDelete.Location = new System.Drawing.Point(10, 440);
            btnLinkDelete.Name = "btnLinkDelete";
            btnLinkDelete.Size = new System.Drawing.Size(100, 25);
            btnLinkDelete.TabIndex = 7;
            btnLinkDelete.Text = "Delete link";
            btnLinkDelete.UseVisualStyleBackColor = true;
            btnLinkDelete.Click += btnLinkDelete_Click;
            //
            // btnLinkGoTarget
            //
            btnLinkGoTarget.Location = new System.Drawing.Point(116, 440);
            btnLinkGoTarget.Name = "btnLinkGoTarget";
            btnLinkGoTarget.Size = new System.Drawing.Size(150, 25);
            btnLinkGoTarget.TabIndex = 8;
            btnLinkGoTarget.Text = "Select the target";
            btnLinkGoTarget.UseVisualStyleBackColor = true;
            btnLinkGoTarget.Click += btnLinkGoTarget_Click;
            //
            // lblLinkHelp
            //
            lblLinkHelp.Location = new System.Drawing.Point(10, 474);
            lblLinkHelp.Name = "lblLinkHelp";
            lblLinkHelp.Size = new System.Drawing.Size(470, 40);
            lblLinkHelp.TabIndex = 9;
            lblLinkHelp.Text = "The condition is checked while the macro plays. When it is met, the action goes to the target of the link.";
            //
            // pnlStatus
            //
            pnlStatus.Controls.Add(lblStatus);
            pnlStatus.Controls.Add(chkStopAfter);
            pnlStatus.Controls.Add(txtStopAfter);
            pnlStatus.Controls.Add(lblStopAfterUnit);
            pnlStatus.Controls.Add(chkSleepWhenDone);
            pnlStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlStatus.Location = new System.Drawing.Point(0, 758);
            pnlStatus.Name = "pnlStatus";
            pnlStatus.Size = new System.Drawing.Size(1400, 62);
            pnlStatus.TabIndex = 3;
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            lblStatus.Location = new System.Drawing.Point(12, 6);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(1080, 50);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "Status";
            //
            // chkStopAfter
            //
            chkStopAfter.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            chkStopAfter.AutoSize = true;
            chkStopAfter.Location = new System.Drawing.Point(1110, 9);
            chkStopAfter.Name = "chkStopAfter";
            chkStopAfter.Size = new System.Drawing.Size(80, 19);
            chkStopAfter.TabIndex = 1;
            chkStopAfter.Text = "Stop after:";
            chkStopAfter.UseVisualStyleBackColor = true;
            chkStopAfter.CheckedChanged += chkStopAfter_CheckedChanged;
            //
            // txtStopAfter
            //
            txtStopAfter.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            txtStopAfter.Location = new System.Drawing.Point(1196, 7);
            txtStopAfter.MaxLength = 6;
            txtStopAfter.Name = "txtStopAfter";
            txtStopAfter.Size = new System.Drawing.Size(60, 23);
            txtStopAfter.TabIndex = 2;
            txtStopAfter.Text = "10";
            txtStopAfter.WordWrap = false;
            txtStopAfter.TextChanged += txtStopAfter_TextChanged;
            //
            // lblStopAfterUnit
            //
            lblStopAfterUnit.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            lblStopAfterUnit.AutoSize = true;
            lblStopAfterUnit.Location = new System.Drawing.Point(1262, 10);
            lblStopAfterUnit.Name = "lblStopAfterUnit";
            lblStopAfterUnit.Size = new System.Drawing.Size(120, 15);
            lblStopAfterUnit.TabIndex = 3;
            lblStopAfterUnit.Text = "times of a final macro";
            //
            // chkSleepWhenDone
            //
            chkSleepWhenDone.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            chkSleepWhenDone.AutoSize = true;
            chkSleepWhenDone.Location = new System.Drawing.Point(1110, 35);
            chkSleepWhenDone.Name = "chkSleepWhenDone";
            chkSleepWhenDone.Size = new System.Drawing.Size(115, 19);
            chkSleepWhenDone.TabIndex = 4;
            chkSleepWhenDone.Text = "Sleep when done";
            chkSleepWhenDone.UseVisualStyleBackColor = true;
            chkSleepWhenDone.CheckedChanged += chkSleepWhenDone_CheckedChanged;
            //
            // mnuCanvas
            //
            mnuCanvas.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuNewMacro, mnuPasteMacro, mnuSeparator, mnuLinkMacro, mnuCopyMacro, mnuFinal, mnuDisabled, mnuDelete });
            mnuCanvas.Name = "mnuCanvas";
            mnuCanvas.Size = new System.Drawing.Size(180, 148);
            //
            // mnuNewMacro
            //
            mnuNewMacro.Name = "mnuNewMacro";
            mnuNewMacro.Size = new System.Drawing.Size(179, 22);
            mnuNewMacro.Text = "New macro";
            mnuNewMacro.Click += mnuNewMacro_Click;
            //
            // mnuPasteMacro
            //
            mnuPasteMacro.Name = "mnuPasteMacro";
            mnuPasteMacro.Size = new System.Drawing.Size(179, 22);
            mnuPasteMacro.Text = "Paste macro";
            mnuPasteMacro.Click += mnuPasteMacro_Click;
            //
            // mnuSeparator
            //
            mnuSeparator.Name = "mnuSeparator";
            mnuSeparator.Size = new System.Drawing.Size(176, 6);
            //
            // mnuLinkMacro
            //
            mnuLinkMacro.Name = "mnuLinkMacro";
            mnuLinkMacro.Size = new System.Drawing.Size(179, 22);
            mnuLinkMacro.Text = "Link macro";
            mnuLinkMacro.Click += btnLinkMacro_Click;
            //
            // mnuCopyMacro
            //
            mnuCopyMacro.Name = "mnuCopyMacro";
            mnuCopyMacro.Size = new System.Drawing.Size(179, 22);
            mnuCopyMacro.Text = "Copy macro";
            mnuCopyMacro.Click += mnuCopyMacro_Click;
            //
            // mnuFinal
            //
            mnuFinal.CheckOnClick = true;
            mnuFinal.Name = "mnuFinal";
            mnuFinal.Size = new System.Drawing.Size(179, 22);
            mnuFinal.Text = "Final step (for \"Stop after\")";
            mnuFinal.Click += mnuFinal_Click;
            //
            // mnuDisabled
            //
            mnuDisabled.CheckOnClick = true;
            mnuDisabled.Name = "mnuDisabled";
            mnuDisabled.Size = new System.Drawing.Size(179, 22);
            mnuDisabled.Text = "Disabled";
            mnuDisabled.Click += mnuDisabled_Click;
            //
            // mnuDelete
            //
            mnuDelete.Name = "mnuDelete";
            mnuDelete.Size = new System.Drawing.Size(179, 22);
            mnuDelete.Text = "Delete";
            mnuDelete.Click += btnDeleteSelected_Click;
            //
            // tmrStatus
            //
            tmrStatus.Interval = 100;
            tmrStatus.Tick += tmrStatus_Tick;
            //
            // AutomationForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1400, 820);
            Controls.Add(canvas);
            Controls.Add(pnlProps);
            Controls.Add(pnlStatus);
            Controls.Add(pnlToolbar);
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(1100, 700);
            Name = "AutomationForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Setup automation";
            FormClosing += AutomationForm_FormClosing;
            FormClosed += AutomationForm_FormClosed;
            Load += AutomationForm_Load;
            pnlToolbar.ResumeLayout(false);
            pnlToolbar.PerformLayout();
            pnlProps.ResumeLayout(false);
            grpInfo.ResumeLayout(false);
            grpNode.ResumeLayout(false);
            grpNode.PerformLayout();
            grpLinks.ResumeLayout(false);
            grpLink.ResumeLayout(false);
            grpLink.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numValue).EndInit();
            grpScreen.ResumeLayout(false);
            grpScreen.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numInterval).EndInit();
            ((System.ComponentModel.ISupportInitialize)numTolerance).EndInit();
            ((System.ComponentModel.ISupportInitialize)numPercent).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSample).EndInit();
            pnlStatus.ResumeLayout(false);
            pnlStatus.PerformLayout();
            mnuCanvas.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Label lblActionPreset;
        private System.Windows.Forms.ComboBox cboActionPreset;
        private System.Windows.Forms.Button btnActionNew;
        private System.Windows.Forms.Button btnActionSave;
        private System.Windows.Forms.Button btnActionDelete;
        private System.Windows.Forms.Button btnNewMacro;
        private System.Windows.Forms.Button btnLinkMacro;
        private System.Windows.Forms.Button btnDeleteSelected;
        private System.Windows.Forms.Button btnFit;
        private System.Windows.Forms.Label lblHint;
        private NodeCanvas canvas;
        private System.Windows.Forms.Panel pnlProps;
        private System.Windows.Forms.GroupBox grpInfo;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.GroupBox grpNode;
        private System.Windows.Forms.Label lblNodeName;
        private System.Windows.Forms.TextBox txtNodeName;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.CheckBox chkFinal;
        private System.Windows.Forms.Label lblNodeType;
        private System.Windows.Forms.ComboBox cboNodeType;
        private System.Windows.Forms.Label lblGroupHint;
        private System.Windows.Forms.Label lblDelay;
        private System.Windows.Forms.TextBox txtDelay;
        private System.Windows.Forms.Label lblDelayHelp;
        private ItemPickupEditor pickupEditor;
        private System.Windows.Forms.Label lblHotkeys;
        private System.Windows.Forms.ListView lstActions;
        private System.Windows.Forms.ColumnHeader colIndex;
        private System.Windows.Forms.ColumnHeader colTime;
        private System.Windows.Forms.ColumnHeader colDelta;
        private System.Windows.Forms.ColumnHeader colAction;
        private System.Windows.Forms.ColumnHeader colDetail;
        private System.Windows.Forms.Button btnMacroStepDelete;
        private System.Windows.Forms.GroupBox grpLinks;
        private System.Windows.Forms.ListView lstLinks;
        private System.Windows.Forms.ColumnHeader colLinkWhen;
        private System.Windows.Forms.ColumnHeader colLinkTarget;
        private System.Windows.Forms.Button btnLinkEdit;
        private System.Windows.Forms.Button btnLinkUp;
        private System.Windows.Forms.Button btnLinkDown;
        private System.Windows.Forms.Button btnLinkRemove;
        private System.Windows.Forms.Label lblLinksHelp;
        private System.Windows.Forms.GroupBox grpLink;
        private System.Windows.Forms.Label lblLinkFromTo;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox cboType;
        private System.Windows.Forms.Label lblValue;
        private System.Windows.Forms.NumericUpDown numValue;
        private System.Windows.Forms.Label lblValueUnit;
        private System.Windows.Forms.GroupBox grpScreen;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.NumericUpDown numInterval;
        private System.Windows.Forms.Label lblIntervalUnit;
        private System.Windows.Forms.Label lblTolerance;
        private System.Windows.Forms.NumericUpDown numTolerance;
        private System.Windows.Forms.Label lblToleranceUnit;
        private System.Windows.Forms.Label lblPercent;
        private System.Windows.Forms.NumericUpDown numPercent;
        private System.Windows.Forms.Label lblPercentUnit;
        private System.Windows.Forms.Label lblSample;
        private System.Windows.Forms.Label lblRegion;
        private System.Windows.Forms.PictureBox picSample;
        private System.Windows.Forms.Button btnCapture;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Label lblTestResult;
        private System.Windows.Forms.Button btnLinkDelete;
        private System.Windows.Forms.Button btnLinkGoTarget;
        private System.Windows.Forms.Label lblLinkHelp;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.CheckBox chkStopAfter;
        private System.Windows.Forms.TextBox txtStopAfter;
        private System.Windows.Forms.Label lblStopAfterUnit;
        private System.Windows.Forms.CheckBox chkSleepWhenDone;
        private System.Windows.Forms.ContextMenuStrip mnuCanvas;
        private System.Windows.Forms.ToolStripMenuItem mnuNewMacro;
        private System.Windows.Forms.ToolStripMenuItem mnuPasteMacro;
        private System.Windows.Forms.ToolStripSeparator mnuSeparator;
        private System.Windows.Forms.ToolStripMenuItem mnuLinkMacro;
        private System.Windows.Forms.ToolStripMenuItem mnuCopyMacro;
        private System.Windows.Forms.ToolStripMenuItem mnuFinal;
        private System.Windows.Forms.ToolStripMenuItem mnuDisabled;
        private System.Windows.Forms.ToolStripMenuItem mnuDelete;
        private System.Windows.Forms.Timer tmrStatus;
    }
}
