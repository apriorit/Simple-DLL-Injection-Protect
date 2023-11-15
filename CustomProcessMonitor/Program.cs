using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace CustomProcessMonitor
{
    internal class ProcessMonitor
    {
        static async Task Main()
        {
            Console.WriteLine("Process Monitor has started.");
            Console.WriteLine("Press 'Q' or 'q' to quit.");

            // Applying protection, in case, when user marked it in configuration file.
            string applyProtectionSetting = ConfigurationManager.AppSettings["ApplyProtection"];
            if (!string.IsNullOrEmpty(applyProtectionSetting) && 
                applyProtectionSetting.ToLower() == "true")
            {
                Console.WriteLine("Applying DLL Inject Protection to this application.");

                // Load the DLL, that will protect from DLL injection, into the application.
                const string protectDLLInjection = "InjectDLLProtectionPlugin.dll";
                WinAPI.LoadLibrary(protectDLLInjection);
            }
            
            var cancellationTokenSource = new CancellationTokenSource();
            Task processMonitoringTask = MonitorProcessesAsync(cancellationTokenSource.Token);

            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                {
                    cancellationTokenSource.Cancel();
                    break;
                }
            }

            // Wait for the monitoring task to complete
            await processMonitoringTask;
        }

        static async Task MonitorProcessesAsync(CancellationToken _cancellationToken)
        {
            // Geting the initial list of processes and printing it.
            List<Process> initialProcesses = GetRunningProcesses();
            if (initialProcesses.Count == 0)
            {
                return;
            }
            PrintNewProcessList(initialProcesses);

            while (!_cancellationToken.IsCancellationRequested)
            {
                // Wait for 3 seconds before checking for new processes
                await Task.Delay(3000);

                List<Process> currentProcesses = GetRunningProcesses();
                if (currentProcesses.Count == 0)
                {
                    return;
                }

                // Print the list of new processes.
                List<Process> newProcesses = FindNewProcesses(initialProcesses, currentProcesses);
                if (newProcesses.Count > 0)
                {
                    PrintNewProcessList(newProcesses);
                }

                // Print the list of terminated processes.
                List<Process> terminatedProcesses = FindTerminatedProcesses(initialProcesses, currentProcesses);
                if (terminatedProcesses.Count > 0)
                {
                    PrintTerminatedProcessList(terminatedProcesses);
                }
                
                initialProcesses = currentProcesses;
            }
        }

        static List<Process> GetRunningProcesses()
        {
            List<Process> processes = new List<Process>();

            try
            {
                foreach (Process process in Process.GetProcesses())
                {
                    try
                    {
                        // No need to add the Idle process to the process list.
                        if (process.ProcessName == "Idle")
                        {
                            continue;
                        }

                        // Check if the process has exited (this may throw an "Access is denied" exception).
                        // If it doesn't throw an exception and is not exited - add the process to the list.
                        if (!process.HasExited)
                        {
                            processes.Add(process);
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving process list: {ex.Message}");
            }

            return processes;
        }

        // Custom comparer to compare processes by ID.
        class ProcessIDComparer : IEqualityComparer<Process>
        {
            public bool Equals(Process process1, Process process2)
            {
                return process1.Id == process2.Id;
            }

            public int GetHashCode(Process process)
            {
                return process.Id;
            }
        }

        static List<Process> FindNewProcesses(List<Process> _oldProcesses, List<Process> _currentProcesses)
        {
            return _currentProcesses.Except(_oldProcesses, new ProcessIDComparer()).ToList();
        }

        static List<Process> FindTerminatedProcesses(List<Process> _oldProcesses, List<Process> _currentProcesses)
        {
            return _oldProcesses.Except(_currentProcesses, new ProcessIDComparer()).ToList();
        }

        static void PrintNewProcessList(List<Process> _processes)
        {
            Console.WriteLine("| {0,-5} | {1,-40} | {2,16} | {3,18} | {4,21} | ", "PID", "Process Name", "CPU Usage", "Memory Usage", "Start Time");
            foreach (Process process in _processes)
            {
                process.Refresh();
                try
                {
                    Console.WriteLine($"| {process.Id,-5} | {process.ProcessName,-40} | {process.TotalProcessorTime,16} | {process.WorkingSet64 / 1024, 15} KB | {process.StartTime,21} |");
                }
                catch
                {
                }
            }
        }

        static void PrintTerminatedProcessList(List<Process> _processes)
        {
            Console.WriteLine("| {0,-5} | {1,-40} | {2,16} | {3,18} | {4,21} | ", "PID", "Terminated Process Name", "CPU Usage", "Memory Usage", "Start Time");
            foreach (Process process in _processes)
            {
                try
                {
                    Console.WriteLine($"| {process.Id,-5} | {process.ProcessName,-40} | {"-----",16} | {"-----",15} KB | {"-----",21} |");
                }
                catch
                {
                }
            }
        }
    }
}

