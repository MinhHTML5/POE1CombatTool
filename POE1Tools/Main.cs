using System;
using System.Drawing;
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
        private WASDModule _wasdModule;

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
            _wasdModule = new WASDModule(this, _windowsUtil, _inputHook, _colorUtil);

            _inputHook.RegisterRawInputDevices(this.Handle, OnMouseKeyEvent, OnKeyEvent);

            _timer.Interval = UPDATE_INTERVAL;
            _timer.Tick += (s, e) => MainLoop();
            _timer.Start();
            _stopwatch.Start();

            LoadSettings();
        }

        private void Start()
        {
            SaveSettings();
            btnStartStop.Text = "STOP";
            _flaskModule.Start();
            _skillModule.Start();
            _wasdModule.Start();
        }

        private void Stop()
        {
            btnStartStop.Text = "START";
            _flaskModule.Stop();
            _skillModule.Stop();
            _wasdModule.Stop();
        }

        private void MainLoop()
        {
            // This variable turn off all submodule from doing logic, but still let them to count cooldown
            bool shouldDoLogic = true;
            int deltaTime = (int)(_stopwatch.Elapsed.TotalMilliseconds);
            lblDeltaTime.Text = deltaTime.ToString();
            _stopwatch.Restart();

            // Check for game focus
            if (_windowsUtil.GetCurrentWindowsProcessName() != "PathOfExile" && started)
            {
                shouldDoLogic = false;
                lblMessage.Text = "POE is out of focus. Current window: " + _windowsUtil.GetCurrentWindowsProcessName();
            }

            // Check for loading screen
            if (shouldDoLogic && started)
            {
                Color cornerColor1 = _colorUtil.GetColorAt(new Point(5, Screen.PrimaryScreen.Bounds.Height - 4));
                Color cornerColor2 = _colorUtil.GetColorAt(new Point(Screen.PrimaryScreen.Bounds.Width - 4, Screen.PrimaryScreen.Bounds.Height - 4));
                if (_colorUtil.IsColorSimilar(cornerColor1, Color.Black, COLOR_TOLERANCE)
                && _colorUtil.IsColorSimilar(cornerColor2, Color.Black, COLOR_TOLERANCE))
                {
                    shouldDoLogic = false;
                    lblMessage.Text = "POE is in loading screen. (Probably)";
                }
            }

            if (!started)
            {
                lblMessage.Text = "Toolbox is not started...";
            }
            else if (shouldDoLogic)
            {
                lblMessage.Text = "Toolbox is working...";
            }

            _flaskModule.MainLoop(deltaTime, shouldDoLogic, started);
            _skillModule.MainLoop(deltaTime, shouldDoLogic, started);
            _wasdModule.MainLoop(deltaTime, shouldDoLogic, started);
        }





        private void OnKeyEvent(Keys key, bool isDown, bool isControlDown)
        {
            if ((key == Keys.B || key == Keys.Space) && !isDown && isControlDown)
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
                _wasdModule.HandleWASD(key, isDown);
            }
        }

        private void OnMouseKeyEvent(MouseButtons key, bool isDown)
        {
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


            Properties.Settings.Default.Save();
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
    }
}
