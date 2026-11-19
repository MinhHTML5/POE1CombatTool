using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace POE1Tools.Utilities
{
    public class InputHook
    {
        private const int RIDEV_INPUTSINK = 0x00000100;
        private const int RID_INPUT = 0x10000003;

        private Action<MouseButtons, bool> _onMouseEvent;
        private Action<Keys, bool, bool> _onKeyEvent;
        private bool _controlPressing = false;

        // True while the event being dispatched to the callbacks was injected by software
        public bool LastEventInjected { get; private set; }

        [DllImport("User32.dll")]
        static extern uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        [StructLayout(LayoutKind.Sequential)]
        struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RAWKEYBOARD
        {
            public ushort MakeCode;
            public ushort Flags;
            public ushort Reserved;
            public ushort VKey;
            public uint Message;
            public uint ExtraInformation;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct RAWMOUSE
        {
            [FieldOffset(0)] public ushort usFlags;
            [FieldOffset(4)] public uint ulButtons;
            [FieldOffset(4)] public ushort usButtonFlags;
            [FieldOffset(6)] public ushort usButtonData;
            [FieldOffset(8)] public uint ulRawButtons;
            [FieldOffset(12)] public int lLastX;
            [FieldOffset(16)] public int lLastY;
            [FieldOffset(20)] public uint ulExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RAWINPUT
        {
            public RAWINPUTHEADER header;
            public RAWMOUSE mouse;
        }







        [DllImport("User32.dll")]
        static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

        public void RegisterRawInputDevices(IntPtr hwnd, Action<MouseButtons, bool> onMouseEvent, Action<Keys, bool, bool> onKeyEvent)
        {
            RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[2];

            rid[0].usUsagePage = 0x01;    // Generic desktop controls
            rid[0].usUsage = 0x02;        // Mouse
            rid[0].dwFlags = RIDEV_INPUTSINK;
            rid[0].hwndTarget = hwnd;

            rid[1].usUsagePage = 0x01;    // Generic desktop controls
            rid[1].usUsage = 0x06;        // Keyboard
            rid[1].dwFlags = RIDEV_INPUTSINK;
            rid[1].hwndTarget = hwnd;

            _onMouseEvent = onMouseEvent;
            _onKeyEvent = onKeyEvent;

            if (!RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
            {
                MessageBox.Show("Failed to register raw input devices.");
            }
        }


        public void ProcessRawInput(IntPtr hRawInput)
        {
            uint dwSize = 0;
            GetRawInputData(hRawInput, RID_INPUT, IntPtr.Zero, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));

            IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);
            try
            {
                if (GetRawInputData(hRawInput, RID_INPUT, buffer, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER))) != dwSize)
                    return;

                RAWINPUT raw = Marshal.PtrToStructure<RAWINPUT>(buffer);

                // Injected input (SendInput / keybd_event, including our own) has no source device
                LastEventInjected = raw.header.hDevice == IntPtr.Zero;

                if (raw.header.dwType == 0) // Mouse
                {
                    ushort buttonFlags = raw.mouse.usButtonFlags;
                    if ((buttonFlags & 0x0001) != 0) _onMouseEvent(MouseButtons.Left, true);
                    if ((buttonFlags & 0x0002) != 0) _onMouseEvent(MouseButtons.Left, false);
                    if ((buttonFlags & 0x0004) != 0) _onMouseEvent(MouseButtons.Right, true);
                    if ((buttonFlags & 0x0008) != 0) _onMouseEvent(MouseButtons.Right, false);
                    if ((buttonFlags & 0x0010) != 0) _onMouseEvent(MouseButtons.Middle, true);
                    if ((buttonFlags & 0x0020) != 0) _onMouseEvent(MouseButtons.Middle, false);
                    if ((buttonFlags & 0x0040) != 0) _onMouseEvent(MouseButtons.XButton1, true);
                    if ((buttonFlags & 0x0080) != 0) _onMouseEvent(MouseButtons.XButton1, false);
                    if ((buttonFlags & 0x0100) != 0) _onMouseEvent(MouseButtons.XButton2, true);
                    if ((buttonFlags & 0x0200) != 0) _onMouseEvent(MouseButtons.XButton2, false);
                }
                else if (raw.header.dwType == 1) // Keyboard
                {
                    IntPtr keyboardPtr = IntPtr.Add(buffer, Marshal.SizeOf<RAWINPUTHEADER>());
                    RAWKEYBOARD keyboard = Marshal.PtrToStructure<RAWKEYBOARD>(keyboardPtr);

                    ushort vk = keyboard.VKey;
                    bool isDown = (keyboard.Flags & 0x01) == 0;
                    bool isUp = !isDown;
                    // Ask Windows for the real Ctrl state instead of tracking key events, so a missed
                    // Ctrl key-up (e.g. alt-tab while holding it) can't leave Ctrl stuck "down"
                    _controlPressing = IsControlDown();
                    _onKeyEvent((Keys)vk, isDown, _controlPressing);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }



        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public static bool IsControlDown()
        {
            return (GetAsyncKeyState((int)Keys.ControlKey) & 0x8000) != 0;
        }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        private const int KEYEVENTF_KEYDOWN = 0x0000;
        private const int KEYEVENTF_KEYUP = 0x0002;

        public void PressKey(Keys key, bool control = false)
        {
            _controlPressing = IsControlDown();
            if (control && !_controlPressing)
            {
                keybd_event((byte)Keys.ControlKey, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            }
            keybd_event((byte)key, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            keybd_event((byte)key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            if (control && !_controlPressing)
            {
                keybd_event((byte)Keys.ControlKey, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
        }





        public void SendKeyDown(Keys key)
        {
            keybd_event((byte)key, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        }

        public void SendKeyUp(Keys key)
        {
            keybd_event((byte)key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }





        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        const int INPUT_MOUSE = 0;
        const uint MOUSEEVENTF_MOVE = 0x0001;
        const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        static extern bool ShowCursor(bool bShow);

        public Point GetCurrentMousePosition()
        {
            GetCursorPos(out POINT originalPos);
            Point result = new Point(originalPos.X, originalPos.Y);
            return result;
        }
        public void SendLeftClick()
        {
            INPUT[] inputs = new INPUT[2];

            // Click down
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mi.dwFlags = MOUSEEVENTF_LEFTDOWN;

            // Click up
            inputs[1].type = INPUT_MOUSE;
            inputs[1].mi.dwFlags = MOUSEEVENTF_LEFTUP;

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public void MoveMouse(int x, int y)
        {
            int screenWidth = GetSystemMetrics(0);
            int screenHeight = GetSystemMetrics(1);

            int absX = x * 65535 / screenWidth;
            int absY = y * 65535 / screenHeight;

            INPUT[] inputs = new INPUT[1];

            // Move to target (invisibly if you want)
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mi.dx = absX;
            inputs[0].mi.dy = absY;
            inputs[0].mi.dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE;

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        const uint MOUSEEVENTF_XDOWN = 0x0080;
        const uint MOUSEEVENTF_XUP = 0x0100;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        // A pixel to percent of the screen size (0 - 100). It points at the center of the pixel,
        // so converting back on the same resolution always gives the same pixel
        public static double PixelToPercent(int pixel, int screenSize)
        {
            return Math.Round((pixel + 0.5) * 100.0 / screenSize, 4);
        }

        public static int PercentToPixel(double percent, int screenSize)
        {
            int pixel = (int)Math.Floor(percent * screenSize / 100.0);
            return Math.Max(0, Math.Min(screenSize - 1, pixel));
        }

        // Percent of the main monitor size, to the pixel it is on the current resolution
        public Point PercentToPixelPosition(double xPercent, double yPercent)
        {
            return new Point(PercentToPixel(xPercent, GetSystemMetrics(0)), PercentToPixel(yPercent, GetSystemMetrics(1)));
        }

        // Where the cursor is, in percent of the main monitor size
        public void GetCurrentMousePercentPosition(out double xPercent, out double yPercent)
        {
            GetCursorPos(out POINT pos);
            xPercent = PixelToPercent(pos.X, GetSystemMetrics(0));
            yPercent = PixelToPercent(pos.Y, GetSystemMetrics(1));
        }

        public Point GetCurrentMousePixelPosition()
        {
            GetCursorPos(out POINT pos);
            return new Point(pos.X, pos.Y);
        }

        // Move the cursor to the exact pixel (x, y) of the main monitor
        public void MoveMouseTo(int x, int y)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0] = CreateMoveInput(x, y);
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private INPUT CreateMoveInput(int x, int y)
        {
            int screenWidth = GetSystemMetrics(0);
            int screenHeight = GetSystemMetrics(1);

            // Aim at the pixel center so the absolute coordinate maps back to the exact pixel
            INPUT input = new INPUT();
            input.type = INPUT_MOUSE;
            input.mi.dx = (int)(((long)x * 65536 + 32768) / screenWidth);
            input.mi.dy = (int)(((long)y * 65536 + 32768) / screenHeight);
            input.mi.dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE;
            return input;
        }

        // Move the cursor to (x, y) then press or release the given button there
        public void SendMouseButton(MouseButtons button, bool isDown, int x, int y)
        {
            uint flags;
            uint mouseData = 0;
            switch (button)
            {
                case MouseButtons.Left: flags = isDown ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP; break;
                case MouseButtons.Right: flags = isDown ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP; break;
                case MouseButtons.Middle: flags = isDown ? MOUSEEVENTF_MIDDLEDOWN : MOUSEEVENTF_MIDDLEUP; break;
                case MouseButtons.XButton1: flags = isDown ? MOUSEEVENTF_XDOWN : MOUSEEVENTF_XUP; mouseData = 1; break;
                case MouseButtons.XButton2: flags = isDown ? MOUSEEVENTF_XDOWN : MOUSEEVENTF_XUP; mouseData = 2; break;
                default: return;
            }

            INPUT[] inputs = new INPUT[2];
            inputs[0] = CreateMoveInput(x, y);

            inputs[1].type = INPUT_MOUSE;
            inputs[1].mi.mouseData = mouseData;
            inputs[1].mi.dwFlags = flags;

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        // Like SendKeyDown / SendKeyUp, but flags extended keys (arrows, Insert, Delete...) properly
        public void SendKey(Keys key, bool isDown)
        {
            uint flags = isDown ? (uint)KEYEVENTF_KEYDOWN : (uint)KEYEVENTF_KEYUP;
            switch (key)
            {
                case Keys.Up: case Keys.Down: case Keys.Left: case Keys.Right:
                case Keys.Insert: case Keys.Delete: case Keys.Home: case Keys.End:
                case Keys.PageUp: case Keys.PageDown:
                case Keys.LWin: case Keys.RWin: case Keys.Apps:
                case Keys.Divide: case Keys.NumLock:
                    flags |= KEYEVENTF_EXTENDEDKEY;
                    break;
            }
            keybd_event((byte)key, 0, flags, UIntPtr.Zero);
        }

        public void ShowMouseCursor (bool show)
        {
            ShowCursor(show);
        }
    }
}
