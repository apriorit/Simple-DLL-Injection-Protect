using System;
using System.Runtime.InteropServices;

namespace ProcessHidePlugin_EasyHook
{
    public class WinAPI
    {
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            AllAccess = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
            ProcessAccessFlags processAccess,
            bool bInheritHandle,
            int processId);

        [UnmanagedFunctionPointer(CallingConvention.StdCall,
           CharSet = CharSet.Unicode,
           SetLastError = true)]
        public delegate IntPtr DOpenProcess(
            ProcessAccessFlags processAccess,
            bool bInheritHandle,
            int processId);

    }
}
