using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace InternetRadio
{
    internal static class WindowIconHelper
    {
        private const int WM_SETICON = 0x80;
        private const int ICON_BIG = 1;
        private const int ICON_SMALL = 0;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadImage(IntPtr hInstance, string lpszName, uint uType,
            int cxDesired, int cyDesired, uint fuLoad);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

        private const uint IMAGE_ICON = 1;
        private const uint LR_LOADFROMFILE = 0x00000010;

        public static void SetWindowIcon(Window window, string iconPath)
        {
            var hwnd = WindowNative.GetWindowHandle(window);
            var hIcon = LoadImage(IntPtr.Zero, iconPath, IMAGE_ICON, 0, 0, LR_LOADFROMFILE);

            if (hIcon != IntPtr.Zero)
            {
                SendMessage(hwnd, WM_SETICON, ICON_BIG, hIcon);
                SendMessage(hwnd, WM_SETICON, ICON_SMALL, hIcon);
            }
        }
    }
}
