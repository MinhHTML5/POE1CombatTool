using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using POE1Tools.Modules;
using POE1Tools.Utilities;
using System.Diagnostics;

namespace POE1Tools
{
    public partial class Main : Form
    {
        public static Main sInstance;

        public const int UPDATE_INTERVAL = 40;
        public const int COLOR_TOLERANCE = 5;

        private WindowsUtil _windowsUtil;
        private InputHook _inputHook;
        private ColorUtil _colorUtil;

        private FlaskModule _flaskModule;
        private SkillModule _skillModule;
        private AutomationModule _automationModule;
        private AutomationForm _automationForm;

        public bool started = false;

        private Timer _timer = new Timer();
        private Stopwatch _stopwatch = new Stopwatch();


        public Main(WindowsUtil windowsUtil, InputHook inputHook, ColorUtil colorUtil)
        {
            sInstance = this;

            _windowsUtil = windowsUtil;
            _inputHook = inputHook;
            _colorUtil = colorUtil;

            InitializeComponent();
            CreateScreenSelector();
            CreateHideoutOption();
            CreateHealLowCooldownOption();
            CreateFlaskOffsetOptions();

            // Keep option changes even if the tool is closed without pressing START
            FormClosing += (s, e) => SaveSettings();
        }

        private readonly ClientLogReader _clientLog = new ClientLogReader();
        private CheckBox _chkPauseInHideout;

        private void CreateHideoutOption()
        {
            _chkPauseInHideout = new CheckBox
            {
                AutoSize = true,
                Checked = true,
                Text = "Pause flasks in hideout (reads Client.txt)",
                Location = new Point(521, 8)
            };
            Controls.Add(_chkPauseInHideout);
        }

        private CheckBox _chkHealLowIgnoreCooldown;
        private TextBox _txtHealLowCooldown;

        // Sits in the header row of the "Flask usage" group, right of its title
        private void CreateHealLowCooldownOption()
        {
            _chkHealLowIgnoreCooldown = new CheckBox
            {
                AutoSize = true,
                Text = "Heal low: ignore flask cooldown, use (ms):",
                Location = new Point(90, 0)
            };
            _chkHealLowIgnoreCooldown.CheckedChanged += (s, e) =>
            {
                _txtHealLowCooldown.Enabled = _chkHealLowIgnoreCooldown.Checked;
                _flaskModule?.SetHealLowIgnoreCooldown(_chkHealLowIgnoreCooldown.Checked);
            };

            _txtHealLowCooldown = new TextBox
            {
                Text = "30",
                Enabled = false,
                Location = new Point(355, 0),
                Size = new Size(60, 23)
            };
            _txtHealLowCooldown.TextChanged += (s, e) =>
            {
                if (int.TryParse(_txtHealLowCooldown.Text, out int value) && value >= 0)
                {
                    _txtHealLowCooldown.BackColor = SystemColors.Window;
                    _flaskModule?.SetHealLowCooldown(value);
                }
                else
                {
                    _txtHealLowCooldown.BackColor = Color.LightCoral;
                }
            };

            grpFlask.Controls.Add(_chkHealLowIgnoreCooldown);
            grpFlask.Controls.Add(_txtHealLowCooldown);
            _chkHealLowIgnoreCooldown.BringToFront();
            _txtHealLowCooldown.BringToFront();
        }

        private TextBox[] _txtFlaskOffsets;
        private readonly ToolTip _toolTip = new ToolTip();

