using System;
using System.Runtime.InteropServices;

namespace CustomProcessMonitor
{
    public class WinAPI
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllFilePath);
    }
}
