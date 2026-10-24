using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using POE1Tools.Modules;
using POE1Tools.Utilities;

namespace POE1Tools
{
    // The automation window. An action preset is a canvas of nodes: the Start, the End, and the macros,
    // linked together by conditions. The right panel edits what is selected on the canvas:
    // a macro (its type, its settings, and the links leaving it) or a link (its condition).
    public partial class AutomationForm : Form, IMessageFilter
    {
        // Columns of lstActions that can be edited with a double click
        private const int COLUMN_TIME = 1;
        private const int COLUMN_DELTA = 2;
        private const int COLUMN_ACTION = 3;
        private const int COLUMN_DETAIL = 4;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const string HINT_DEFAULT = "Wheel: zoom.   Drag the background: pan.   Right click: menu.";
        private const string HINT_LINKING = "Click the macro to link to. Right click or Esc cancels.";

        // The macro taken by "Copy macro", with the links leaving it. They stay there even if the window is closed
        private static ActionNode _copiedNode;
        private static List<CopiedLink> _copiedLinks = new List<CopiedLink>();

        // A link of the copied macro, and what it was pointing at, to find the target again when pasting elsewhere
        private class CopiedLink
        {
            public ActionLink Link;
            public ActionNodeKind ToKind;
            public string ToName;
        }

        private AutomationModule _automationModule;
        private MacroPresetStore _store;
        // The action preset as it is saved in the store, and the copy that is being edited on the canvas
        private ActionPreset _activeAction;
        private ActionPreset _workingAction;
        // The macro node whose recording is loaded in the module. Recording and edits go there
        private ActionNode _macroNode;
        // Set while the code itself fills the controls
        private bool _updatingSelection = false;
        // Set while the module is given a macro, so its MacroChanged is not written back to the node
        private bool _loadingMacro = false;
        // The sample of the selected link, shown in picSample
        private Bitmap _sample;
        // Where the canvas was right clicked, for "New macro" / "Paste macro"
        private PointF _contextWorldPoint;
        // Index of a step of the running plan -> id of its node
        private List<string> _planNodeIds = new List<string>();
        private int _shownAction = -1;
        // The node selected on the canvas because the action moved to it, so it is only selected once
        private string _shownRunningId;
        // The box shown over a time of lstActions while it is being edited
        private TextBox _timeEditor;
        private int _timeEditorRow;
        private int _timeEditorColumn;
        // Set while the countdown of the module is written in txtStopAfter
        private bool _updatingStopAfter = false;

        public AutomationForm(AutomationModule automationModule)
        {
            _automationModule = automationModule;
            InitializeComponent();
        }

        private void AutomationForm_Load(object sender, EventArgs e)
        {
            _automationModule.StateChanged += OnModuleStateChanged;
            _automationModule.MacroChanged += OnModuleMacroChanged;
            _automationModule.ActionRecorded += OnModuleActionRecorded;
            _automationModule.ActionPlanProvider = BuildActionPlan;
            _automationModule.SetWindowOpen(true);

            cboNodeType.Items.AddRange(ActionNode.TypeNames);
            cboType.Items.AddRange(ActionCondition.TypeNames);
            pickupEditor.SettingsChanged += OnPickupSettingsChanged;

            canvas.SelectionChanged += OnCanvasSelectionChanged;
            canvas.GraphChanged += OnCanvasGraphChanged;
            canvas.LinkRequested += OnCanvasLinkRequested;
            canvas.ContextMenuRequested += OnCanvasContextMenuRequested;
            canvas.DeleteRequested += DeleteSelected;
            canvas.LinkModeChanged += OnCanvasLinkModeChanged;
            canvas.ViewChanged += UpdateHint;
            UpdateHint();

            _store = MacroPresetStore.Load();
            txtStopAfter.Text = _store.StopAfterCount.ToString();
            chkStopAfter.Checked = _store.StopAfterEnabled;
            chkSleepWhenDone.Checked = _store.SleepWhenDone;
            ApplyStopAfter();
            RefreshActionPresetList();

            ActionPreset action = _store.FindAction(_store.SelectedActionPreset);
            if (action == null && _store.ActionPresets.Count > 0) action = _store.ActionPresets[0];
            ActivateAction(action);

            // The wheel zooms the canvas even when another control has the focus
            Application.AddMessageFilter(this);
            tmrStatus.Start();
        }

        private void AutomationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _automationModule.StopAll();
            if (!ConfirmUnsavedActionChanges())
            {
                e.Cancel = true;
            }
        }

        private void AutomationForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.RemoveMessageFilter(this);
            tmrStatus.Stop();
            _automationModule.StateChanged -= OnModuleStateChanged;
            _automationModule.MacroChanged -= OnModuleMacroChanged;
            _automationModule.ActionRecorded -= OnModuleActionRecorded;
            _automationModule.ActionPlanProvider = null;
            _automationModule.SetWindowOpen(false);
            picSample.Image = null;
            _sample?.Dispose();
            _sample = null;

