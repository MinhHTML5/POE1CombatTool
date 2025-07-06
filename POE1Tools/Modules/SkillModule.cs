using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using POE1Tools.Utilities;

namespace POE1Tools.Modules
{
    public class SkillModule
    {
        public Main _main;
        public WindowsUtil _windowsUtil;
        public InputHook _inputHook;
        public FlaskModule _flaskModule;

        private List<bool> _useSkillHighLifeIndexArray = new List<bool>();
        private List<bool> _useSkillLowLifeIndexArray = new List<bool>();
        private List<bool> _useSkillLatencyIndexArray = new List<bool>();
        private List<int> _useSkillCooldownArray = new List<int>();

        private List<int> _skillCooldownCountArray = new List<int>();

        public SkillModule(Main main, WindowsUtil windowsUtil, InputHook inputHook, FlaskModule flaskModule)
        {
            _main = main;
            _windowsUtil = windowsUtil;
            _inputHook = inputHook;
            _flaskModule = flaskModule;

            for (int i = 0; i<4; i++)
            {
                _useSkillHighLifeIndexArray.Add(false);
                _useSkillLowLifeIndexArray.Add(false);
                _useSkillLatencyIndexArray.Add(false);
                _useSkillCooldownArray.Add(4000);
                _skillCooldownCountArray.Add(0);
            }
        }

        public void Start()
        {

        }

        public void Stop()
        {

        }

        public void SetUseSkillHighLife(int index, bool value)
        {
            if (index < 4)
            {
                _useSkillHighLifeIndexArray[index] = value;
            }
        }
        public void SetUseSkillLowLife(int index, bool value)
        {
            if (index < 4)
            {
                _useSkillLowLifeIndexArray[index] = value;
            }
        }
        public void SetUseSkillLatency(int index, bool value)
        {
            if (index < 4)
            {
                _useSkillLatencyIndexArray[index] = value;
            }
        }

        public void SetSkillCooldown(int index, int value)
        {
            if (index < 4)
            {
                _useSkillCooldownArray[index] = value;
            }
        }



        public void MainLoop(int deltaTime, bool shouldDoLogic, bool started)
        {
            for (int i = 0; i < 4; i++)
            {
                if (_skillCooldownCountArray[i] <= 0 && started && shouldDoLogic)
                {
                    if (_useSkillHighLifeIndexArray[i] == true && _flaskModule.isHighLifeRecently == true)
                    {
                        UseSkill(i);
                    }
                    else if (_useSkillLowLifeIndexArray[i] == true && _flaskModule.isLowLifeRecently == true)
                    {
                        UseSkill(i);
                    }
                    else if (_useSkillLatencyIndexArray[i] == true)
                    {
                        UseSkill(i);
                    }
                }
                else
                {
                    _skillCooldownCountArray[i] -= deltaTime;
                    if (_skillCooldownCountArray[i] < 0) _skillCooldownCountArray[i] = 0;
                }

                switch (i)
                {
                    case 0:
                        _main.txtSkillCooldownDebug1.Text = _skillCooldownCountArray[i].ToString();
                        break;
                    case 1:
                        _main.txtSkillCooldownDebug2.Text = _skillCooldownCountArray[i].ToString();
                        break;
                    case 2:
                        _main.txtSkillCooldownDebug3.Text = _skillCooldownCountArray[i].ToString();
                        break;
                    case 3:
                        _main.txtSkillCooldownDebug4.Text = _skillCooldownCountArray[i].ToString();
                        break;
                }
            }
        }

        public void UseSkill(int index)
        {
            _skillCooldownCountArray[index] = _useSkillCooldownArray[index];
            switch (index)
            {
                case 0:
                    _inputHook.PressKey(Keys.W, true);
                    break;
                case 1:
                    _inputHook.PressKey(Keys.E, true);
                    break;
                case 2:
                    _inputHook.PressKey(Keys.R, true);
                    break;
                case 3:
                    _inputHook.PressKey(Keys.T, true);
                    break;
            }
        }

    }
}
