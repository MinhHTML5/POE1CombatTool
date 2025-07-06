using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using POE1Tools.Utilities;

namespace POE1Tools.Modules
{
    public class FlaskModule
    {
        public const int COLOR_TOLERANCE = 5;

        public const int FLASK_COOLDOWN = 100;

        public const float HIGH_LIFE_PIXEL_X = 0.062f;
        public const float HIGH_LIFE_PIXEL_Y = 0.84f;
        public const float LOW_LIFE_PIXEL_X = 0.062f;
        public const float LOW_LIFE_PIXEL_Y = 0.905f;
        public const float MANA_PIXEL_X = 0.94f;
        public const float MANA_PIXEL_Y = 0.96f;

        public Main _main;
        public WindowsUtil _windowsUtil;
        public InputHook _inputHook;
        public ColorUtil _colorUtil;

        private bool _sampled = false;

        private List<bool> healLowFlaskIndexArray = new List<bool>();
        private List<bool> healHighFlaskIndexArray = new List<bool>();
        private List<bool> manaFlaskIndexArray = new List<bool>();
        private List<bool> latencyFlaskIndexArray = new List<bool>();
        private List<int> latencyAmountArray = new List<int>();

        private int _autoHealLowIndex = 0;
        private int _autoHealLowCooldown = 0;
        private Color _autoHealLowColor = Color.Black;
        private Point _autoHealLowPoint = new Point();

        private int _autoHealHighIndex = 0;
        private int _autoHealHighCooldown = 0;
        private Color _autoHealHighColor = Color.Black;
        private Point _autoHealHighPoint = new Point();

        private int _autoManaIndex = 0;
        private int _autoManaCooldown = 0;
        private Color _autoManaColor = Color.Black;
        private Point _autoManaPoint = new Point();

        private List<int> _autoFlaskLatencyCooldown = new List<int>();

        public bool isHighLifeRecently = false;
        public bool isLowLifeRecently = false;
        public bool isLowManaRecently = false;




        public FlaskModule(Main main, WindowsUtil windowsUtil, InputHook inputHook, ColorUtil colorUtil)
        {
            _main = main;
            _windowsUtil = windowsUtil;
            _inputHook = inputHook;
            _colorUtil = colorUtil;

            _autoHealLowPoint = _colorUtil.GetPixelPosition(LOW_LIFE_PIXEL_X, LOW_LIFE_PIXEL_Y);
            _autoHealHighPoint = _colorUtil.GetPixelPosition(HIGH_LIFE_PIXEL_X, HIGH_LIFE_PIXEL_Y);
            _autoManaPoint = _colorUtil.GetPixelPosition(MANA_PIXEL_X, MANA_PIXEL_Y);

            for (int i = 0; i < 5; i++)
            {
                healLowFlaskIndexArray.Add(false);
                healHighFlaskIndexArray.Add(false);
                manaFlaskIndexArray.Add(false);
                latencyFlaskIndexArray.Add(false);
                latencyAmountArray.Add(4000);
                _autoFlaskLatencyCooldown.Add(0);
            }
        }

        public void Sample()
        {
            _autoHealLowColor = _colorUtil.GetColorAt(_autoHealLowPoint);
            _main.pnlLowLifeSample.BackColor = _autoHealLowColor;

            _autoHealHighColor = _colorUtil.GetColorAt(_autoHealHighPoint);
            _main.pnlHighLifeSample.BackColor = _autoHealHighColor;

            _autoManaColor = _colorUtil.GetColorAt(_autoManaPoint);
            _main.pnlManaSample.BackColor = _autoManaColor;

            _sampled = true;
        }

        public void Start()
        {
            if (!_sampled) Sample();

            _windowsUtil.AddDebugDrawPoint(_autoHealLowPoint);
            _windowsUtil.AddDebugDrawPoint(_autoHealHighPoint);
            _windowsUtil.AddDebugDrawPoint(_autoManaPoint);
            _windowsUtil.SetDrawTextOn(true);
        }

        public void Stop()
        {
            _windowsUtil.RemoveDebugDrawPoint(_autoHealLowPoint);
            _windowsUtil.RemoveDebugDrawPoint(_autoHealHighPoint);
            _windowsUtil.RemoveDebugDrawPoint(_autoManaPoint);

            _windowsUtil.SetDrawTextOn(false);
        }

        
        public void SetFlaskHealLow (int index, bool value)
        {
            if (index < 5)
            {
                healLowFlaskIndexArray[index] = value;
            }
        }

        public void SetFlaskHealHigh(int index, bool value)
        {
            if (index < 5)
            {
                healHighFlaskIndexArray[index] = value;
            }
        }

        public void SetFlaskMana(int index, bool value)
        {
            if (index < 5)
            {
                manaFlaskIndexArray[index] = value;
            }
        }

        public void SetFlaskLatency(int index, bool value)
        {
            if (index < 5)
            {
                latencyFlaskIndexArray[index] = value;
            }
        }

        public void SetFlaskLatencyValue(int index, int value)
        {
            if (index < 5)
            {
                latencyAmountArray[index] = value;
            }
        }



