using System;
using System.IO;
using System.Runtime.Remoting;

using EasyHook;

namespace ProcessHide_EasyHook
{
    public class ProcessHideInterface : MarshalByRefObject
    {
        public void IsInstalled(Int32 InClientPID)
        {
            Console.WriteLine("ProcessHide has been installed in target {0}.\r\n", InClientPID);
        }

        public void ReportException(Exception InInfo)
        {
            Console.WriteLine("The target process has reported an error:\r\n" + InInfo.ToString());
        }

        public void Ping()
        {
        }
    }

    internal class Program
    {
        private static string ChannelName = null;

        static void Main(string[] args)
        {
            int TargetPID = 0;
            string targetExe = null;

            while ((args.Length != 1) || !int.TryParse(args[0], out TargetPID) || !File.Exists(args[0]))
            {
                if (TargetPID > 0)
                {
                    break;
                }

                if (args.Length != 1 || !File.Exists(args[0]))
                {
                    Console.WriteLine("Usage: ProcessHide %PID%");
                    Console.WriteLine("   or: ProcessHide PathToExecutable");
                    Console.WriteLine();
                    Console.Write("Please enter a process Id or path to executable: ");

                    // Get input from the user.
                    args = new string[] { Console.ReadLine() };
                    if (string.IsNullOrEmpty(args[0]))
                    {
                        return;
                    }
                }
                else
                {
                    targetExe = args[0];
                    break;
                }
            }

            try
            {
                RemoteHooking.IpcCreateServer<ProcessHideInterface>(ref ChannelName, WellKnownObjectMode.SingleCall);

                // Constructing file path to DLL that will be injected into running target process.
                string injectionLibrary = Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), 
                    "ProcessHidePlugin_EasyHook.dll");

                if (String.IsNullOrEmpty(targetExe))
                {
                    // Injecting DLL into the target process.
                    RemoteHooking.Inject(
                        TargetPID,
                        injectionLibrary,
                        injectionLibrary,
                        ChannelName);

                    Console.WriteLine("Injected to process {0}", TargetPID);
                }
                else
                {
                    // If the target process is not running - creating target process and injecting DLL.
                    RemoteHooking.CreateAndInject(targetExe, "", 0, InjectionOptions.DoNotRequireStrongName, injectionLibrary, injectionLibrary, out TargetPID, ChannelName);
                    Console.WriteLine("Created and injected process {0}", TargetPID);
                }
                Console.WriteLine("<Press any key to exit>");
                Console.ReadKey();
            }
            catch (Exception ExtInfo)
            {
                Console.WriteLine("There was an error while connecting to target:\r\n{0}", ExtInfo.ToString());
                Console.WriteLine("<Press any key to exit>");
                Console.ReadKey();
            }
        }
    }
}