        // An offset box right of each flask's "Periodically" duration box: how long after START (or after
        // leaving the hideout) the flask is pressed for the first time
        private void CreateFlaskOffsetOptions()
        {
            TextBox[] latencies = { txtFlaskLatency1, txtFlaskLatency2, txtFlaskLatency3, txtFlaskLatency4, txtFlaskLatency5 };
            _txtFlaskOffsets = new TextBox[latencies.Length];
            for (int i = 0; i < latencies.Length; i++)
            {
                TextBox latency = latencies[i];
                int fullWidth = latency.Width;
                latency.Width = (fullWidth - 6) / 2;

                int index = i;
                TextBox offset = new TextBox
                {
                    PlaceholderText = "Offset",
                    Location = new Point(latency.Right + 6, latency.Top),
                    Size = new Size(fullWidth - latency.Width - 6, latency.Height),
                    WordWrap = false
                };
                offset.TextChanged += (s, e) =>
                {
                    // Empty means no offset
                    if (offset.Text.Trim() == "")
                    {
                        offset.BackColor = SystemColors.Window;
                        _flaskModule?.SetFlaskOffset(index, 0);
                    }
                    else if (int.TryParse(offset.Text, out int value) && value >= 0)
                    {
                        offset.BackColor = SystemColors.Window;
                        _flaskModule?.SetFlaskOffset(index, value);
                    }
                    else
                    {
                        offset.BackColor = Color.LightCoral;
                    }
                };

                _toolTip.SetToolTip(latency, "Periodically: press the flask every this many ms");
                _toolTip.SetToolTip(offset, "Offset (ms): wait this long after START or leaving the hideout before the first press");
                latency.Parent.Controls.Add(offset);
                _txtFlaskOffsets[i] = offset;
            }
        }

        private ComboBox _cboScreen;
        private Label _lblScreen;
        private Button _btnDetectScreen;

        private void CreateScreenSelector()
        {
            _lblScreen = new Label { AutoSize = true, Text = "Game screen:", Location = new Point(300, 22) };
            _cboScreen = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(300, 40),
                Size = new Size(140, 23)
            };
            _btnDetectScreen = new Button
            {
                Text = "Detect",
                Location = new Point(446, 39),
                Size = new Size(60, 25)
            };
            _btnDetectScreen.Click += (s, e) => DetectGameScreen();
            PopulateScreens();
            _cboScreen.SelectedIndexChanged += cboScreen_SelectedIndexChanged;
            Controls.Add(_lblScreen);
            Controls.Add(_cboScreen);
            Controls.Add(_btnDetectScreen);
        }

        private void PopulateScreens()
        {
            Screen[] screens = Screen.AllScreens;
            var names = MonitorNames.GetAll();
            _cboScreen.Items.Clear();
            for (int i = 0; i < screens.Length; i++)
            {
                Screen s = screens[i];
                string name = names.TryGetValue(s.DeviceName, out string n) ? n : $"Screen {i + 1}";
                _cboScreen.Items.Add(new ScreenItem(s, name + (s.Primary ? " (Primary)" : ""), s.DeviceName + "|" + name));
            }
            int selected = Array.FindIndex(screens, s => s.DeviceName == ScreenHelper.Current.DeviceName);
            _cboScreen.SelectedIndex = selected >= 0 ? selected : 0;
        }

