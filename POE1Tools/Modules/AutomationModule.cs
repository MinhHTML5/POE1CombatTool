using POE1Tools.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace POE1Tools.Modules
{
    // Records the user's mouse clicks / key presses into a macro, and plays it back on a loop.
    // Hotkeys (only while the toolbox is started and the Automation window is opened):
    //   Ctrl + Up:   start / stop recording into the macro selected on the canvas
    //   Ctrl + Down: start / stop the action preset from its Start node
    //   Ctrl + Right: take a screenshot, only while the capture window is opened
    public class AutomationModule
    {
        public const Keys RECORD_HOTKEY = Keys.Up;
        public const Keys PLAYBACK_HOTKEY = Keys.Down;
        public const Keys CAPTURE_HOTKEY = Keys.Right;
        // Never loop faster than this, even if the recording was stopped immediately
        public const int MIN_LOOP_DURATION = 50;
        // Limit of a manually edited time, one day
        public const int MAX_ACTION_TIME = 24 * 60 * 60 * 1000;
        // The cursor is moved this long before a click, the game can miss a click sent together with the move
        public const int MOUSE_MOVE_LEAD_TIME = 50;
        // "Item pickup" macro: the cursor waits there (top right of the screen) while the screen is searched,
        // as the game highlights the label under the cursor, which changes its colors
        public const double PICKUP_NEUTRAL_X_PERCENT = 99.5;
        public const double PICKUP_NEUTRAL_Y_PERCENT = 0.5;
        // Time for the game to draw a frame without the highlight, after the cursor moved away
        public const int PICKUP_SETTLE_TIME = 60;
        public const int PICKUP_CLICK_HOLD_TIME = 30;

        public Main _main;
        public WindowsUtil _windowsUtil;
        public InputHook _inputHook;

        // The working macro. It is loaded from / saved to a preset by the Automation window
        private List<MacroAction> _actions = new List<MacroAction>();
        private int _duration = 0;
        // Set when the working macro is an "Item pickup" one, which has nothing recorded
        private ItemPickupSettings _pickup;

        private bool _windowOpen = false;
        private bool _macroSelected = false;
        private bool _toolboxStarted = false;
        private bool _inFocus = false;
        private bool _hotkeyHeld = false;

        private bool _recording = false;
        private Stopwatch _recordWatch = new Stopwatch();
        private HashSet<Keys> _recordHeldKeys = new HashSet<Keys>();
        private HashSet<MouseButtons> _recordHeldButtons = new HashSet<MouseButtons>();

        private Thread _playThread;
        private volatile bool _playStopRequested = false;
        private volatile int _loopCount = 0;
        private volatile int _currentStep = 0;
        private volatile int _currentActionIndex = -1;
        private volatile int _finalStepCount = 0;
        // Counted down by the playback thread every time the final step is completed
        private int _stopAfterFinalSteps = 0;
        // The action ended by itself: "Stop after" was reached, or a condition ended it. Not set when stopped by the user
        private volatile bool _finishedByItself = false;
        private volatile string _lastCheckInfo = "";
        private volatile string _threadMessage = "";
        private bool _runningAction = false;

        // All events are raised on the UI thread
        public event Action StateChanged;
        public event Action MacroChanged;
        public event Action<MacroAction> ActionRecorded;
        // A mouse button went down / up anywhere on the screen. Used to pick a new position for a click
        public event Action<MouseButtons, bool> MouseInput;
        // Ctrl + Right was pressed. The capture window listens to this one
        public event Action CaptureRequested;

        // Set by the Automation window. Builds what the action preset must run, or returns null and an error.
        // Null without any error means there is no action to run, the macro preset plays instead
        public delegate List<AutomationStep> ActionPlanProviderDelegate(out string error);
        public ActionPlanProviderDelegate ActionPlanProvider;

        public bool IsRecording => _recording;
        public bool IsPlaying => _playThread != null;
        public bool IsToolboxStarted => _toolboxStarted;
        public bool IsRunningAction => IsPlaying && _runningAction;
        public int LoopCount => _loopCount;
        // How many times the final step of the running action was completed
        public int FinalStepCount => _finalStepCount;
        // Put the computer to sleep when the action ends by itself (StopAfterFinalSteps, or a condition that ends the action)
        public bool SleepWhenDone { get; set; } = false;
        // The action stops by itself once its final step was completed this many more times. 0 to never stop.
        // It goes down by one every time the final step is completed
        public int StopAfterFinalSteps
        {
            get { return Volatile.Read(ref _stopAfterFinalSteps); }
            set { Interlocked.Exchange(ref _stopAfterFinalSteps, Math.Max(0, value)); }
        }
        public int CurrentStep => _currentStep;
        // Index of the last action played in the macro of the current step, -1 if none yet
        public int CurrentActionIndex => _currentActionIndex;
        public string LastCheckInfo => _lastCheckInfo;
        public List<MacroAction> Actions => _actions;
        public int Duration => _duration;
        public string LastMessage { get; private set; } = "";

        // Used by the playback thread only, to pick one of the targets of a check
        private static readonly Random _random = new Random();

        [DllImport("winmm.dll")]
        private static extern uint timeBeginPeriod(uint uPeriod);
        [DllImport("winmm.dll")]
        private static extern uint timeEndPeriod(uint uPeriod);
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public AutomationModule(Main main, WindowsUtil windowsUtil, InputHook inputHook)
        {
            _main = main;
            _windowsUtil = windowsUtil;
            _inputHook = inputHook;
        }

        public void Start()
        {

        }

        public void Stop()
        {
            _toolboxStarted = false;
            StopAll();
        }

        public void MainLoop(int deltaTime, bool inFocus, bool started)
        {
            _toolboxStarted = started;
            _inFocus = inFocus;

            if (IsPlaying && !_playThread.IsAlive)
            {
                // The action reached its end by itself
                bool sleep = _finishedByItself && SleepWhenDone;
                StopPlayback(_threadMessage);
                if (sleep)
                {
                    // Sleep (not hibernate), don't force, and keep wake events enabled
                    Application.SetSuspendState(PowerState.Suspend, false, false);
                }
            }
            else if (IsPlaying && !inFocus)
            {
                StopPlayback("Stopped: POE is out of focus.");
            }
        }

        // Called by the Automation window when it opens / closes
        public void SetWindowOpen(bool open)
        {
            _windowOpen = open;
            if (!open)
            {
                StopAll();
            }
        }

        // Called by the Automation window: is there a macro preset to record into / play from
        public void SetMacroSelected(bool selected)
        {
            _macroSelected = selected;
            if (!selected)
            {
                StopAll();
            }
        }

        // Called by the Automation window: the macro preset is an "Item pickup" one, with those settings. Null if it's not.
        // The settings are read when the playback starts, the window can keep changing them
        public void SetPickupMacro(ItemPickupSettings settings)
        {
            _pickup = settings;
        }

        public void LoadMacro(List<MacroAction> actions, int duration)
        {
            StopAll();
            _actions = new List<MacroAction>();
            foreach (MacroAction action in actions)
            {
                _actions.Add(action.Clone());
            }
            _duration = duration;
            LastMessage = "";
            MacroChanged?.Invoke();
        }

        // Manually change when an action happens. Everything after it is shifted by the same amount,
        // so only the wait before this action changes. Returns false if nothing was changed.
        public bool SetActionTime(int index, int newTime)
        {
            if (_recording || IsPlaying || index < 0 || index >= _actions.Count) return false;

            // An action can't happen before the previous one
            int previousTime = index > 0 ? _actions[index - 1].Time : 0;
            newTime = Math.Max(previousTime, Math.Min(newTime, MAX_ACTION_TIME));
            // A first mouse down needs the time of the mouse move that goes before it
            if (index == 0 && _actions[0].Type == MacroActionType.MouseDown) newTime = Math.Max(newTime, MOUSE_MOVE_LEAD_TIME);
            int shift = newTime - _actions[index].Time;
            if (shift == 0) return false;

            for (int i = index; i < _actions.Count; i++)
            {
                _actions[i].Time += shift;
            }
            _duration += shift;
            MacroChanged?.Invoke();
            return true;
        }

        // Manually change when the macro ends, after its last action. Returns false if nothing was changed.
        public bool SetDuration(int newDuration)
        {
            if (_recording || IsPlaying) return false;

            int lastTime = _actions.Count > 0 ? _actions[_actions.Count - 1].Time : 0;
            newDuration = Math.Max(lastTime, Math.Min(newDuration, MAX_ACTION_TIME));
            if (newDuration == _duration) return false;

            _duration = newDuration;
            MacroChanged?.Invoke();
            return true;
        }

        public bool RemoveAction(int index)
        {
            if (_recording || IsPlaying || index < 0 || index >= _actions.Count) return false;

            // The other actions stay where they are in time
            _actions.RemoveAt(index);
            MacroChanged?.Invoke();
            return true;
        }

        // Change the key of a key press. The other half of the press (the "up" of a "down", or the "down" of an "up")
        // follows, or the old key would never be released
        public bool SetActionKey(int index, Keys key)
        {
            if (_recording || IsPlaying || index < 0 || index >= _actions.Count) return false;

            MacroAction action = _actions[index];
            if (action.IsMouse || action.Key == key) return false;

            bool isDown = action.Type == MacroActionType.KeyDown;
            for (int i = index + (isDown ? 1 : -1); i >= 0 && i < _actions.Count; i += isDown ? 1 : -1)
            {
                MacroAction other = _actions[i];
                if (other.IsMouse || other.Key != action.Key) continue;

                if (other.Type != action.Type) other.Key = key;
                break;
            }
            action.Key = key;
            MacroChanged?.Invoke();
            return true;
        }

        // Change where a mouse button goes down / up. The other half of the click follows if it is
        // right next to this action, at the same position
        public bool SetActionPosition(int index, double xPercent, double yPercent)
        {
            if (_recording || IsPlaying || index < 0 || index >= _actions.Count) return false;

            MacroAction action = _actions[index];
            if (!action.IsMouse) return false;

            int otherIndex = action.Type == MacroActionType.MouseDown ? index + 1 : index - 1;
            if (otherIndex >= 0 && otherIndex < _actions.Count)
            {
                MacroAction other = _actions[otherIndex];
                if (other.IsMouse && other.Type != action.Type && other.Button == action.Button
                    && other.XPercent == action.XPercent && other.YPercent == action.YPercent)
                {
                    other.XPercent = xPercent;
                    other.YPercent = yPercent;
                }
            }
            action.XPercent = xPercent;
            action.YPercent = yPercent;
            MacroChanged?.Invoke();
            return true;
        }

        public void StopAll()
        {
            if (_recording) StopRecording();
            if (IsPlaying) StopPlayback("");
        }



        public void OnKeyEvent(Keys key, bool isDown, bool isControlDown)
        {
            // Ignore what the toolbox (or this playback) is typing by itself
            if (_inputHook.LastEventInjected) return;

            bool isHotkey = key == RECORD_HOTKEY || key == PLAYBACK_HOTKEY || key == CAPTURE_HOTKEY;
            if (isHotkey && !isDown)
            {
                _hotkeyHeld = false;
            }
            if (isHotkey && isControlDown)
            {
                // Key repeat sends many "down", only react to the first one
                if (isDown && !_hotkeyHeld)
                {
                    _hotkeyHeld = true;
                    if (key == RECORD_HOTKEY) ToggleRecording();
                    else if (key == PLAYBACK_HOTKEY) TogglePlayback();
                    else CaptureRequested?.Invoke();
                }
                return;
            }

            if (!_recording) return;
            if ((int)key == 0 || (int)key >= 255) return;

            if (isDown)
            {
                if (!_recordHeldKeys.Add(key)) return; // Key repeat
                AddRecordedAction(new MacroAction { Type = MacroActionType.KeyDown, Key = key });
            }
            else
            {
                // A key that was already held when the recording started (usually the hotkey's Ctrl)
                if (!_recordHeldKeys.Remove(key)) return;
                AddRecordedAction(new MacroAction { Type = MacroActionType.KeyUp, Key = key });
            }
        }

        public void OnMouseEvent(MouseButtons button, bool isDown)
        {
            if (_inputHook.LastEventInjected) return;
            MouseInput?.Invoke(button, isDown);
            if (!_recording) return;

            if (isDown)
            {
                if (!_recordHeldButtons.Add(button)) return;
            }
            else
            {
                if (!_recordHeldButtons.Remove(button)) return;
            }

            _inputHook.GetCurrentMousePercentPosition(out double xPercent, out double yPercent);
            AddRecordedAction(new MacroAction
            {
                Type = isDown ? MacroActionType.MouseDown : MacroActionType.MouseUp,
                Button = button,
                XPercent = xPercent,
                YPercent = yPercent
            });
        }

        private void AddRecordedAction(MacroAction action)
        {
            action.Time = (int)_recordWatch.ElapsedMilliseconds;
            _actions.Add(action);
            ActionRecorded?.Invoke(action);
        }



        private bool CanRun()
        {
            // Only when the toolbox is ON, and the Automation window is opened with a preset selected
            return _windowOpen && _toolboxStarted;
        }

        private void ToggleRecording()
        {
            // An "Item pickup" macro has nothing to record
            if (!CanRun() || !_macroSelected || _pickup != null) return;
            if (IsPlaying)
            {
                SetMessage("Stop the playback before recording.");
                return;
            }

            if (_recording) StopRecording();
            else StartRecording();
        }

        private void TogglePlayback()
        {
            if (!CanRun()) return;
            if (_recording)
            {
                SetMessage("Stop the recording (Ctrl+Up) before playing.");
                return;
            }

            if (IsPlaying)
            {
                StopPlayback("Stopped.");
                return;
            }

            // The action preset goes first, the macro preset alone is the fallback
            string error = "";
            List<AutomationStep> steps = ActionPlanProvider?.Invoke(out error);
            if (steps != null) StartThread(steps, true);
            else if (error != "") SetMessage(error);
            else if (_macroSelected) StartPlayback();
        }

        private void SetMessage(string message)
        {
            LastMessage = message;
            StateChanged?.Invoke();
        }

        private void StartRecording()
        {
            _actions = new List<MacroAction>();
            _duration = 0;
            _recordHeldKeys.Clear();
            _recordHeldButtons.Clear();
            _recordWatch.Restart();
            _recording = true;
            LastMessage = "";
            MacroChanged?.Invoke();
            StateChanged?.Invoke();
        }

        private void StopRecording()
        {
            _recording = false;
            _recordWatch.Stop();
            _duration = (int)_recordWatch.ElapsedMilliseconds;

            // Drop the trailing presses that never got released, that's the Ctrl of the stop hotkey.
            // The loop then ends where that Ctrl was pressed.
            while (_actions.Count > 0)
            {
                MacroAction last = _actions[_actions.Count - 1];
                bool unreleased = (last.Type == MacroActionType.KeyDown && _recordHeldKeys.Remove(last.Key))
                               || (last.Type == MacroActionType.MouseDown && _recordHeldButtons.Remove(last.Button));
                if (!unreleased) break;
                if (last.Type == MacroActionType.KeyDown && last.Key == Keys.ControlKey)
                {
                    _duration = Math.Min(_duration, last.Time);
                }
                _actions.RemoveAt(_actions.Count - 1);
            }
            if (_actions.Count > 0)
            {
                _duration = Math.Max(_duration, _actions[_actions.Count - 1].Time);
            }

            // Anything else still held was used by the macro (e.g. Ctrl held for Ctrl+clicks),
            // release it at the end so every loop is balanced
            foreach (Keys key in _recordHeldKeys)
            {
                _actions.Add(new MacroAction { Type = MacroActionType.KeyUp, Key = key, Time = _duration });
            }
            foreach (MouseButtons button in _recordHeldButtons)
            {
                MacroAction down = _actions.FindLast(a => a.Type == MacroActionType.MouseDown && a.Button == button);
                _actions.Add(new MacroAction { Type = MacroActionType.MouseUp, Button = button, XPercent = down.XPercent, YPercent = down.YPercent, Time = _duration });
            }
            _recordHeldKeys.Clear();
            _recordHeldButtons.Clear();

            LastMessage = "Recorded " + _actions.Count + " actions.";
            MacroChanged?.Invoke();
            StateChanged?.Invoke();
        }

        private void StartPlayback()
        {
            if (_pickup != null)
            {
                if (_pickup.Labels.Count == 0)
                {
                    SetMessage("Nothing to pick up, add a label color first.");
                    return;
                }
                AutomationStep pickupStep = new AutomationStep { Name = "", Pickup = MacroPresetStore.ClonePickup(_pickup) };
                StartThread(new List<AutomationStep> { pickupStep }, false);
                return;
            }

            if (_actions.Count == 0)
            {
                SetMessage("Nothing to play, record something first (Ctrl+Up).");
                return;
            }

            // Playing a macro preset is running a single step that never jumps anywhere.
            // The thread works on its own copy, so the UI can do whatever it wants with the list
            AutomationStep step = new AutomationStep { Name = "", Duration = _duration };
            foreach (MacroAction action in _actions)
            {
                step.Actions.Add(action.Clone());
            }
            StartThread(new List<AutomationStep> { step }, false);
        }

        private void StartThread(List<AutomationStep> steps, bool isAction)
        {
            if (!_inFocus)
            {
                DisposeSteps(steps);
                SetMessage("POE is out of focus, not started.");
                return;
            }

            _playStopRequested = false;
            _loopCount = 1;
            _currentStep = 0;
            _currentActionIndex = -1;
            _finalStepCount = 0;
            _finishedByItself = false;
            _lastCheckInfo = "";
            _threadMessage = "";
            _runningAction = isAction;
            _playThread = new Thread(() => PlaybackThread(steps));
            _playThread.IsBackground = true;
            _playThread.Start();

            LastMessage = "";
            _windowsUtil.SetAutomationMode(true);
            StateChanged?.Invoke();
        }

        private void StopPlayback(string message)
        {
            _playStopRequested = true;
            _playThread?.Join(1000);
            _playThread = null;
            _runningAction = false;

            LastMessage = message;
            _windowsUtil.SetAutomationMode(false);
            StateChanged?.Invoke();
        }

        private static void DisposeSteps(List<AutomationStep> steps)
        {
            foreach (AutomationStep step in steps)
            {
                foreach (AutomationScreenCheck check in step.Checks)
                {
                    check.Matcher.Dispose();
                }
                step.Scanner?.Dispose();
                step.Scanner = null;
            }
        }

        private void PlaybackThread(List<AutomationStep> steps)
        {
            HashSet<Keys> heldKeys = new HashSet<Keys>();
            HashSet<MouseButtons> heldButtons = new HashSet<MouseButtons>();

            timeBeginPeriod(1);
            try
            {
                // Wait for the Ctrl of the start hotkey to be released, or the first clicks become Ctrl+clicks
                while ((GetAsyncKeyState((int)Keys.ControlKey) & 0x8000) != 0)
                {
                    if (_playStopRequested) return;
                    Thread.Sleep(1);
                }

                int stepIndex = 0;
                bool limitReached = false;
                while (stepIndex >= 0 && stepIndex < steps.Count && !_playStopRequested)
                {
                    bool isFinalStep = steps[stepIndex].IsFinal;
                    _currentActionIndex = -1;
                    _currentStep = stepIndex;
                    _loopCount = 1;
                    stepIndex = RunStep(steps[stepIndex], heldKeys, heldButtons);
                    // A jump can happen in the middle of a macro, start the next one clean
                    ReleaseHeld(heldKeys, heldButtons);

                    if (isFinalStep && !_playStopRequested)
                    {
                        _finalStepCount++;
                        // Read every time, the limit can be changed while the action is running
                        if (StopAfterFinalSteps > 0 && Interlocked.Decrement(ref _stopAfterFinalSteps) <= 0)
                        {
                            Interlocked.Exchange(ref _stopAfterFinalSteps, 0);
                            limitReached = true;
                            break;
                        }
                    }
                }
                _finishedByItself = limitReached || !_playStopRequested;
                if (limitReached) _threadMessage = "Action stopped after " + _finalStepCount + " times of its final step.";
                else if (!_playStopRequested) _threadMessage = "Action finished.";
            }
            catch (Exception ex)
            {
                _threadMessage = "Automation stopped by an error: " + ex.Message;
            }
            finally
            {
                // Don't leave anything stuck down when stopped in the middle of a loop
                ReleaseHeld(heldKeys, heldButtons);
                DisposeSteps(steps);
                timeEndPeriod(1);
            }
        }

        private void ReleaseHeld(HashSet<Keys> heldKeys, HashSet<MouseButtons> heldButtons)
        {
            foreach (Keys key in heldKeys) _inputHook.SendKey(key, false);
            System.Drawing.Point pos = _inputHook.GetCurrentMousePixelPosition();
            foreach (MouseButtons button in heldButtons) _inputHook.SendMouseButton(button, false, pos.X, pos.Y);
            heldKeys.Clear();
            heldButtons.Clear();
        }

        // Loop the macro of the step until one of its conditions jumps somewhere.
        // Returns the index of the next step, AutomationStep.TARGET_END to finish.
        private int RunStep(AutomationStep step, HashSet<Keys> heldKeys, HashSet<MouseButtons> heldButtons)
        {
            int duration = Math.Max(step.Duration, MIN_LOOP_DURATION);
            Stopwatch stepWatch = Stopwatch.StartNew();
            Stopwatch loopWatch = new Stopwatch();
            int loopsDone = 0;
            foreach (AutomationScreenCheck check in step.Checks)
            {
                check.NextCheckTime = check.Interval;
            }

            while (true)
            {
                loopWatch.Restart();
                if (step.Pickup != null)
                {
                    int pickupJump = RunPickupLoop(step, loopWatch, stepWatch);
                    if (pickupJump != AutomationStep.NO_JUMP) return pickupJump;
                }

                for (int i = 0; i < step.Actions.Count; i++)
                {
                    MacroAction action = step.Actions[i];
                    int jump;
                    if (action.IsMouse)
                    {
                        // Put the cursor in place ahead of time (right away if the previous action was less than that ago)
                        jump = WaitUntil(loopWatch, action.Time - MOUSE_MOVE_LEAD_TIME, step, stepWatch);
                        if (jump != AutomationStep.NO_JUMP) return jump;
                        System.Drawing.Point movePos = _inputHook.PercentToPixelPosition(action.XPercent, action.YPercent);
                        _inputHook.MoveMouseTo(movePos.X, movePos.Y);
                    }

                    jump = WaitUntil(loopWatch, action.Time, step, stepWatch);
                    if (jump != AutomationStep.NO_JUMP) return jump;
                    _currentActionIndex = i;

                    if (action.IsMouse)
                    {
                        bool isDown = action.Type == MacroActionType.MouseDown;
                        System.Drawing.Point clickPos = _inputHook.PercentToPixelPosition(action.XPercent, action.YPercent);
                        _inputHook.SendMouseButton(action.Button, isDown, clickPos.X, clickPos.Y);
                        if (isDown) heldButtons.Add(action.Button); else heldButtons.Remove(action.Button);
                    }
                    else
                    {
                        bool isDown = action.Type == MacroActionType.KeyDown;
                        _inputHook.SendKey(action.Key, isDown);
                        if (isDown) heldKeys.Add(action.Key); else heldKeys.Remove(action.Key);
                    }
                }

                int endJump = WaitUntil(loopWatch, step.Pickup != null ? 0 : duration, step, stepWatch);
                if (endJump != AutomationStep.NO_JUMP) return endJump;

                // The macro is never cut in the middle by those, they wait for the end of the loop
                loopsDone++;
                foreach (AutomationEndCheck check in step.EndChecks)
                {
                    if (loopsDone >= check.Loops && stepWatch.ElapsedMilliseconds >= check.Milliseconds) return PickTarget(check.Targets);
                }
                _loopCount++;
            }
        }

        // One loop of an "Item pickup" macro: click the labels one by one, until none is found a few times in a row.
        // Returns AutomationStep.NO_JUMP when the loop ended, otherwise where to jump.
        private int RunPickupLoop(AutomationStep step, Stopwatch loopWatch, Stopwatch stepWatch)
        {
            ItemPickupSettings settings = step.Pickup;
            step.Scanner ??= new ItemLabelScanner(settings);

            int emptyScans = 0;
            int clicks = 0;
            while (emptyScans < Math.Max(1, settings.EmptyScansToEnd) && (settings.MaxClicks <= 0 || clicks < settings.MaxClicks))
            {
                System.Drawing.Point neutralPos = _inputHook.PercentToPixelPosition(PICKUP_NEUTRAL_X_PERCENT, PICKUP_NEUTRAL_Y_PERCENT);
                _inputHook.MoveMouseTo(neutralPos.X, neutralPos.Y);
                int jump = WaitFor(PICKUP_SETTLE_TIME, loopWatch, step, stepWatch);
                if (jump != AutomationStep.NO_JUMP) return jump;

                // The labels move with the character, so only one is clicked per capture
                bool found = step.Scanner.FindNearestLabel(out System.Drawing.Point labelPos);
                _lastCheckInfo = "Last item scan: " + step.Scanner.LastFoundCount + " label rows found, " + clicks + " clicks so far";
                if (!found)
                {
                    emptyScans++;
                    jump = WaitFor(settings.ScanInterval, loopWatch, step, stepWatch);
                    if (jump != AutomationStep.NO_JUMP) return jump;
                    continue;
                }

                emptyScans = 0;
                clicks++;
                _inputHook.MoveMouseTo(labelPos.X, labelPos.Y);
                jump = WaitFor(MOUSE_MOVE_LEAD_TIME, loopWatch, step, stepWatch);
                if (jump != AutomationStep.NO_JUMP) return jump;

                _inputHook.SendMouseButton(MouseButtons.Left, true, labelPos.X, labelPos.Y);
                Thread.Sleep(PICKUP_CLICK_HOLD_TIME);
                _inputHook.SendMouseButton(MouseButtons.Left, false, labelPos.X, labelPos.Y);

                jump = WaitFor(settings.DelayAfterClick, loopWatch, step, stepWatch);
                if (jump != AutomationStep.NO_JUMP) return jump;
            }
            return AutomationStep.NO_JUMP;
        }

        // A check with several targets (macros sharing a name) goes to one of them at random
        private static int PickTarget(int[] targets)
        {
            if (targets == null || targets.Length == 0) return AutomationStep.TARGET_END;
            return targets.Length == 1 ? targets[0] : targets[_random.Next(targets.Length)];
        }

        // Same as WaitUntil, for a number of milliseconds from now
        private int WaitFor(int milliseconds, Stopwatch loopWatch, AutomationStep step, Stopwatch stepWatch)
        {
            return WaitUntil(loopWatch, (int)Math.Min(int.MaxValue, loopWatch.ElapsedMilliseconds + Math.Max(0, milliseconds)), step, stepWatch);
        }

        // Wait until the loop reaches the given time, checking the screen conditions meanwhile.
        // Returns AutomationStep.NO_JUMP when the time is reached, otherwise where to jump.
        private int WaitUntil(Stopwatch loopWatch, int time, AutomationStep step, Stopwatch stepWatch)
        {
            while (true)
            {
                if (_playStopRequested) return AutomationStep.TARGET_END;

                foreach (AutomationScreenCheck check in step.Checks)
                {
                    if (stepWatch.ElapsedMilliseconds < check.NextCheckTime) continue;

                    double percent = check.Matcher.GetMatchPercent(check.Tolerance);
                    check.NextCheckTime = stepWatch.ElapsedMilliseconds + check.Interval;
                    _lastCheckInfo = "Last screen check: " + percent.ToString("0.0") + "% (needs " + check.MatchPercent + "%)";
                    if (percent >= check.MatchPercent) return PickTarget(check.Targets);
                }

                if (loopWatch.ElapsedMilliseconds >= time) return AutomationStep.NO_JUMP;
                Thread.Sleep(1);
            }
        }
    }

    // What the playback thread runs: a macro, and where to go when its conditions are met
    public class AutomationStep
    {
        public const int TARGET_END = -1;
        public const int NO_JUMP = -2;

        public string Name = "";
        // Counts for StopAfterFinalSteps every time it is completed
        public bool IsFinal = false;
        public List<MacroAction> Actions = new List<MacroAction>();
        public int Duration = 0;
        // Set for an "Item pickup" macro, which has no action. The scanner is made by the playback thread
        public ItemPickupSettings Pickup;
        public ItemLabelScanner Scanner;
        // Checked in this order every time the macro played until its end. It loops again if none is met
        public List<AutomationEndCheck> EndChecks = new List<AutomationEndCheck>();
        public List<AutomationScreenCheck> Checks = new List<AutomationScreenCheck>();
    }

    // Met when the macro has looped at least that many times, and the step has been running for at least that long.
    // Targets are step indexes (or AutomationStep.TARGET_END), one of them is picked at random when there are several
    public class AutomationEndCheck
    {
        public int Loops;
        public long Milliseconds;
        public int[] Targets;
    }

    public class AutomationScreenCheck
    {
        public ScreenMatcher Matcher;
        public int Interval;
        public int Tolerance;
        public int MatchPercent;
        public int[] Targets;
        public long NextCheckTime;
    }
}
