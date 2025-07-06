using System;
using System.Windows.Forms;

using POE1Tools.Utilities;

namespace POE1Tools
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            var windowUtil = new WindowsUtil();
            var inputHook = new InputHook();
            var colorUtil = new ColorUtil();

            var mainForm = new Main(windowUtil, inputHook, colorUtil);

            Application.Run(mainForm);
        }
    }
}