        private void cboScreen_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cboScreen.SelectedItem is ScreenItem item)
            {
                ScreenHelper.Current = item.Screen;
                _windowsUtil.MoveOverlayToScreen(item.Screen);
                _flaskModule?.RefreshPoints();
            }
        }

        private class ScreenItem
        {
            public Screen Screen;
            public string Key; // "DeviceName|MonitorName", stored in settings
            private string _text;
            public ScreenItem(Screen screen, string text, string key) { Screen = screen; _text = text; Key = key; }
            public override string ToString() => _text;
        }

        private void SelectScreen(Screen screen)
        {
            int index = _cboScreen.Items.Cast<ScreenItem>().ToList().FindIndex(i => i.Screen.DeviceName == screen.DeviceName);
            if (index < 0) return;
            _cboScreen.SelectedIndex = index;
            // SelectedIndexChanged doesn't fire if the index is unchanged; make sure state is applied
            cboScreen_SelectedIndexChanged(null, EventArgs.Empty);
        }

        // Select the saved screen; fall back to the primary screen if it isn't connected.
        private void SelectSavedScreen(string key)
        {
            int index = -1;
            if (!string.IsNullOrEmpty(key))
            {
                string savedName = key.Contains('|') ? key.Substring(key.IndexOf('|') + 1) : key;
                string savedDevice = key.Contains('|') ? key.Substring(0, key.IndexOf('|')) : key;
                for (int i = 0; i < _cboScreen.Items.Count && index < 0; i++)
                    if (((ScreenItem)_cboScreen.Items[i]).Key == key) index = i;
                for (int i = 0; i < _cboScreen.Items.Count && index < 0; i++)
                    if (((ScreenItem)_cboScreen.Items[i]).Key.EndsWith("|" + savedName) && !savedName.StartsWith("Screen ")) index = i;
                for (int i = 0; i < _cboScreen.Items.Count && index < 0; i++)
                    if (((ScreenItem)_cboScreen.Items[i]).Screen.DeviceName == savedDevice) index = i;
            }
            if (index < 0)
                index = _cboScreen.Items.Cast<ScreenItem>().ToList().FindIndex(s => s.Screen.Primary);
            _cboScreen.SelectedIndex = index < 0 ? 0 : index;
            cboScreen_SelectedIndexChanged(null, EventArgs.Empty);
        }

        // Find the Path of Exile window and switch the dropdown to the screen it is on.
        // Returns false (selection unchanged) if the game isn't running or has no window.
        private bool DetectGameScreen()
        {
            IntPtr hwnd = IntPtr.Zero;
            foreach (Process proc in Process.GetProcesses())
            {
                try
                {
                    if (hwnd == IntPtr.Zero
                        && proc.ProcessName.StartsWith("PathOfExile", StringComparison.OrdinalIgnoreCase)
                        && proc.MainWindowHandle != IntPtr.Zero)
                    {
                        hwnd = proc.MainWindowHandle;
                    }
                }
                catch { }
                finally { proc.Dispose(); }
            }
            if (hwnd == IntPtr.Zero) return false;

            SelectScreen(Screen.FromHandle(hwnd));
            return true;
        }

        private const int WM_INPUT = 0x00FF;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_INPUT)
            {
                _inputHook.ProcessRawInput(m.LParam);
            }

            base.WndProc(ref m);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            _flaskModule = new FlaskModule(this, _windowsUtil, _inputHook, _colorUtil);
            _skillModule = new SkillModule(this, _windowsUtil, _inputHook, _flaskModule);
            _automationModule = new AutomationModule(this, _windowsUtil, _inputHook);

            _inputHook.RegisterRawInputDevices(this.Handle, OnMouseKeyEvent, OnKeyEvent);

            _timer.Interval = UPDATE_INTERVAL;
            _timer.Tick += (s, e) => MainLoop();
            _timer.Start();
            _stopwatch.Start();

            SharedSettings.Import(); // pick up settings saved by another build (Debug / Release)
            LoadSettings();
            DetectGameScreen(); // if the game is running, its screen wins over the saved one
        }

        private void Start()
        {
            SaveSettings();
            btnStartStop.Text = "STOP";
            _flaskModule.Start();
            _skillModule.Start();
            _automationModule.Start();
        }

        private void Stop()
        {
            btnStartStop.Text = "START";
            _flaskModule.Stop();
            _skillModule.Stop();
            _automationModule.Stop();
        }

        private const int LOADING_SCREEN_DELAY = 300;
        private int _blackCornersMs = 0;
        private int _housekeepingMs = 0;
        private bool _wasHideoutDisabled = false;

        // The game is focused: if its window is on a different screen than the selected one, switch to it
        private void FollowGameScreen()
        {
            IntPtr hwnd = _windowsUtil.GetForegroundWindowHandle();
            if (hwnd == IntPtr.Zero) return;
            Screen gameScreen = Screen.FromHandle(hwnd);
            if (gameScreen.DeviceName != ScreenHelper.Current.DeviceName)
            {
                SelectScreen(gameScreen);
            }
        }

        private void MainLoop()
        {
            // This variable turn off all submodule from doing logic, but still let them to count cooldown
            bool shouldDoLogic = true;
            int deltaTime = (int)(_stopwatch.Elapsed.TotalMilliseconds);
            lblDeltaTime.Text = deltaTime.ToString();
            _stopwatch.Restart();

            // Check for game focus. Steam / x64 / Kakao / Epic builds all start with "PathOfExile"
            string currentProcessName = _windowsUtil.GetCurrentWindowsProcessName();
            bool inFocus = currentProcessName.StartsWith("PathOfExile", StringComparison.OrdinalIgnoreCase);
            if (!inFocus && started)
            {
                shouldDoLogic = false;
                lblMessage.Text = "POE is out of focus. Current window: " + currentProcessName;
            }

            // Once a second: follow the game if it is on another screen, and keep the overlay on top
            _housekeepingMs += deltaTime;
            if (_housekeepingMs >= 1000)
            {
                _housekeepingMs = 0;
                if (inFocus) FollowGameScreen();
                _windowsUtil.EnsureOverlayTopMost();
            }

            // Check for loading screen. The corners must stay black for a moment, so a dark frame
            // or a brief flicker doesn't pause everything
            if (shouldDoLogic && started)
            {
                Rectangle sb = ScreenHelper.Current.Bounds;
                Color cornerColor1 = _colorUtil.GetColorAt(new Point(sb.X + 5, sb.Y + sb.Height - 4));
                Color cornerColor2 = _colorUtil.GetColorAt(new Point(sb.X + sb.Width - 4, sb.Y + sb.Height - 4));
                if (_colorUtil.IsColorSimilar(cornerColor1, Color.Black, COLOR_TOLERANCE)
                && _colorUtil.IsColorSimilar(cornerColor2, Color.Black, COLOR_TOLERANCE))
                {
                    _blackCornersMs += deltaTime;
                    if (_blackCornersMs >= LOADING_SCREEN_DELAY)
                    {
                        shouldDoLogic = false;
                        lblMessage.Text = "POE is in loading screen. (Probably)";
                    }
                }
                else
                {
                    _blackCornersMs = 0;
                }
            }

            if (!started)
            {
                lblMessage.Text = "Toolbox is not started...";
            }
            else if (shouldDoLogic)
            {
                lblMessage.Text = _automationModule.IsPlaying ? "Toolbox is working... (Automation mode)" : "Toolbox is working...";
            }

            // Hideout detection from Client.txt: flasks are paused there, and resume on entering a map
            bool flaskLogic = shouldDoLogic;
            bool hideoutDisabled = false;
            if (_chkPauseInHideout.Checked)
            {
                _clientLog.Poll();
                if (_clientLog.InHideout)
                {
                    hideoutDisabled = true;
                    flaskLogic = false;
                    if (shouldDoLogic && started) lblMessage.Text = "In hideout: flasks paused.";
                }
            }
            _windowsUtil.SetHideoutDisabled(hideoutDisabled);
            // Leaving the hideout counts as a fresh start for the flask offsets
            if (_wasHideoutDisabled && !hideoutDisabled && started) _flaskModule.ApplyOffsets();
            _wasHideoutDisabled = hideoutDisabled;

            _flaskModule.MainLoop(deltaTime, flaskLogic, started);
            _skillModule.MainLoop(deltaTime, shouldDoLogic, started);
            // Automation keeps playing through loading screens, it only cares about the game focus
            _automationModule.MainLoop(deltaTime, inFocus, started);
        }





        private void OnKeyEvent(Keys key, bool isDown, bool isControlDown)
        {
            _automationModule.OnKeyEvent(key, isDown, isControlDown);

            // Only a real keyboard Ctrl+B / Ctrl+Space toggles; ignore keys sent by software (ours included)
            if ((key == Keys.B || key == Keys.Space) && !isDown && isControlDown && !_inputHook.LastEventInjected)
            {
                started = !started;
                if (started)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
            else if ((key == Keys.Left || key == Keys.Up || key == Keys.Right || key == Keys.Down) && isControlDown == false)
            {

            }
        }

        private void OnMouseKeyEvent(MouseButtons key, bool isDown)
        {
            _automationModule.OnMouseEvent(key, isDown);

            if (key == MouseButtons.XButton2 && isDown)
            {
                //_rakeModule.RakeKeyDown();
            }

            if (key == MouseButtons.XButton2 && !isDown)
            {
                //_rakeModule.RakeKeyRelease();
            }
        }

        private void lblSample_Click(object sender, EventArgs e)
        {
            _flaskModule.Sample();
        }
        private void btnStartStop_Click(object sender, EventArgs e)
        {
            started = !started;
            if (started)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        private void chkAutoHealLow_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            int index = 0;
            if (checkBox == chkAutoHealLow1) index = 0;
            else if (checkBox == chkAutoHealLow2) index = 1;
            else if (checkBox == chkAutoHealLow3) index = 2;
            else if (checkBox == chkAutoHealLow4) index = 3;
            else if (checkBox == chkAutoHealLow5) index = 4;
            _flaskModule.SetFlaskHealLow(index, checkBox.Checked);
        }

        private void chkAutoHealHigh_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            int index = 0;
            if (checkBox == chkAutoHealHigh1) index = 0;
            else if (checkBox == chkAutoHealHigh2) index = 1;
            else if (checkBox == chkAutoHealHigh3) index = 2;
            else if (checkBox == chkAutoHealHigh4) index = 3;
            else if (checkBox == chkAutoHealHigh5) index = 4;
            _flaskModule.SetFlaskHealHigh(index, checkBox.Checked);
        }

        private void chkAutoMana_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            int index = 0;
            if (checkBox == chkAutoMana1) index = 0;
            else if (checkBox == chkAutoMana2) index = 1;
            else if (checkBox == chkAutoMana3) index = 2;
            else if (checkBox == chkAutoMana4) index = 3;
            else if (checkBox == chkAutoMana5) index = 4;
            _flaskModule.SetFlaskMana(index, checkBox.Checked);
        }

        private void chkFlaskLatency_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            int index = 0;
            if (checkBox == chkFlaskLatency1) index = 0;
            else if (checkBox == chkFlaskLatency2) index = 1;
            else if (checkBox == chkFlaskLatency3) index = 2;
            else if (checkBox == chkFlaskLatency4) index = 3;
            else if (checkBox == chkFlaskLatency5) index = 4;
            _flaskModule.SetFlaskLatency(index, checkBox.Checked);
        }

        private void txtLatency_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int index = 0;
            if (textBox == txtFlaskLatency1) index = 0;
            else if (textBox == txtFlaskLatency2) index = 1;
            else if (textBox == txtFlaskLatency3) index = 2;
            else if (textBox == txtFlaskLatency4) index = 3;
            else if (textBox == txtFlaskLatency5) index = 4;

            if (!int.TryParse(textBox.Text, out int textValue))
            {
                textBox.BackColor = Color.LightCoral;
            }
            else
            {
                textBox.BackColor = SystemColors.Window;
                _flaskModule.SetFlaskLatencyValue(index, textValue);
            }
        }

        private void chkSkillHighLife_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            int index = 0;
            if (checkBox == chkSkillHighLife1) index = 0;
            else if (checkBox == chkSkillHighLife2) index = 1;
            else if (checkBox == chkSkillHighLife3) index = 2;
            else if (checkBox == chkSkillHighLife4) index = 3;
            _skillModule.SetUseSkillHighLife(index, checkBox.Checked);

            if (checkBox.Checked)
            {
                if (index == 0) chkSkillLatency1.Checked = false;
                else if (index == 1) chkSkillLatency2.Checked = false;
                else if (index == 2) chkSkillLatency3.Checked = false;
                else if (index == 3) chkSkillLatency4.Checked = false;
            }
        }

        private void chkSkillLowLife_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            int index = 0;
            if (checkBox == chkSkillLowLife1) index = 0;
            else if (checkBox == chkSkillLowLife2) index = 1;
            else if (checkBox == chkSkillLowLife3) index = 2;
            else if (checkBox == chkSkillLowLife4) index = 3;
            _skillModule.SetUseSkillLowLife(index, checkBox.Checked);

            if (checkBox.Checked)
            {
                if (index == 0) chkSkillLatency1.Checked = false;
                else if (index == 1) chkSkillLatency2.Checked = false;
                else if (index == 2) chkSkillLatency3.Checked = false;
                else if (index == 3) chkSkillLatency4.Checked = false;
            }
        }

        private void chkSkillLatency_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            int index = 0;
            if (checkBox == chkSkillLatency1) index = 0;
            else if (checkBox == chkSkillLatency2) index = 1;
            else if (checkBox == chkSkillLatency3) index = 2;
            else if (checkBox == chkSkillLatency4) index = 3;
            _skillModule.SetUseSkillLatency(index, checkBox.Checked);

            if (checkBox.Checked)
            {
                if (index == 0) chkSkillLowLife1.Checked = false;
                else if (index == 1) chkSkillLowLife2.Checked = false;
                else if (index == 2) chkSkillLowLife3.Checked = false;
                else if (index == 3) chkSkillLowLife4.Checked = false;
            }
        }

        private void txtSkillCooldown_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int index = 0;
            if (textBox == txtSkillCooldown1) index = 0;
            else if (textBox == txtSkillCooldown2) index = 1;
            else if (textBox == txtSkillCooldown3) index = 2;
            else if (textBox == txtSkillCooldown4) index = 3;

            if (!int.TryParse(textBox.Text, out int textValue))
            {
                textBox.BackColor = Color.LightCoral;
            }
            else
            {
                textBox.BackColor = SystemColors.Window;
                _skillModule.SetSkillCooldown(index, textValue);
            }
        }

        private void chkDrawDebug_CheckedChanged(object sender, EventArgs e)
        {
            _windowsUtil.SetDebugEnabled(chkDrawDebug.Checked);
        }


        private void UpdateRateChange()
        {
            int value = trkUpdateRate.Value * 5;
            lblUpdateRate.Text = value.ToString() + "ms";
            _timer.Interval = value;
        }
        private void trkUpdateRate_Scroll(object sender, EventArgs e)
        {
            UpdateRateChange();
        }


        private void SaveSettings()
        {
            Properties.Settings.Default.chkAutoHealLow1 = chkAutoHealLow1.Checked;
            Properties.Settings.Default.chkAutoHealLow2 = chkAutoHealLow2.Checked;
            Properties.Settings.Default.chkAutoHealLow3 = chkAutoHealLow3.Checked;
            Properties.Settings.Default.chkAutoHealLow4 = chkAutoHealLow4.Checked;
            Properties.Settings.Default.chkAutoHealLow5 = chkAutoHealLow5.Checked;

            Properties.Settings.Default.chkAutoHealHigh1 = chkAutoHealHigh1.Checked;
            Properties.Settings.Default.chkAutoHealHigh2 = chkAutoHealHigh2.Checked;
            Properties.Settings.Default.chkAutoHealHigh3 = chkAutoHealHigh3.Checked;
            Properties.Settings.Default.chkAutoHealHigh4 = chkAutoHealHigh4.Checked;
            Properties.Settings.Default.chkAutoHealHigh5 = chkAutoHealHigh5.Checked;

            Properties.Settings.Default.chkAutoMana1 = chkAutoMana1.Checked;
            Properties.Settings.Default.chkAutoMana2 = chkAutoMana2.Checked;
            Properties.Settings.Default.chkAutoMana3 = chkAutoMana3.Checked;
            Properties.Settings.Default.chkAutoMana4 = chkAutoMana4.Checked;
            Properties.Settings.Default.chkAutoMana5 = chkAutoMana5.Checked;

            Properties.Settings.Default.chkFlaskLatency1 = chkFlaskLatency1.Checked;
            Properties.Settings.Default.chkFlaskLatency2 = chkFlaskLatency2.Checked;
            Properties.Settings.Default.chkFlaskLatency3 = chkFlaskLatency3.Checked;
            Properties.Settings.Default.chkFlaskLatency4 = chkFlaskLatency4.Checked;
            Properties.Settings.Default.chkFlaskLatency5 = chkFlaskLatency5.Checked;

            Properties.Settings.Default.txtFlaskLatency1 = txtFlaskLatency1.Text;
            Properties.Settings.Default.txtFlaskLatency2 = txtFlaskLatency2.Text;
            Properties.Settings.Default.txtFlaskLatency3 = txtFlaskLatency3.Text;
            Properties.Settings.Default.txtFlaskLatency4 = txtFlaskLatency4.Text;
            Properties.Settings.Default.txtFlaskLatency5 = txtFlaskLatency5.Text;

            Properties.Settings.Default.txtFlaskOffset1 = _txtFlaskOffsets[0].Text;
            Properties.Settings.Default.txtFlaskOffset2 = _txtFlaskOffsets[1].Text;
            Properties.Settings.Default.txtFlaskOffset3 = _txtFlaskOffsets[2].Text;
            Properties.Settings.Default.txtFlaskOffset4 = _txtFlaskOffsets[3].Text;
            Properties.Settings.Default.txtFlaskOffset5 = _txtFlaskOffsets[4].Text;

            Properties.Settings.Default.chkSkillLatency1 = chkSkillLatency1.Checked;
            Properties.Settings.Default.chkSkillLatency2 = chkSkillLatency2.Checked;
            Properties.Settings.Default.chkSkillLatency3 = chkSkillLatency3.Checked;
            Properties.Settings.Default.chkSkillLatency4 = chkSkillLatency4.Checked;

            Properties.Settings.Default.chkSkillLowLife1 = chkSkillLowLife1.Checked;
            Properties.Settings.Default.chkSkillLowLife2 = chkSkillLowLife2.Checked;
            Properties.Settings.Default.chkSkillLowLife3 = chkSkillLowLife3.Checked;
            Properties.Settings.Default.chkSkillLowLife4 = chkSkillLowLife4.Checked;

            Properties.Settings.Default.txtSkillCooldown1 = txtSkillCooldown1.Text;
            Properties.Settings.Default.txtSkillCooldown2 = txtSkillCooldown2.Text;
            Properties.Settings.Default.txtSkillCooldown3 = txtSkillCooldown3.Text;
            Properties.Settings.Default.txtSkillCooldown4 = txtSkillCooldown4.Text;

            Properties.Settings.Default.chkSkillHighLife1 = chkSkillHighLife1.Checked;
            Properties.Settings.Default.chkSkillHighLife2 = chkSkillHighLife2.Checked;
            Properties.Settings.Default.chkSkillHighLife3 = chkSkillHighLife3.Checked;
            Properties.Settings.Default.chkSkillHighLife4 = chkSkillHighLife4.Checked;

            Properties.Settings.Default.chkDrawDebug = chkDrawDebug.Checked;
            Properties.Settings.Default.trkUpdateRate = trkUpdateRate.Value;
            Properties.Settings.Default.pauseFlasksInHideout = _chkPauseInHideout.Checked;
            Properties.Settings.Default.chkHealLowIgnoreCooldown = _chkHealLowIgnoreCooldown.Checked;
            Properties.Settings.Default.txtHealLowCooldown = _txtHealLowCooldown.Text;
            Properties.Settings.Default.selectedScreen = (_cboScreen.SelectedItem as ScreenItem)?.Key ?? "";

            Properties.Settings.Default.Save();
            SharedSettings.Export();
        }

        private void LoadSettings()
        {
            chkAutoHealLow1.Checked = Properties.Settings.Default.chkAutoHealLow1;
            chkAutoHealLow2.Checked = Properties.Settings.Default.chkAutoHealLow2;
            chkAutoHealLow3.Checked = Properties.Settings.Default.chkAutoHealLow3;
            chkAutoHealLow4.Checked = Properties.Settings.Default.chkAutoHealLow4;
            chkAutoHealLow5.Checked = Properties.Settings.Default.chkAutoHealLow5;

            chkAutoHealHigh1.Checked = Properties.Settings.Default.chkAutoHealHigh1;
            chkAutoHealHigh2.Checked = Properties.Settings.Default.chkAutoHealHigh2;
            chkAutoHealHigh3.Checked = Properties.Settings.Default.chkAutoHealHigh3;
            chkAutoHealHigh4.Checked = Properties.Settings.Default.chkAutoHealHigh4;
            chkAutoHealHigh5.Checked = Properties.Settings.Default.chkAutoHealHigh5;

            chkAutoMana1.Checked = Properties.Settings.Default.chkAutoMana1;
            chkAutoMana2.Checked = Properties.Settings.Default.chkAutoMana2;
            chkAutoMana3.Checked = Properties.Settings.Default.chkAutoMana3;
            chkAutoMana4.Checked = Properties.Settings.Default.chkAutoMana4;
            chkAutoMana5.Checked = Properties.Settings.Default.chkAutoMana5;

            chkFlaskLatency1.Checked = Properties.Settings.Default.chkFlaskLatency1;
            chkFlaskLatency2.Checked = Properties.Settings.Default.chkFlaskLatency2;
            chkFlaskLatency3.Checked = Properties.Settings.Default.chkFlaskLatency3;
            chkFlaskLatency4.Checked = Properties.Settings.Default.chkFlaskLatency4;
            chkFlaskLatency5.Checked = Properties.Settings.Default.chkFlaskLatency5;

            txtFlaskLatency1.Text = Properties.Settings.Default.txtFlaskLatency1;
            txtFlaskLatency2.Text = Properties.Settings.Default.txtFlaskLatency2;
            txtFlaskLatency3.Text = Properties.Settings.Default.txtFlaskLatency3;
            txtFlaskLatency4.Text = Properties.Settings.Default.txtFlaskLatency4;
            txtFlaskLatency5.Text = Properties.Settings.Default.txtFlaskLatency5;

            _txtFlaskOffsets[0].Text = Properties.Settings.Default.txtFlaskOffset1;
            _txtFlaskOffsets[1].Text = Properties.Settings.Default.txtFlaskOffset2;
            _txtFlaskOffsets[2].Text = Properties.Settings.Default.txtFlaskOffset3;
            _txtFlaskOffsets[3].Text = Properties.Settings.Default.txtFlaskOffset4;
            _txtFlaskOffsets[4].Text = Properties.Settings.Default.txtFlaskOffset5;

            chkSkillLatency1.Checked = Properties.Settings.Default.chkSkillLatency1;
            chkSkillLatency2.Checked = Properties.Settings.Default.chkSkillLatency2;
            chkSkillLatency3.Checked = Properties.Settings.Default.chkSkillLatency3;
            chkSkillLatency4.Checked = Properties.Settings.Default.chkSkillLatency4;

            chkSkillLowLife1.Checked = Properties.Settings.Default.chkSkillLowLife1;
            chkSkillLowLife2.Checked = Properties.Settings.Default.chkSkillLowLife2;
            chkSkillLowLife3.Checked = Properties.Settings.Default.chkSkillLowLife3;
            chkSkillLowLife4.Checked = Properties.Settings.Default.chkSkillLowLife4;

            txtSkillCooldown1.Text = Properties.Settings.Default.txtSkillCooldown1;
            txtSkillCooldown2.Text = Properties.Settings.Default.txtSkillCooldown2;
            txtSkillCooldown3.Text = Properties.Settings.Default.txtSkillCooldown3;
            txtSkillCooldown4.Text = Properties.Settings.Default.txtSkillCooldown4;

            chkSkillHighLife1.Checked = Properties.Settings.Default.chkSkillHighLife1;
            chkSkillHighLife2.Checked = Properties.Settings.Default.chkSkillHighLife2;
            chkSkillHighLife3.Checked = Properties.Settings.Default.chkSkillHighLife3;
            chkSkillHighLife4.Checked = Properties.Settings.Default.chkSkillHighLife4;

            chkDrawDebug.Checked = Properties.Settings.Default.chkDrawDebug;
            trkUpdateRate.Value = Properties.Settings.Default.trkUpdateRate;

            _chkPauseInHideout.Checked = Properties.Settings.Default.pauseFlasksInHideout;
            _chkHealLowIgnoreCooldown.Checked = Properties.Settings.Default.chkHealLowIgnoreCooldown;
            _txtHealLowCooldown.Text = Properties.Settings.Default.txtHealLowCooldown;

            PopulateScreens();
            SelectSavedScreen(Properties.Settings.Default.selectedScreen);

            UpdateRateChange();
        }


        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void btnLoadSettings_Click(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void btnAutomation_Click(object sender, EventArgs e)
        {
            if (_automationForm != null && !_automationForm.IsDisposed)
            {
                _automationForm.Activate();
                return;
            }

            _automationForm = new AutomationForm(_automationModule);
            _automationForm.FormClosed += (s, args) => _automationForm = null;
            _automationForm.Show(this);
        }
    }
}
