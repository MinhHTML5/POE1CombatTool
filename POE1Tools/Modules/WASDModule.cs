using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using POE1Tools.Utilities;

namespace POE1Tools.Modules
{
    public class WASDModule
    {
        private Main _main;
        private WindowsUtil _windowsUtil;
        private InputHook _inputHook;
        private ColorUtil _colorUtil;

        public const int COOLDOWN = 100;
        public const float CENTER_SCREEN_X = 0.5f;
        public const float CENTER_SCREEN_Y = 0.45f;

        private int _xAxis = 0;
        private int _yAxis = 0;

        private int _cooldown = 0;
        private bool _wasdEnabled = false;
        private bool _started = false;
        private bool _shouldDoLogic = false;

        public WASDModule(Main main, WindowsUtil windowsUtil, InputHook inputHook, ColorUtil colorUtil)
        {
            _main = main;
            _windowsUtil = windowsUtil;
            _inputHook = inputHook;
            _colorUtil = colorUtil;
        }

        public void Start()
        {
            _started = true;
        }
        public void Stop()
        {
            _started = false;
        }

        public void HandleWASD(Keys key, bool isDown)
        {
            if (_wasdEnabled)
            {
                int oldXAxis = _xAxis;
                int oldYAxis = _yAxis;

                if (isDown)
                {
                    if (key == Keys.Left) _xAxis = -1;
                    if (key == Keys.Right) _xAxis = 1;
                    if (key == Keys.Up) _yAxis = -1;
                    if (key == Keys.Down) _yAxis = 1;
                }
                else
                {
                    if (key == Keys.Left && _xAxis == -1) _xAxis = 0;
                    if (key == Keys.Right && _xAxis == 1) _xAxis = 0;
                    if (key == Keys.Up && _yAxis == -1) _yAxis = 0;
                    if (key == Keys.Down && _yAxis == 1) _yAxis = 0;
                }

                if (oldXAxis != _xAxis || oldYAxis != _yAxis)
                {
                    PerformMouseSend();
                }
            }
        }

        public void MainLoop(int deltaTime, bool shouldDoLogic, bool started)
        {
            _shouldDoLogic = shouldDoLogic;
            if (_cooldown > 0)
            {
                _cooldown -= deltaTime;
            }
            else if (_wasdEnabled)
            {
                if (_xAxis != 0 || _yAxis != 0)
                {
                    PerformMouseSend();
                }
            }
        }

        private void PerformMouseSend()
        {
            if (_wasdEnabled && _started && _shouldDoLogic)
            {
                _cooldown = COOLDOWN;

                Point oldPos = _inputHook.GetCurrentMousePosition();
                Point clickPos = _colorUtil.GetPixelPosition(CENTER_SCREEN_X + _xAxis * 0.09f, CENTER_SCREEN_Y + _yAxis * 0.16f);
                _inputHook.MoveMouse(clickPos.X, clickPos.Y);
                _inputHook.ShowMouseCursor(false);

                RestoreMousePos(oldPos.X, oldPos.Y);
            }
        }

        private async void RestoreMousePos(int x, int y)
        {
            await Task.Delay(16); // Wait 10 milliseconds
            _inputHook.SendLeftClick();
            _inputHook.MoveMouse(x, y);
            _inputHook.ShowMouseCursor(true);
        }
    }
}