using System.Runtime.InteropServices;

namespace System.Helpers
{
    public class ConsoleWindow
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("User32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SW_SHOWNA = 8;

        private static volatile IntPtr _consoleWindowHandle;

        private static void StoreConsoleWindowHandle() { _consoleWindowHandle = GetConsoleWindow(); }
        static ConsoleWindow() { StoreConsoleWindowHandle(); }

        public static IntPtr Handle { get { return _consoleWindowHandle; } }
        public static void Open(string title = null)
        {
            if (IntPtr.Zero == _consoleWindowHandle)
            {
                AllocConsole();
                StoreConsoleWindowHandle();
            }
            else if (!IsWindowVisible(_consoleWindowHandle)) Show();
            if (null != title) Console.Title = title;
        }
        public static void Show(bool activeConsole = true)
        {
            ShowWindow(_consoleWindowHandle, activeConsole ? SW_SHOW : SW_SHOWNA);
        }
        public static void Hide(bool clearConsole = false)
        {
            if (IntPtr.Zero == _consoleWindowHandle) return;
            ShowWindow(_consoleWindowHandle, SW_HIDE);
            if (clearConsole) Console.Clear();
        }
        public static void Close()
        {
            Hide(true);
        }
    }
}