            // Remember the "Stop after" option for the next time
            if (_store != null && (_store.StopAfterEnabled != chkStopAfter.Checked || _store.StopAfterCount != GetStopAfterCount()
                || _store.SleepWhenDone != chkSleepWhenDone.Checked))
            {
                _store.StopAfterEnabled = chkStopAfter.Checked;
                _store.SleepWhenDone = chkSleepWhenDone.Checked;
                if (GetStopAfterCount() > 0) _store.StopAfterCount = GetStopAfterCount();
                _store.Save();
            }
        }

        // Zoom the canvas with the wheel wherever the focus is, as long as the cursor is over it
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg != WM_MOUSEWHEEL || !canvas.IsHandleCreated || !canvas.Visible || Form.ActiveForm != this) return false;

            Point client = canvas.PointToClient(Cursor.Position);
            if (!canvas.ClientRectangle.Contains(client)) return false;

            int delta = (short)((m.WParam.ToInt64() >> 16) & 0xFFFF);
            canvas.ZoomAt(client, delta);
            return true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape && canvas.IsLinking)
            {
                canvas.CancelLink();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool IsIdle()
        {
            return !_automationModule.IsRecording && !_automationModule.IsPlaying;
        }

        // Returns 0 if the text is not a valid number of times
        private int GetStopAfterCount()
        {
            return int.TryParse(txtStopAfter.Text, out int count) && count > 0 ? count : 0;
        }

        // Tell the module when the action must stop by itself. It can be changed while the action is running
        private void ApplyStopAfter()
        {
            int count = GetStopAfterCount();
            txtStopAfter.BackColor = count > 0 ? SystemColors.Window : Color.LightCoral;
            _automationModule.StopAfterFinalSteps = chkStopAfter.Checked ? count : 0;
            _automationModule.SleepWhenDone = chkSleepWhenDone.Checked;
        }

        private void chkSleepWhenDone_CheckedChanged(object sender, EventArgs e)
        {
            ApplyStopAfter();
        }

        private void chkStopAfter_CheckedChanged(object sender, EventArgs e)
        {
            ApplyStopAfter();
        }

        private void txtStopAfter_TextChanged(object sender, EventArgs e)
        {
            if (_updatingStopAfter) return;
            ApplyStopAfter();
        }

        // The module counts down the "Stop after" every time the final macro is done, show it in the box
        private void UpdateStopAfterCountdown()
        {
            int count = GetStopAfterCount();
            int left = _automationModule.StopAfterFinalSteps;
            if (!chkStopAfter.Checked || count <= 0 || left == count) return;

            // Not given back to the module, the playback thread may already have counted down again
            _updatingStopAfter = true;
            txtStopAfter.Text = left.ToString();
            txtStopAfter.BackColor = left > 0 ? SystemColors.Window : Color.LightCoral;
            _updatingStopAfter = false;
        }



        // ------------------------------------------------------------------------------------
        // Action preset
        // ------------------------------------------------------------------------------------

        private bool IsActionDirty()
        {
            return _activeAction != null && _workingAction != null && MacroPresetStore.ToJson(_activeAction) != MacroPresetStore.ToJson(_workingAction);
        }

        // Returns false if the user cancelled, meaning the current preset must stay as it is
        private bool ConfirmUnsavedActionChanges()
        {
            if (!IsActionDirty()) return true;

            DialogResult result = MessageBox.Show(this,
                "Action preset \"" + _activeAction.Name + "\" has unsaved changes.\nDo you want to save them?",
                "Unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

            if (result == DialogResult.Cancel) return false;
            if (result == DialogResult.Yes) return SaveActiveAction();
            return true;
        }

        private bool SaveActiveAction()
        {
            if (_activeAction == null || _workingAction == null) return false;

            // The recording in the module belongs to its node, make sure the node has the latest
            SyncMacroNodeFromModule();

            int index = _store.ActionPresets.IndexOf(_activeAction);
            ActionPreset saved = MacroPresetStore.CloneAction(_workingAction);
            ActionPreset old = _activeAction;
            _store.ActionPresets[index] = saved;
            _activeAction = saved;
            if (!_store.Save())
            {
                _store.ActionPresets[index] = old;
                _activeAction = old;
                return false;
            }
            UpdateControls();
            return true;
        }

        private void RefreshActionPresetList()
        {
            _updatingSelection = true;
            cboActionPreset.Items.Clear();
            foreach (ActionPreset preset in _store.ActionPresets)
            {
                cboActionPreset.Items.Add(preset.Name);
            }
            _updatingSelection = false;
        }

        // Show the action preset on the canvas. Null means there is no preset at all
        private void ActivateAction(ActionPreset preset)
        {
            _automationModule.StopAll();
            UnloadMacro();
            _activeAction = preset;
            _workingAction = preset == null ? null : MacroPresetStore.CloneAction(preset);
            _workingAction?.Normalize();

            _updatingSelection = true;
            cboActionPreset.SelectedIndex = preset == null ? -1 : _store.ActionPresets.IndexOf(preset);
            _updatingSelection = false;

            string selectedName = preset == null ? "" : preset.Name;
            if (_store.SelectedActionPreset != selectedName)
            {
                _store.SelectedActionPreset = selectedName;
                _store.Save();
            }

            // Clears the selection, which refreshes the panel
            canvas.SetGraph(_workingAction);
            UpdateControls();
        }

        private void cboActionPreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingSelection) return;

            ActionPreset preset = cboActionPreset.SelectedIndex >= 0 ? _store.ActionPresets[cboActionPreset.SelectedIndex] : null;
            if (preset == _activeAction) return;

            if (!ConfirmUnsavedActionChanges())
            {
                // Cancelled, go back to the previous preset
                _updatingSelection = true;
                cboActionPreset.SelectedIndex = _store.ActionPresets.IndexOf(_activeAction);
                _updatingSelection = false;
                return;
            }

            ActivateAction(preset);
        }

        private void btnActionNew_Click(object sender, EventArgs e)
        {
            if (!ConfirmUnsavedActionChanges()) return;

            string name = PromptForNewName("New action preset", n => _store.FindAction(n) != null);
            if (name == null) return;

            ActionPreset preset = new ActionPreset { Name = name };
            preset.EnsureStartEnd();
            _store.ActionPresets.Add(preset);
            _store.SelectedActionPreset = name;
            if (!_store.Save())
            {
                _store.ActionPresets.Remove(preset);
                return;
            }

            RefreshActionPresetList();
            ActivateAction(preset);
        }

        private void btnActionSave_Click(object sender, EventArgs e)
        {
            SaveActiveAction();
        }

        private void btnActionDelete_Click(object sender, EventArgs e)
        {
            if (_activeAction == null) return;

            DialogResult result = MessageBox.Show(this,
                "Delete action preset \"" + _activeAction.Name + "\"?\nIts macros, their recordings, links and samples will be lost.",
                "Delete action preset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes) return;

            int index = _store.ActionPresets.IndexOf(_activeAction);
            _store.ActionPresets.RemoveAt(index);
            _store.SelectedActionPreset = "";
            if (!_store.Save())
            {
                _store.ActionPresets.Insert(index, _activeAction);
                return;
            }

            RefreshActionPresetList();
            ActionPreset next = _store.ActionPresets.Count == 0 ? null : _store.ActionPresets[Math.Min(index, _store.ActionPresets.Count - 1)];
            ActivateAction(next);
        }

        // Ask for a name until it is valid. Returns null if the user cancelled
        private string PromptForNewName(string title, Func<string, bool> nameExists)
        {
            string name = "";
            while (true)
            {
                name = PromptForName(title, name);
                if (name == null) return null;

                if (name.Length == 0)
                {
                    MessageBox.Show(this, "The preset name can't be empty.", title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (nameExists(name))
                {
                    MessageBox.Show(this, "There is already a preset named \"" + name + "\".", title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    return name;
                }
            }
        }

        // Returns the trimmed name, or null if the user cancelled
        private string PromptForName(string title, string initialName)
        {
            using (Form dialog = new Form())
            using (Label label = new Label())
            using (TextBox textBox = new TextBox())
            using (Button okButton = new Button())
            using (Button cancelButton = new Button())
            {
                dialog.Text = title;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.ClientSize = new Size(300, 100);
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;

                label.Text = "Preset name:";
                label.AutoSize = true;
                label.Location = new Point(12, 12);

                textBox.Location = new Point(12, 34);
                textBox.Size = new Size(276, 23);
                textBox.MaxLength = 50;
                textBox.Text = initialName;

                okButton.Text = "OK";
                okButton.DialogResult = DialogResult.OK;
                okButton.Location = new Point(132, 66);
                okButton.Size = new Size(75, 25);

                cancelButton.Text = "Cancel";
                cancelButton.DialogResult = DialogResult.Cancel;
                cancelButton.Location = new Point(213, 66);
                cancelButton.Size = new Size(75, 25);

                dialog.Controls.Add(label);
                dialog.Controls.Add(textBox);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                return dialog.ShowDialog(this) == DialogResult.OK ? textBox.Text.Trim() : null;
            }
        }



        // ------------------------------------------------------------------------------------
        // Canvas: nodes and links
        // ------------------------------------------------------------------------------------

        private void OnCanvasSelectionChanged()
        {
            ActionNode node = canvas.SelectedNode;
            // The selected macro is the one that gets recorded into. Not while something runs, that would stop it
            if (node != null && node.IsMacro && IsIdle()) LoadNodeMacro(node);
            RefreshPanel();
        }

        private void OnCanvasGraphChanged()
        {
            UpdateControls();
        }

        private void OnCanvasLinkModeChanged()
        {
            UpdateHint();
            UpdateControls();
        }

        private void UpdateHint()
        {
            string zoom = "Zoom " + (canvas.Zoom * 100).ToString("0") + "%.   ";
            lblHint.Text = canvas.IsLinking ? HINT_LINKING : zoom + HINT_DEFAULT;
            lblHint.ForeColor = canvas.IsLinking ? Color.FromArgb(30, 120, 220) : SystemColors.GrayText;
        }

        private void OnCanvasContextMenuRequested(PointF world, Point client)
        {
            if (_workingAction == null) return;
            _contextWorldPoint = world;

            bool idle = IsIdle();
            ActionNode node = canvas.SelectedNode;
            ActionLink link = canvas.SelectedLink;
            bool macro = node != null && node.IsMacro;

            mnuNewMacro.Enabled = idle;
            mnuPasteMacro.Enabled = idle && _copiedNode != null;
            mnuLinkMacro.Visible = node != null && node.Kind != ActionNodeKind.End;
            mnuLinkMacro.Enabled = idle;
            mnuCopyMacro.Visible = macro;
            mnuFinal.Visible = macro;
            mnuFinal.Enabled = idle;
            mnuFinal.Checked = macro && node.IsFinal;
            mnuDisabled.Visible = macro;
            mnuDisabled.Enabled = idle;
            mnuDisabled.Checked = macro && !node.Enabled;
            mnuDelete.Visible = macro || link != null;
            mnuDelete.Enabled = idle;
            mnuDelete.Text = link != null ? "Delete link" : "Delete macro";
            mnuSeparator.Visible = mnuLinkMacro.Visible || mnuDelete.Visible;
            mnuCanvas.Show(canvas, client);
        }

        private void btnFit_Click(object sender, EventArgs e)
        {
            canvas.FitToContent();
        }

        // Add a macro next to the selected node, or in the middle of the view
        private void btnNewMacro_Click(object sender, EventArgs e)
        {
            if (_workingAction == null) return;

            PointF position;
            ActionNode selected = canvas.SelectedNode;
            if (selected != null)
            {
                RectangleF bounds = NodeCanvas.GetNodeBounds(selected);
                position = new PointF(bounds.Right + 70, bounds.Y + bounds.Height / 2 - NodeCanvas.NODE_HEIGHT / 2);
            }
            else
            {
                PointF center = canvas.ViewCenterWorld;
                position = new PointF(center.X - NodeCanvas.NODE_WIDTH / 2, center.Y - NodeCanvas.NODE_HEIGHT / 2);
            }
            // Don't drop it exactly over another node
            while (_workingAction.Nodes.Any(n => Math.Abs(n.X - position.X) < 20 && Math.Abs(n.Y - position.Y) < 20))
            {
                position.Y += NodeCanvas.NODE_HEIGHT + 30;
            }
            AddMacroNode(position);
        }

        private void mnuNewMacro_Click(object sender, EventArgs e)
        {
            AddMacroNode(new PointF(_contextWorldPoint.X - NodeCanvas.NODE_WIDTH / 2, _contextWorldPoint.Y - NodeCanvas.NODE_HEIGHT / 2));
        }

        private void AddMacroNode(PointF position)
        {
            if (_workingAction == null || !IsIdle()) return;

            ActionNode node = _workingAction.AddMacroNode(Math.Round(position.X), Math.Round(position.Y));
            canvas.RefreshGraph();
            canvas.Select(node);
            canvas.Focus();
            UpdateControls();
        }

        private void mnuCopyMacro_Click(object sender, EventArgs e)
        {
            ActionNode node = canvas.SelectedNode;
            if (node == null || !node.IsMacro) return;

            SyncMacroNodeFromModule();
            _copiedNode = MacroPresetStore.CloneNode(node);
            _copiedLinks = new List<CopiedLink>();
            foreach (ActionLink link in _workingAction.LinksFrom(node.Id))
            {
                ActionNode target = _workingAction.FindNode(link.ToId);
                if (target == null) continue;
                _copiedLinks.Add(new CopiedLink
                {
                    Link = MacroPresetStore.CloneJson(link),
                    ToKind = target.Kind,
                    ToName = target.Name
                });
            }
        }

        // The copy gets its own id, but keeps its name: macros sharing a name form a group that links go to at random.
        // Its links come with it. It can come from another action preset, the targets are then found by their name
        private void mnuPasteMacro_Click(object sender, EventArgs e)
        {
            if (_workingAction == null || _copiedNode == null || !IsIdle()) return;

            ActionNode node = MacroPresetStore.CloneNode(_copiedNode);
            node.Id = ActionNode.NewId();
            if (node.Name == "") node.Name = _workingAction.UniqueName(ActionNode.DEFAULT_NAME);
            node.X = Math.Round(_contextWorldPoint.X - NodeCanvas.NODE_WIDTH / 2);
            node.Y = Math.Round(_contextWorldPoint.Y - NodeCanvas.NODE_HEIGHT / 2);
            _workingAction.Nodes.Add(node);

            foreach (CopiedLink copied in _copiedLinks)
            {
                ActionNode target = FindPasteTarget(copied, node);
                if (target == null) continue;
                _workingAction.Links.Add(new ActionLink
                {
                    FromId = node.Id,
                    ToId = target.Id,
                    Condition = copied.Link.Condition == null ? new ActionCondition() : MacroPresetStore.CloneJson(copied.Link.Condition)
                });
            }

            canvas.RefreshGraph();
            canvas.Select(node);
            UpdateControls();
        }

        // Where a copied link goes in this action. Null if there is nothing like its target here
        private ActionNode FindPasteTarget(CopiedLink copied, ActionNode pastedNode)
        {
            // A link of the macro to itself stays a loop on the copy
            if (copied.Link.ToId == _copiedNode.Id) return pastedNode;
            if (copied.ToKind == ActionNodeKind.End) return _workingAction.EndNode;
            if (copied.ToKind != ActionNodeKind.Macro) return null;

            ActionNode target = _workingAction.FindNode(copied.Link.ToId);
            if (target != null && target.IsMacro) return target;
            // Pasted in another action: a macro with the same name is as good, they form a group anyway
            if (copied.ToName == "") return null;
            return _workingAction.MacroNodes.FirstOrDefault(n => string.Equals(n.Name, copied.ToName, StringComparison.OrdinalIgnoreCase));
        }

        private void mnuFinal_Click(object sender, EventArgs e)
        {
            ActionNode node = canvas.SelectedNode;
            if (node == null || !node.IsMacro) return;

            node.IsFinal = mnuFinal.Checked;
            _updatingSelection = true;
            chkFinal.Checked = node.IsFinal;
            _updatingSelection = false;
            canvas.RefreshGraph();
            UpdateControls();
        }

        private void btnLinkMacro_Click(object sender, EventArgs e)
        {
            ActionNode node = canvas.SelectedNode;
            if (node == null || node.Kind == ActionNodeKind.End || !IsIdle()) return;
            canvas.BeginLink(node);
        }

        // The user clicked a node while in link mode
        private void OnCanvasLinkRequested(ActionNode from, ActionNode to)
        {
            if (_workingAction == null || !IsIdle()) return;

            if (to.Kind == ActionNodeKind.Start)
            {
                MessageBox.Show(this, "Nothing can link to the Start.", "Link macro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (from.Kind == ActionNodeKind.Start && to.Kind == ActionNodeKind.End)
            {
                MessageBox.Show(this, "The Start must link to a macro.", "Link macro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ActionLink link = new ActionLink { FromId = from.Id, ToId = to.Id };
            if (from.Kind == ActionNodeKind.Start)
            {
                // The Start has a single link, and no condition
                _workingAction.Links.RemoveAll(l => l.FromId == from.Id);
                link.Condition = null;
            }
            _workingAction.Links.Add(link);
            canvas.RefreshGraph();
            canvas.Select(link);
            UpdateControls();
        }

        private void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            DeleteSelected();
        }

        private void DeleteSelected()
        {
            if (_workingAction == null || !IsIdle()) return;

            ActionLink link = canvas.SelectedLink;
            if (link != null)
            {
                ActionNode from = _workingAction.FindNode(link.FromId);
                _workingAction.Links.Remove(link);
                canvas.RefreshGraph();
                canvas.Select(from);
                UpdateControls();
                return;
            }

            ActionNode node = canvas.SelectedNode;
            if (node == null || !node.IsMacro) return;

            if (node == _macroNode) UnloadMacro();
            _workingAction.RemoveNode(node);
            canvas.RefreshGraph();
            canvas.ClearSelection();
            RefreshPanel();
            UpdateControls();
        }



        // ------------------------------------------------------------------------------------
        // The macro loaded in the module
        // ------------------------------------------------------------------------------------

        // Give the recording of the node to the module, so Ctrl + Up records into it and its steps can be edited
        private void LoadNodeMacro(ActionNode node)
        {
            if (_macroNode == node) return;

            // Stopping a recording writes it back to its node, before the node changes
            _automationModule.StopAll();
            _macroNode = node;
            _loadingMacro = true;
            _automationModule.LoadMacro(node.Actions, node.Duration);
            _loadingMacro = false;
            ApplyMacroToModule();
        }

        private void UnloadMacro()
        {
            _automationModule.StopAll();
            _macroNode = null;
            _loadingMacro = true;
            _automationModule.LoadMacro(new List<MacroAction>(), 0);
            _loadingMacro = false;
            ApplyMacroToModule();
        }

        // Tell the module what kind of macro it holds: only a "Repeat" can be recorded
        private void ApplyMacroToModule()
        {
            ActionNode node = _macroNode;
            _automationModule.SetPickupMacro(node != null && node.Type == MacroType.ItemPickup ? node.Pickup : null);
            _automationModule.SetMacroSelected(node != null && node.Type != MacroType.Delay);
        }

        // Copy the recording of the module into its node
        private void SyncMacroNodeFromModule()
        {
            if (_macroNode == null) return;

            _macroNode.Actions = new List<MacroAction>();
            foreach (MacroAction action in _automationModule.Actions)
            {
                _macroNode.Actions.Add(action.Clone());
            }
            _macroNode.Duration = _automationModule.Duration;
        }

        private void OnModuleStateChanged()
        {
            // The playback ended: the selected macro can now be loaded, if it was selected while running
            ActionNode node = canvas.SelectedNode;
            if (IsIdle() && node != null && node.IsMacro && node != _macroNode)
            {
                LoadNodeMacro(node);
                RefreshActionList(node);
            }
            UpdateControls();
        }

        private void OnModuleMacroChanged()
        {
            EndTimeEdit(false);
            if (!_loadingMacro) SyncMacroNodeFromModule();
            if (_macroNode != null && canvas.SelectedNode == _macroNode) RefreshActionList(_macroNode);
            canvas.RefreshGraph();
            UpdateControls();
        }

        private void OnModuleActionRecorded(MacroAction action)
        {
            if (_macroNode == null) return;

            _macroNode.Actions.Add(action.Clone());
            if (canvas.SelectedNode == _macroNode)
            {
                ListViewItem item = lstActions.Items.Add(CreateListItem(_macroNode.Actions, _macroNode.Actions.Count - 1));
                item.EnsureVisible();
            }
            canvas.RefreshGraph();
        }



        // ------------------------------------------------------------------------------------
        // Right panel
        // ------------------------------------------------------------------------------------

        // Show the editor of what is selected on the canvas
        private void RefreshPanel()
        {
            ActionNode node = canvas.SelectedNode;
            ActionLink link = canvas.SelectedLink;

            _updatingSelection = true;
            grpNode.Visible = false;
            grpLinks.Visible = false;
            grpLink.Visible = false;
            grpInfo.Visible = false;

            if (link != null && _workingAction != null)
            {
                RefreshLinkPanel(link);
                grpLink.Visible = true;
            }
            else if (node != null && node.IsMacro)
            {
                RefreshNodePanel(node);
                grpNode.Visible = true;
                grpLinks.Text = "Links from this macro";
                grpLinks.Top = grpNode.Bottom + 8;
                RefreshLinkList(node);
                grpLinks.Visible = true;
            }
            else if (node != null && node.Kind == ActionNodeKind.Start)
            {
                grpInfo.Text = "Start";
                lblInfo.Text = "The action begins here. Use \"Link macro\" to choose the first macro to play, it starts right away.\n\n"
                    + "Ctrl + Down starts / stops the action.";
                grpInfo.Visible = true;
                grpLinks.Text = "Link from the Start";
                grpLinks.Top = grpInfo.Bottom + 8;
                RefreshLinkList(node);
                grpLinks.Visible = true;
            }
            else if (node != null)
            {
                grpInfo.Text = "End";
                lblInfo.Text = "A link going here ends the action.\n\nLink a macro to the End, and pick the condition on that link.";
                grpInfo.Visible = true;
            }
            else
            {
                grpInfo.Text = "Selection";
                lblInfo.Text = _workingAction == null
                    ? "Create an action preset with the New button of the toolbar."
                    : "Click a macro or a link on the canvas to edit it.\n\n"
                    + "\"New macro\" (or a right click on the canvas) adds a macro. Select a macro, then \"Link macro\" and click another one to link them.";
                grpInfo.Visible = true;
            }
            _updatingSelection = false;

            UpdateControls();
        }

        private void RefreshNodePanel(ActionNode node)
        {
            txtNodeName.Text = node.Name;
            cboNodeType.SelectedIndex = (int)node.Type;
            chkFinal.Checked = node.IsFinal;
            chkEnabled.Checked = node.Enabled;
            txtDelay.Text = node.DelayMs.ToString();
            pickupEditor.SetSettings(_automationModule, node.Pickup);
            RefreshActionList(node);
            ApplyNodeTypeVisibility(node);
            UpdateGroupHint(node);
        }

        // Tell when other macros share the name of this one
        private void UpdateGroupHint(ActionNode node)
        {
            int count = _workingAction == null ? 1 : _workingAction.GroupOf(node).Count;
            lblGroupHint.Visible = count > 1;
            lblGroupHint.Text = "Group of " + count + " macros:\na link goes to any of them.";
        }

        // Show the editor of the type of the macro
        private void ApplyNodeTypeVisibility(ActionNode node)
        {
            bool isRepeat = node.Type == MacroType.Repeat;
            bool isDelay = node.Type == MacroType.Delay;
            bool isPickup = node.Type == MacroType.ItemPickup;
            lblDelay.Visible = isDelay;
            txtDelay.Visible = isDelay;
            lblDelayHelp.Visible = isDelay;
            pickupEditor.Visible = isPickup;
            lblHotkeys.Visible = isRepeat;
            lstActions.Visible = isRepeat;
            btnMacroStepDelete.Visible = isRepeat;
        }

        private void RefreshActionList(ActionNode node)
        {
            EndTimeEdit(false);
            lstActions.BeginUpdate();
            lstActions.Items.Clear();
            for (int i = 0; i < node.Actions.Count; i++)
            {
                lstActions.Items.Add(CreateListItem(node.Actions, i));
            }
            // The end of the loop, once the recording is done. Its time is when the macro starts again
            if (!(_automationModule.IsRecording && node == _macroNode))
            {
                lstActions.Items.Add(CreateEndListItem(node.Actions, node.Duration));
            }
            lstActions.EndUpdate();
            _shownAction = -1;
        }

        private void RefreshLinkList(ActionNode node)
        {
            ActionLink selected = GetSelectedListLink();
            lstLinks.BeginUpdate();
            lstLinks.Items.Clear();
            if (_workingAction != null)
            {
                foreach (ActionLink link in _workingAction.LinksFrom(node.Id))
                {
                    ActionNode target = _workingAction.FindNode(link.ToId);
                    ListViewItem item = new ListViewItem(link.Condition == null ? "Right away" : link.Condition.Describe());
                    item.SubItems.Add(_workingAction.DescribeTarget(target));
                    item.Tag = link;
                    item.Selected = link == selected;
                    lstLinks.Items.Add(item);
                }
            }
            lstLinks.EndUpdate();
        }

        private ActionLink GetSelectedListLink()
        {
            return lstLinks.SelectedItems.Count > 0 ? lstLinks.SelectedItems[0].Tag as ActionLink : null;
        }

        private void txtNodeName_TextChanged(object sender, EventArgs e)
        {
            ActionNode node = canvas.SelectedNode;
            if (_updatingSelection || node == null || !node.IsMacro) return;

            node.Name = txtNodeName.Text.Trim();
            UpdateGroupHint(node);
            RefreshLinkList(node);
            canvas.RefreshGraph();
            UpdateControls();
        }

        private void chkFinal_CheckedChanged(object sender, EventArgs e)
        {
            ActionNode node = canvas.SelectedNode;
            if (_updatingSelection || node == null || !node.IsMacro) return;

            node.IsFinal = chkFinal.Checked;
            canvas.RefreshGraph();
            UpdateControls();
        }

        private void chkEnabled_CheckedChanged(object sender, EventArgs e)
        {
            ActionNode node = canvas.SelectedNode;
            if (_updatingSelection || node == null || !node.IsMacro) return;

            SetNodeEnabled(node, chkEnabled.Checked);
        }

        private void mnuDisabled_Click(object sender, EventArgs e)
        {
            ActionNode node = canvas.SelectedNode;
            if (node == null || !node.IsMacro || !IsIdle()) return;

            SetNodeEnabled(node, !mnuDisabled.Checked);
        }

        // Temporarily take a macro out of the action, the links to it go to its group or to the End
        private void SetNodeEnabled(ActionNode node, bool enabled)
        {
            node.Enabled = enabled;
            _updatingSelection = true;
            chkEnabled.Checked = enabled;
            _updatingSelection = false;
            // The targets shown in the lists change with it
            if (canvas.SelectedNode == node) RefreshLinkList(node);
            canvas.RefreshGraph();
            UpdateControls();
        }

        private void cboNodeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActionNode node = canvas.SelectedNode;
            if (_updatingSelection || node == null || !node.IsMacro || cboNodeType.SelectedIndex < 0) return;

            node.Type = (MacroType)cboNodeType.SelectedIndex;
            if (node == _macroNode) ApplyMacroToModule();
            ApplyNodeTypeVisibility(node);
            canvas.RefreshGraph();
            UpdateControls();
        }

        // Returns -1 if the text is not a valid delay
        private int GetDelay()
        {
            return int.TryParse(txtDelay.Text, out int delay) && delay > 0 && delay <= AutomationModule.MAX_ACTION_TIME ? delay : -1;
        }

        private void txtDelay_TextChanged(object sender, EventArgs e)
        {
            int delay = GetDelay();
            txtDelay.BackColor = delay > 0 ? SystemColors.Window : Color.LightCoral;
            ActionNode node = canvas.SelectedNode;
            if (_updatingSelection || node == null || !node.IsMacro || delay < 0) return;

            node.DelayMs = delay;
            canvas.RefreshGraph();
            UpdateControls();
        }

        private void OnPickupSettingsChanged()
        {
            // The editor changes the settings of the node in place
            canvas.RefreshGraph();
            UpdateControls();
        }

        private void lstLinks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingSelection) return;
            UpdateControls();
        }

        private void lstLinks_DoubleClick(object sender, EventArgs e)
        {
            btnLinkEdit_Click(sender, e);
        }

        // Select the link on the canvas, its condition shows up in the panel
        private void btnLinkEdit_Click(object sender, EventArgs e)
        {
            ActionLink link = GetSelectedListLink();
            if (link != null) canvas.Select(link);
        }

        private void btnLinkUp_Click(object sender, EventArgs e)
        {
            MoveSelectedLink(-1);
        }

        private void btnLinkDown_Click(object sender, EventArgs e)
        {
            MoveSelectedLink(1);
        }

        // Change the order the conditions are checked in
        private void MoveSelectedLink(int offset)
        {
            ActionNode node = canvas.SelectedNode;
            ActionLink link = GetSelectedListLink();
            if (node == null || link == null || _workingAction == null || !IsIdle()) return;

            List<ActionLink> links = _workingAction.LinksFrom(node.Id);
            int index = links.IndexOf(link);
            int other = index + offset;
            if (index < 0 || other < 0 || other >= links.Count) return;

            int a = _workingAction.Links.IndexOf(links[index]);
            int b = _workingAction.Links.IndexOf(links[other]);
            _workingAction.Links[a] = links[other];
            _workingAction.Links[b] = links[index];
            RefreshLinkList(node);
            canvas.RefreshGraph();
            UpdateControls();
        }

        private void btnLinkRemove_Click(object sender, EventArgs e)
        {
            ActionNode node = canvas.SelectedNode;
            ActionLink link = GetSelectedListLink();
            if (node == null || link == null || _workingAction == null || !IsIdle()) return;

            _workingAction.Links.Remove(link);
            RefreshLinkList(node);
            canvas.RefreshGraph();
            UpdateControls();
        }



        // ------------------------------------------------------------------------------------
        // Right panel: the condition of a link
        // ------------------------------------------------------------------------------------

        private void RefreshLinkPanel(ActionLink link)
        {
            ActionNode from = _workingAction.FindNode(link.FromId);
            ActionNode to = _workingAction.FindNode(link.ToId);
            lblLinkFromTo.Text = "From \"" + (from == null ? "?" : from.DisplayName) + "\" to \"" + _workingAction.DescribeTarget(to) + "\"";

            bool hasCondition = link.Condition != null;
            lblType.Visible = hasCondition;
            cboType.Visible = hasCondition;
            grpScreen.Visible = hasCondition;
            lblLinkHelp.Text = hasCondition
                ? "The condition is checked while the macro plays. When it is met, the action goes to the target of the link."
                : "The Start goes to its macro right away, this link has no condition.";

            picSample.Image = null;
            _sample?.Dispose();
            _sample = null;
            if (!hasCondition)
            {
                lblValue.Visible = false;
                numValue.Visible = false;
                lblValueUnit.Visible = false;
                return;
            }

            ActionCondition condition = link.Condition;
            cboType.SelectedIndex = (int)condition.Type;
            numInterval.Value = Math.Max(numInterval.Minimum, Math.Min(numInterval.Maximum, condition.CheckInterval));
            numTolerance.Value = Math.Max(numTolerance.Minimum, Math.Min(numTolerance.Maximum, condition.ColorTolerance));
            numPercent.Value = Math.Max(numPercent.Minimum, Math.Min(numPercent.Maximum, condition.MatchPercent));
            ApplyConditionType(condition);
            _sample = ScreenMatcher.FromBase64(condition.SampleBase64);
            UpdateSample(condition);
        }

        private ActionCondition GetSelectedCondition()
        {
            return canvas.SelectedLink?.Condition;
        }

        // Show the controls of the type of the condition
        private void ApplyConditionType(ActionCondition condition)
        {
            bool wasUpdating = _updatingSelection;
            _updatingSelection = true;

            grpScreen.Enabled = condition.Type == ConditionType.ScreenMatch;
            // The number box is the loop count or the seconds, depending on the type
            bool hasValue = condition.Type == ConditionType.MacroLooped || condition.Type == ConditionType.MacroRanFor;
            lblValue.Visible = hasValue;
            numValue.Visible = hasValue;
            lblValueUnit.Visible = hasValue;
            if (condition.Type == ConditionType.MacroLooped)
            {
                lblValue.Text = "Number of loops:";
                lblValueUnit.Text = "times";
                numValue.DecimalPlaces = 0;
                numValue.Minimum = 1;
                numValue.Value = Math.Max(numValue.Minimum, Math.Min(numValue.Maximum, condition.LoopCount));
            }
            else if (condition.Type == ConditionType.MacroRanFor)
            {
                lblValue.Text = "Running time:";
                lblValueUnit.Text = "seconds (checked at the end of each loop)";
                numValue.DecimalPlaces = 1;
                numValue.Minimum = 0.1m;
                numValue.Value = Math.Max(numValue.Minimum, Math.Min(numValue.Maximum, (decimal)condition.RunSeconds));
            }

            _updatingSelection = wasUpdating;
        }

        private void UpdateSample(ActionCondition condition)
        {
            picSample.Image = _sample;
            lblRegion.Text = _sample == null
                ? "No sample yet"
                : "At (" + condition.RegionX + ", " + condition.RegionY + "), " + _sample.Width + " x " + _sample.Height + " pixels";
            lblTestResult.Text = "";
        }

        private void cboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActionCondition condition = GetSelectedCondition();
            if (_updatingSelection || condition == null || cboType.SelectedIndex < 0) return;

            condition.Type = (ConditionType)cboType.SelectedIndex;
            ApplyConditionType(condition);
            canvas.RefreshGraph();
            UpdateControls();
        }

        private void numValue_ValueChanged(object sender, EventArgs e)
        {
            ActionCondition condition = GetSelectedCondition();
            if (_updatingSelection || condition == null) return;

            if (condition.Type == ConditionType.MacroLooped) condition.LoopCount = (int)numValue.Value;
            if (condition.Type == ConditionType.MacroRanFor) condition.RunSeconds = (double)numValue.Value;
            canvas.RefreshGraph();
            UpdateControls();
        }

        private void numScreen_ValueChanged(object sender, EventArgs e)
        {
            ActionCondition condition = GetSelectedCondition();
            if (_updatingSelection || condition == null) return;

            condition.CheckInterval = (int)numInterval.Value;
            condition.ColorTolerance = (int)numTolerance.Value;
            condition.MatchPercent = (int)numPercent.Value;
            canvas.RefreshGraph();
            UpdateControls();
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            ActionCondition condition = GetSelectedCondition();
            if (condition == null) return;

            using (CaptureForm captureForm = new CaptureForm(_automationModule))
            {
                if (captureForm.ShowDialog(this) != DialogResult.OK) return;

                picSample.Image = null;
                _sample?.Dispose();
                _sample = captureForm.Sample;
                condition.RegionX = captureForm.SampleRegion.X;
                condition.RegionY = captureForm.SampleRegion.Y;
                condition.RegionWidth = captureForm.SampleRegion.Width;
                condition.RegionHeight = captureForm.SampleRegion.Height;
                condition.SampleBase64 = ScreenMatcher.ToBase64(_sample);
                UpdateSample(condition);
                canvas.RefreshGraph();
                UpdateControls();
            }
        }

        // Compare the sample with what is on the screen right now, to help picking the numbers
        private void btnTest_Click(object sender, EventArgs e)
        {
            ActionCondition condition = GetSelectedCondition();
            if (condition == null || _sample == null) return;
            try
            {
                using (ScreenMatcher matcher = new ScreenMatcher(_sample, condition.Region))
                {
                    double percent = matcher.GetMatchPercent(condition.ColorTolerance);
                    bool matches = percent >= condition.MatchPercent;
                    lblTestResult.ForeColor = matches ? Color.Green : Color.Red;
                    lblTestResult.Text = percent.ToString("0.0") + "% of the pixels match";
                }
            }
            catch (Exception ex)
            {
                lblTestResult.ForeColor = Color.Red;
                lblTestResult.Text = ex.Message;
            }
        }

        private void btnLinkDelete_Click(object sender, EventArgs e)
        {
            DeleteSelected();
        }

        private void btnLinkGoTarget_Click(object sender, EventArgs e)
        {
            ActionLink link = canvas.SelectedLink;
            if (link == null || _workingAction == null) return;

            ActionNode target = _workingAction.FindNode(link.ToId);
            if (target != null) canvas.Select(target);
        }



        // ------------------------------------------------------------------------------------
        // Running the action
        // ------------------------------------------------------------------------------------

        // Called by the module when Ctrl + Down is pressed: turn the canvas into something it can run.
        // Returns null and an error message if the action preset can't run.
        private List<AutomationStep> BuildActionPlan(out string error)
        {
            error = "";
            _planNodeIds = new List<string>();
            if (_workingAction == null)
            {
                error = "Create an action preset first.";
                return null;
            }
            SyncMacroNodeFromModule();

            ActionNode start = _workingAction.StartNode;
            ActionNode end = _workingAction.EndNode;
            ActionLink startLink = _workingAction.LinksFrom(start.Id).FirstOrDefault();
            ActionNode first = startLink == null ? null : _workingAction.FindNode(startLink.ToId);
            if (first == null || !first.IsMacro)
            {
                error = "Link the Start to the first macro to play.";
                return null;
            }
            // Macros sharing a name are one group, a link to one of them goes to any of them at random
            List<ActionNode> firstGroup = _workingAction.EnabledTargets(first);
            if (firstGroup.Count == 0)
            {
                error = "The first macro \"" + first.DisplayName + "\" is disabled, there is nothing to play.";
                return null;
            }
            first = firstGroup[new Random().Next(firstGroup.Count)];

            // The first macro is step 0, the others follow in the order of the canvas
            List<ActionNode> order = new List<ActionNode> { first };
            order.AddRange(_workingAction.MacroNodes.Where(n => n != first));
            Dictionary<string, int> indexOf = new Dictionary<string, int>();
            for (int i = 0; i < order.Count; i++)
            {
                indexOf[order[i].Id] = i;
            }

            List<AutomationStep> plan = new List<AutomationStep>();
            foreach (ActionNode node in order)
            {
                AutomationStep step = new AutomationStep { Name = node.DisplayName, IsFinal = node.IsFinal };
                plan.Add(step);

                if (node.Type == MacroType.ItemPickup)
                {
                    step.Pickup = MacroPresetStore.ClonePickup(node.Pickup);
                    if (step.Pickup.Labels.Count == 0)
                    {
                        error = "Macro \"" + node.DisplayName + "\" has no label color to look for.";
                        break;
                    }
                }
                else if (node.Type == MacroType.Delay)
                {
                    // A macro without any action, that ends after the delay
                    step.Duration = node.DelayMs;
                    if (step.Duration <= 0 || step.Duration > AutomationModule.MAX_ACTION_TIME)
                    {
                        error = "The delay of macro \"" + node.DisplayName + "\" is not valid.";
                        break;
                    }
                }
                else
                {
                    foreach (MacroAction action in node.Actions)
                    {
                        step.Actions.Add(action.Clone());
                    }
                    step.Duration = node.Duration;
                }

                foreach (ActionLink link in _workingAction.LinksFrom(node.Id))
                {
                    int[] targets;
                    if (link.ToId == end.Id)
                    {
                        targets = new[] { AutomationStep.TARGET_END };
                    }
                    else
                    {
                        ActionNode target = _workingAction.FindNode(link.ToId);
                        if (target == null || !target.IsMacro)
                        {
                            error = "A link of macro \"" + node.DisplayName + "\" goes to a macro that doesn't exist.";
                            break;
                        }
                        // Only the enabled ones. All disabled: the link ends the action
                        targets = _workingAction.EnabledTargets(target).Select(n => indexOf[n.Id]).ToArray();
                        if (targets.Length == 0) targets = new[] { AutomationStep.TARGET_END };
                    }

                    ActionCondition condition = link.Condition ?? new ActionCondition();
                    if (condition.Type != ConditionType.ScreenMatch)
                    {
                        // "Ended" is the same as "looped 1 time"
                        AutomationEndCheck endCheck = new AutomationEndCheck { Loops = 1, Targets = targets };
                        if (condition.Type == ConditionType.MacroLooped) endCheck.Loops = Math.Max(1, condition.LoopCount);
                        if (condition.Type == ConditionType.MacroRanFor) endCheck.Milliseconds = (long)(condition.RunSeconds * 1000);
                        step.EndChecks.Add(endCheck);
                        continue;
                    }

                    using (Bitmap sample = ScreenMatcher.FromBase64(condition.SampleBase64))
                    {
                        if (sample == null)
                        {
                            error = "A screen condition of macro \"" + node.DisplayName + "\" has no sample.";
                            break;
                        }
                        step.Checks.Add(new AutomationScreenCheck
                        {
                            Matcher = new ScreenMatcher(sample, condition.Region),
                            Interval = Math.Max(1, condition.CheckInterval),
                            Tolerance = condition.ColorTolerance,
                            MatchPercent = condition.MatchPercent,
                            Targets = targets
                        });
                    }
                }
                if (error != "") break;
            }

            if (error != "")
            {
                foreach (AutomationStep step in plan)
                {
                    foreach (AutomationScreenCheck check in step.Checks)
                    {
                        check.Matcher.Dispose();
                    }
                }
                return null;
            }

            foreach (ActionNode node in order)
            {
                _planNodeIds.Add(node.Id);
            }
            return plan;
        }

        // The node of the step being played, null if the action isn't running
        private ActionNode GetRunningNode()
        {
            if (!_automationModule.IsRunningAction || _workingAction == null) return null;

            int stepIndex = _automationModule.CurrentStep;
            return stepIndex >= 0 && stepIndex < _planNodeIds.Count ? _workingAction.FindNode(_planNodeIds[stepIndex]) : null;
        }



        // ------------------------------------------------------------------------------------
        // Recorded actions of the selected macro
        // ------------------------------------------------------------------------------------

        private void lstActions_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!IsIdle() || _macroNode == null || canvas.SelectedNode != _macroNode) return;

            ListViewHitTestInfo hit = lstActions.HitTest(e.Location);
            if (hit.Item == null || hit.SubItem == null) return;

            int column = hit.Item.SubItems.IndexOf(hit.SubItem);
            int row = hit.Item.Index;
            if (column == COLUMN_TIME || column == COLUMN_DELTA)
            {
                BeginTimeEdit(row, column, hit.SubItem.Bounds);
            }
            else if ((column == COLUMN_ACTION || column == COLUMN_DETAIL) && row < _automationModule.Actions.Count)
            {
                MacroAction action = _automationModule.Actions[row];
                bool changed;
                if (action.IsMouse)
                {
                    changed = PromptForPosition(action, out double xPercent, out double yPercent)
                        && _automationModule.SetActionPosition(row, xPercent, yPercent);
                }
                else
                {
                    Keys key = PromptForKey(action);
                    changed = key != Keys.None && _automationModule.SetActionKey(row, key);
                }
                if (changed) SelectActionRow(row);
            }
        }

        // The list was rebuilt, go back to the edited row
        private void SelectActionRow(int row)
        {
            if (lstActions.Items.Count == 0) return;

            row = Math.Max(0, Math.Min(row, lstActions.Items.Count - 1));
            lstActions.Items[row].Selected = true;
            lstActions.Items[row].EnsureVisible();
        }

        private void lstActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            // The playback also moves the selection, many times per second
            if (_automationModule.IsPlaying) return;
            UpdateControls();
        }

        private void btnMacroStepDelete_Click(object sender, EventArgs e)
        {
            if (lstActions.SelectedIndices.Count == 0 || canvas.SelectedNode != _macroNode) return;

            int row = lstActions.SelectedIndices[0];
            if (_automationModule.RemoveAction(row)) SelectActionRow(row);
        }

        // A form that takes every key for itself, even the ones a dialog normally uses (Enter, Escape, Tab, Alt...)
        private class KeyCaptureDialog : Form
        {
            public Keys CapturedKey = Keys.None;

            protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            {
                CapturedKey = keyData & Keys.KeyCode;
                DialogResult = DialogResult.OK;
                return true;
            }
        }

        private static void SetupPromptDialog(Form dialog, Label label, Button cancelButton, string title, string text)
        {
            dialog.Text = title;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.ClientSize = new Size(320, 105);
            dialog.MaximizeBox = false;
            dialog.MinimizeBox = false;
            dialog.ShowInTaskbar = false;

            label.Text = text;
            label.Location = new Point(12, 12);
            label.Size = new Size(296, 50);

            cancelButton.Text = "Cancel";
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(233, 70);
            cancelButton.Size = new Size(75, 25);
            // Space / Enter must not press it, they are keys to capture
            cancelButton.TabStop = false;

            dialog.Controls.Add(label);
            dialog.Controls.Add(cancelButton);
        }

        // Returns Keys.None if the user cancelled
        private Keys PromptForKey(MacroAction action)
        {
            using (KeyCaptureDialog dialog = new KeyCaptureDialog())
            using (Label label = new Label())
            using (Button cancelButton = new Button())
            {
                SetupPromptDialog(dialog, label, cancelButton, "Change the key",
                    "Press the key that replaces \"" + action.Key + "\".\nThe other half of the press (down / up) changes with it.");
                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.CapturedKey : Keys.None;
            }
        }

        // Wait for a click anywhere on the screen, the game included. Returns false if the user cancelled
        private bool PromptForPosition(MacroAction action, out double xPercent, out double yPercent)
        {
            double newX = 0, newY = 0;
            using (Form dialog = new Form())
            using (Label label = new Label())
            using (Button cancelButton = new Button())
            {
                SetupPromptDialog(dialog, label, cancelButton, "Change the position",
                    "Click where the mouse " + (action.Type == MacroActionType.MouseDown ? "down" : "up") + " must happen, anywhere on the screen.\n"
                    + "Now: " + action.DescribeDetail());
                // Stays visible when the click gives the focus to the game
                dialog.TopMost = true;

                Action<MouseButtons, bool> onMouseInput = (button, isDown) =>
                {
                    // The clicks on this dialog are for its Cancel button or to move it
                    if (!isDown || dialog.DialogResult != DialogResult.None || dialog.Bounds.Contains(Cursor.Position)) return;

                    _automationModule._inputHook.GetCurrentMousePercentPosition(out newX, out newY);
                    dialog.DialogResult = DialogResult.OK;
                };

                _automationModule.MouseInput += onMouseInput;
                try
                {
                    bool ok = dialog.ShowDialog(this) == DialogResult.OK;
                    xPercent = newX;
                    yPercent = newY;
                    // The click probably went to another window
                    if (ok) Activate();
                    return ok;
                }
                finally
                {
                    _automationModule.MouseInput -= onMouseInput;
                }
            }
        }

        // Show a text box right over the time, with the number of milliseconds in it
        private void BeginTimeEdit(int row, int column, Rectangle bounds)
        {
            EndTimeEdit(false);

            // The row after the actions is the end of the macro
            List<MacroAction> actions = _automationModule.Actions;
            if (row > actions.Count) return;
            int time = row < actions.Count ? actions[row].Time : _automationModule.Duration;
            int previousTime = row > 0 ? actions[row - 1].Time : 0;

            _timeEditorRow = row;
            _timeEditorColumn = column;
            _timeEditor = new TextBox();
            _timeEditor.MaxLength = 8;
            _timeEditor.Text = (column == COLUMN_TIME ? time : time - previousTime).ToString();
            _timeEditor.Bounds = bounds;
            _timeEditor.KeyDown += timeEditor_KeyDown;
            _timeEditor.Leave += timeEditor_Leave;
            lstActions.Controls.Add(_timeEditor);
            _timeEditor.SelectAll();
            _timeEditor.Focus();
        }

        private void EndTimeEdit(bool commit)
        {
            if (_timeEditor == null) return;

            TextBox editor = _timeEditor;
            // Removing the box makes it lose the focus, which must not end the edit a second time
            _timeEditor = null;
            string text = editor.Text.Trim();
            lstActions.Controls.Remove(editor);
            editor.Dispose();

            if (!commit || _timeEditorRow > _automationModule.Actions.Count) return;
            if (!int.TryParse(text, out int value) || value < 0)
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            // Both columns move the action and everything after it, they only count from a different place
            int previousTime = _timeEditorRow > 0 ? _automationModule.Actions[_timeEditorRow - 1].Time : 0;
            long newTime = _timeEditorColumn == COLUMN_TIME ? value : (long)previousTime + value;
            int row = _timeEditorRow;
            int clampedTime = (int)Math.Min(newTime, int.MaxValue);
            bool changed = row < _automationModule.Actions.Count
                ? _automationModule.SetActionTime(row, clampedTime)
                : _automationModule.SetDuration(clampedTime);
            if (changed && row < lstActions.Items.Count)
            {
                // The list was rebuilt, go back to the edited row
                lstActions.Items[row].Selected = true;
                lstActions.Items[row].EnsureVisible();
            }
        }

        private void timeEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
            {
                // No "ding"
                e.Handled = true;
                e.SuppressKeyPress = true;
                EndTimeEdit(e.KeyCode == Keys.Enter);
                lstActions.Focus();
            }
        }

        private void timeEditor_Leave(object sender, EventArgs e)
        {
            EndTimeEdit(true);
        }

        private ListViewItem CreateListItem(List<MacroAction> actions, int index)
        {
            MacroAction action = actions[index];
            int previousTime = index > 0 ? actions[index - 1].Time : 0;

            ListViewItem item = new ListViewItem((index + 1).ToString());
            item.SubItems.Add(action.Time + " ms");
            item.SubItems.Add((action.Time - previousTime) + " ms");
            item.SubItems.Add(action.Describe());
            item.SubItems.Add(action.DescribeDetail());
            return item;
        }

        // The last row of the list, it can't be removed. Only its time can be changed
        private ListViewItem CreateEndListItem(List<MacroAction> actions, int duration)
        {
            int previousTime = actions.Count > 0 ? actions[actions.Count - 1].Time : 0;

            ListViewItem item = new ListViewItem("");
            item.SubItems.Add(duration + " ms");
            item.SubItems.Add((duration - previousTime) + " ms");
            item.SubItems.Add("End macro");
            item.SubItems.Add("Starts again from step 1");
            item.ForeColor = SystemColors.GrayText;
            return item;
        }



        // ------------------------------------------------------------------------------------
        // Enabled states and status
        // ------------------------------------------------------------------------------------

        private void UpdateControls()
        {
            // Nothing can be changed in the middle of a recording or a playback
            bool idle = IsIdle();
            bool hasAction = _workingAction != null;
            ActionNode node = canvas.SelectedNode;
            ActionLink link = canvas.SelectedLink;
            bool macro = node != null && node.IsMacro;

            cboActionPreset.Enabled = idle;
            btnActionNew.Enabled = idle;
            btnActionDelete.Enabled = idle && _activeAction != null;
            btnActionSave.Enabled = idle && IsActionDirty();

            canvas.Locked = !idle;
            btnNewMacro.Enabled = idle && hasAction && !canvas.IsLinking;
            btnLinkMacro.Visible = node != null && node.Kind != ActionNodeKind.End;
            btnLinkMacro.Enabled = idle && !canvas.IsLinking;
            btnDeleteSelected.Visible = macro || link != null;
            btnDeleteSelected.Text = link != null ? "Delete link" : "Delete macro";
            btnDeleteSelected.Enabled = idle && !canvas.IsLinking;
            btnFit.Enabled = hasAction;

            // The macro
            bool ownMacro = macro && node == _macroNode;
            txtNodeName.Enabled = idle;
            cboNodeType.Enabled = idle;
            chkFinal.Enabled = idle;
            chkEnabled.Enabled = idle;
            txtDelay.Enabled = idle;
            pickupEditor.Enabled = idle;
            btnMacroStepDelete.Enabled = idle && ownMacro && lstActions.SelectedIndices.Count > 0
                && lstActions.SelectedIndices[0] < _automationModule.Actions.Count;

            // Its links
            ActionLink listLink = GetSelectedListLink();
            int listIndex = lstLinks.SelectedIndices.Count > 0 ? lstLinks.SelectedIndices[0] : -1;
            btnLinkEdit.Enabled = listLink != null;
            btnLinkUp.Enabled = idle && listIndex > 0;
            btnLinkDown.Enabled = idle && listIndex >= 0 && listIndex < lstLinks.Items.Count - 1;
            btnLinkRemove.Enabled = idle && listLink != null;

            // The condition of a link
            cboType.Enabled = idle;
            numValue.Enabled = idle;
            numInterval.Enabled = idle;
            numTolerance.Enabled = idle;
            numPercent.Enabled = idle;
            btnCapture.Enabled = idle;
            btnTest.Enabled = _sample != null;
            btnLinkDelete.Enabled = idle;

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            string text;
            Color color = SystemColors.ControlText;
            if (_automationModule.IsRecording)
            {
                string into = _macroNode == null ? "" : " into \"" + _macroNode.DisplayName + "\"";
                text = "RECORDING" + into + "... " + _automationModule.Actions.Count + " actions. Press Ctrl + Up to stop.";
                color = Color.Red;
            }
            else if (_automationModule.IsRunningAction)
            {
                ActionNode running = GetRunningNode();
                string finalSteps = "final macro done " + _automationModule.FinalStepCount + " times"
                    + (_automationModule.StopAfterFinalSteps > 0 ? " (" + _automationModule.StopAfterFinalSteps + " left)" : "");
                text = "RUNNING ACTION, macro \"" + (running == null ? "?" : running.DisplayName) + "\", loop " + _automationModule.LoopCount + ", " + finalSteps
                    + ". Press Ctrl + Down to stop.\n" + _automationModule.LastCheckInfo;
                color = Color.Green;
            }
            else if (_automationModule.IsPlaying)
            {
                text = "PLAYING, loop " + _automationModule.LoopCount + ". Press Ctrl + Down to stop.\n" + _automationModule.LastCheckInfo;
                color = Color.Green;
            }
            else if (!_automationModule.IsToolboxStarted)
            {
                text = "The toolbox is OFF. Start it (Ctrl + B) to record or play.";
            }
            else if (_workingAction == null)
            {
                text = "Create an action preset with the New button to use the automation.";
            }
            else
            {
                text = "Ready. Ctrl + Up records into the selected macro, Ctrl + Down runs the action from the Start.";
            }

            if (IsIdle() && _automationModule.LastMessage != "")
            {
                text = _automationModule.LastMessage + "\n" + text;
            }

            lblStatus.ForeColor = color;
            lblStatus.Text = text;
        }

        private void tmrStatus_Tick(object sender, EventArgs e)
        {
            UpdateStopAfterCountdown();
            UpdateStatus();
            UpdateRunningSelection();
        }

        // While playing, follow the action: the running macro is selected on the canvas, so the right panel
        // shows its recorded steps, and the step being played is selected in that list
        private void UpdateRunningSelection()
        {
            if (!_automationModule.IsPlaying)
            {
                canvas.RunningNodeId = null;
                _shownRunningId = null;
                _shownAction = -1;
                return;
            }

            ActionNode running = GetRunningNode();
            canvas.RunningNodeId = running?.Id;
            if (running != null && running.Id != _shownRunningId)
            {
                // Only when the action moves to another macro, the user can still look at something else meanwhile
                _shownRunningId = running.Id;
                canvas.Select(running);
            }

            // The list shows the selected macro, its actions can be followed if that's the one playing
            bool showAction = running != null && canvas.SelectedNode == running;
            int actionIndex = _automationModule.CurrentActionIndex;
            if (showAction && actionIndex != _shownAction && actionIndex >= 0 && actionIndex < lstActions.Items.Count)
            {
                _shownAction = actionIndex;
                lstActions.Items[actionIndex].Selected = true;
                lstActions.Items[actionIndex].EnsureVisible();
            }
        }
    }
}
