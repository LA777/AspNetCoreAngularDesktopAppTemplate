using System.Runtime.InteropServices;

namespace WindowsConsole
{
    public static class ConsoleHelper
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SwHide = 0;
        const int SwShow = 5;

        public static bool HideConsole()
        {
            var handle = GetConsoleWindow();
            return ShowWindow(handle, SwHide);
        }

        public static bool ShowConsole()
        {
            var handle = GetConsoleWindow();
            return ShowWindow(handle, SwShow);
        }
    }
}