        public void MainLoop(int deltaTime, bool shouldDoLogic, bool started)
        {
            // =============================================================================================================================================
            // Auto heal on low life
            // =============================================================================================================================================
            if (_autoHealLowCooldown <= 0 && started && shouldDoLogic)
            {
                Color lowLifeColor = _colorUtil.GetColorAt(_autoHealLowPoint);
                _main.pnlLowLifeNow.BackColor = lowLifeColor;
                if (!_colorUtil.IsColorSimilar(lowLifeColor, _autoHealLowColor, COLOR_TOLERANCE))
                {
                    _autoHealLowCooldown = FLASK_COOLDOWN;
                    for (int i = 1; i <= 5; i++)
                    {
                        int checkIndex = _autoHealLowIndex + i;
                        if (checkIndex > 4) checkIndex -= 5;
                        if (healLowFlaskIndexArray[checkIndex] == true)
                        {
                            PressFlaskKey(checkIndex);
                            _autoHealLowIndex = checkIndex;
                            break;
                        }
                    }
                    isLowLifeRecently = true;
                }
                else
                {
                    isLowLifeRecently = false;
                }
            }
            else
            {
                _autoHealLowCooldown -= deltaTime;
                if (_autoHealLowCooldown < 0) _autoHealLowCooldown = 0;
            }

            // =============================================================================================================================================
            // Auto heal to top up life
            // =============================================================================================================================================
            if (_autoHealHighCooldown <= 0 && started && shouldDoLogic)
            {
                Color highLifeColor = _colorUtil.GetColorAt(_autoHealHighPoint);
                _main.pnlHighLifeNow.BackColor = highLifeColor;
                if (!_colorUtil.IsColorSimilar(highLifeColor, _autoHealHighColor, COLOR_TOLERANCE))
                {
                    _autoHealHighCooldown = FLASK_COOLDOWN;
                    for (int i = 1; i <= 5; i++)
                    {
                        int checkIndex = _autoHealHighIndex + i;
                        if (checkIndex > 4) checkIndex -= 5;
                        if (healHighFlaskIndexArray[checkIndex] == true && _autoFlaskLatencyCooldown[checkIndex] <= 0)
                        {
                            PressFlaskKey(checkIndex);
                            _autoHealHighIndex = checkIndex;
                            _autoFlaskLatencyCooldown[checkIndex] = latencyAmountArray[checkIndex];
                            break;
                        }
                    }

                    isHighLifeRecently = true;
                }
                else
                {
                    isHighLifeRecently = false;
                }
            }
            else
            {
                _autoHealHighCooldown -= deltaTime;
                if (_autoHealHighCooldown < 0) _autoHealHighCooldown = 0;
            }


            // =============================================================================================================================================
            // Auto mana
            // =============================================================================================================================================
            if (_autoManaCooldown <= 0 && started && shouldDoLogic)
            {
                Color manaColor = _colorUtil.GetColorAt(_autoManaPoint);
                _main.pnlManaNow.BackColor = manaColor;
                if (!_colorUtil.IsColorSimilar(manaColor, _autoManaColor, COLOR_TOLERANCE))
                {
                    _autoManaCooldown = FLASK_COOLDOWN;
                    for (int i = 1; i <= 5; i++)
                    {
                        int checkIndex = _autoManaIndex + i;
                        if (checkIndex > 4) checkIndex -= 5;
                        if (manaFlaskIndexArray[checkIndex] == true && _autoFlaskLatencyCooldown[checkIndex] <= 0)
                        {
                            PressFlaskKey(checkIndex);
                            _autoManaIndex = checkIndex;
                            _autoFlaskLatencyCooldown[checkIndex] = latencyAmountArray[checkIndex];
                            break;
                        }
                    }
                }
            }
            else
            {
                _autoManaCooldown -= deltaTime;
                if (_autoManaCooldown < 0) _autoManaCooldown = 0;
            }


            // =============================================================================================================================================
            // Auto flask periodically
            // =============================================================================================================================================
            for (int i = 0; i < 5; i++)
            {
                if (_autoFlaskLatencyCooldown[i] <= 0 && started && shouldDoLogic)
                {
                    if (latencyFlaskIndexArray[i])
                    {
                        _autoFlaskLatencyCooldown[i] = latencyAmountArray[i];
                        PressFlaskKey(i);
                    }
                }
                else
                {
                    _autoFlaskLatencyCooldown[i] -= Main.UPDATE_INTERVAL;
                }
            }

            _main.txtFlaskLowDebug.Text = _autoHealLowCooldown.ToString();
            _main.txtFlaskHighDebug.Text = _autoHealHighCooldown.ToString();
            _main.txtFlaskManaDebug.Text = _autoManaCooldown.ToString();
        }

        public void PressFlaskKey(int index)
        {
            if (index == 0) _inputHook.PressKey(Keys.D1);
            else if (index == 1) _inputHook.PressKey(Keys.D2);
            else if (index == 2) _inputHook.PressKey(Keys.D3);
            else if (index == 3) _inputHook.PressKey(Keys.D4);
            else if (index == 4) _inputHook.PressKey(Keys.D5);
        }
    }
}
