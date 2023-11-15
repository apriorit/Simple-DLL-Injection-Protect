# About projects
1. **CustomProcessMonitor** - The C# project monitors the system's opened and newly opened processes. 
Prints information about processes to the console: PID, Process Name, CPU Usage, Memory Usage, and Start Time.
You need to press Q or q to exit. The project is used to inject into it and change its behavior.

2. **InjectDLLProtectionPlugin** - The C++ DLL that protects the target application from DLL injection. 
The project uses Microsoft Detours library to inject into the target application and set the hook.

3. **ProcessHide_Deviare** and **ProcessHidePlugin_Deviare**- Projects use Nektra Deviare library to inject into the running application and hide the process named "DangerousMiner".
This is achieved by injecting DLL written in C++ and setting the hook on the OpenProcess WinAPI function call. 
Need to provide the PID of an application into what needs to inject DLL. After that, all the OpenProcess function calls will be intercepted.

4. **ProcessHide_EasyHook** and **ProcessHidePlugin_EasyHook** - Projects written in C# that use EasyHook library to inject into the running application and hide the process named "DangerousMiner".
This is achieved by injecting DLL and setting the hook on the OpenProcess WinAPI function call. 
Need to provide PID or full path to an executable file of an application into what needs to inject DLL. After that, all the OpenProcess function calls will be intercepted.

5. **DangerousMiner** - The WPF project that creates the process is named "DangerousMiner". The application simulates time-consuming operations. 
This application is used to demonstrate how to work ProcessHide_Deviare and ProcessHide_EasyHook projects. 
Without injecting DLL, the "DangerousMiner" process displays in the CustomProcessMonitor project, but after using *ProcessHide_Deviare* or *ProcessHide_EasyHook* applications - the process will not be shown when the user starts it.

6. **3rd-party** - The source code and licenses of libraries that were used for projects.


# How to build the solution
> [!NOTE]
> - Visual Studio 2017 or later must be installed.
> - C++ ATL for v141 build tools (x86 and x64) must be installed in Visual Studio.

1. Clone this repository.
2. Open a solution file via Visual Studio.
3. Specify solution configuration to *DEBUG*.
4. Specify solution platform to *x86*.
5. Perform Build Solution.

> [!IMPORTANT]
> The project is configured correctly and works for DEBUG x86 ONLY!


# How to use the solution
After successfully building the solution, a Debug folder will be created in the Solution directory. The folder contains all built executable and DLL files.

## How to inject DLL into **CustomProcessMonitor** and hide DangerousMiner process: 
- To use Deviare library, the user must register the respective COM object in the system. So, open CMD as administrator and navigate to Debug folder, which contains all built executable and DLL files, and run the command:
```
Regsvr32 DeviareLiteCOM.dll
```
- Run **CustomProcessMonitor.exe** to open process monitor application.
- Run **ProcessHide_Deviare.exe** or **ProcessHide_EasyHook.exe**, then provide a PID of **CustomProcessMonitor**. After that, the **ProcessHidePlugin_Deviare.dll** or **ProcessHidePlugin_EasyHook.dll** will be injected into the target application.
- Open a **DangerousMiner** to test that the process will not be shown in **CustomProcessMonitor** application.

## How to protect **CustomProcessMonitor** from DLL injection:
- Navigate to the Debug folder and open a configuration file for **CustomProcessMonitor.exe**: "CustomProcessMonitor.exe.config".
- Change the "ApplyProtection" value to "true" and save changes.
- Run **CustomProcessMonitor.exe** to open process monitor application with the protection. *InjectDLLProtectionPlugin.dll will be injected into an application to protect it.*
- Try to inject some DLLs into **CustomProcessMonitor.exe** using **ProcessHide_Deviare.exe** or **ProcessHide_EasyHook.exe**.
- As a result, the application must notify you that something is trying to inject DLL and reject its request.


# License
[Apriorit](http://www.apriorit.com/) released under the OSI-approved 3-clause BSD license. You can freely use it in your commercial or opensource software.

