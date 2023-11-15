using System;
using System.IO;

namespace ProcessHide_Deviare
{
    class Program
    {
        static DeviareLiteInterop.HookLib cHook;

        static Program()
        {
            cHook = new DeviareLiteInterop.HookLib();
        }

        static void Main(string[] args)
        {
            int TargetPID = 0;

            while ((args.Length != 1) || !int.TryParse(args[0], out TargetPID))
            {
                if (TargetPID > 0)
                {
                    break;
                }

                if (args.Length != 1)
                {
                    Console.WriteLine("Usage: ProcessHide %PID%");
                    Console.WriteLine();
                    Console.Write("Please enter a process Id of the executable to inject: ");

                    // Get input from the user.
                    args = new string[] { Console.ReadLine() };
                    if (string.IsNullOrEmpty(args[0]))
                    {
                        return;
                    }
                }
            }

            try
            {
                // Constructing file path to DLL that will be injected into running target process.
                string injectionLibrary = Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "ProcessHidePlugin_Deviare.dll"
                );

                // Opening target process.
                IntPtr result = WinAPI.OpenProcess(WinAPI.ProcessAccessFlags.AllAccess, false, TargetPID);
                if (result == IntPtr.Zero)
                {
                    Console.WriteLine("Error: Unable to open process with PID: {0}", TargetPID);
                    Console.WriteLine("<Press any key to exit>");
                    Console.ReadKey();
                    return;
                }

                // Injecting DLL into the opened process.
                cHook.InjectDll(TargetPID, injectionLibrary, "", 5000);
                Console.WriteLine("Injected into the target process {0}", TargetPID);

                WinAPI.WaitForSingleObject(result, 0xFFFFFFFF);
            }
            catch (Exception ExtInfo)
            {
                Console.WriteLine("Couldn't complete operation.\r\rError:\n{0}", ExtInfo.ToString());
                Console.WriteLine("<Press any key to exit>");
                Console.ReadKey();
            }
        }
    }
}
