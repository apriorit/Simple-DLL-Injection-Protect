#include "pch.h"
#include "HookMngr.h"

typedef HMODULE(WINAPI* _HookLoadLibraryExW)(
    IN LPCWSTR lpLibFileName,
    HANDLE hFile,
    IN DWORD dwFlags
    );

_HookLoadLibraryExW TrueLoadLibraryExW = reinterpret_cast<_HookLoadLibraryExW>(
    GetProcAddress(GetModuleHandleW(L"kernelbase"), "LoadLibraryExW")
    );

HMODULE __stdcall HookLoadLibraryExW(IN LPCWSTR lpLibFileName, HANDLE hFile, IN DWORD dwFlags)
{
    const std::wstring libName(lpLibFileName);

    // Checking: is a dynamic-link library in the allowed list, or is a digital signature verified.
    // If any of these checks are true - the DLL is legit and can be injected into the application.
    if (!FileExists(libName) ||
        IsDllInAllowList(libName) ||
        VerifyDigitalSignature(libName.c_str()))
    {
        return TrueLoadLibraryExW(lpLibFileName, hFile, dwFlags);
    }

    // In case when a dynamic-link library is not in the allowed list and not verified - need to block loading this DLL.
    fflush(stdout);
    std::wcout << L"InjectDLLProtectionPlugin.dll - Something is trying to inject DLL: " << libName.c_str() << std::endl;
    std::wcout << L"InjectDLLProtectionPlugin.dll - Rejected LoadLibrary function call! Application is protected from DLL injection!" << std::endl;

    SetLastError(ERROR_ACCESS_DENIED);
    return NULL;
}

void HookFunction()
{
    DetourRestoreAfterWith();
    fflush(stdout);

    DetourTransactionBegin();
    DetourUpdateThread(GetCurrentThread());
    DetourAttach(&(reinterpret_cast<PVOID&>(TrueLoadLibraryExW)), HookLoadLibraryExW);
    const LONG error = DetourTransactionCommit();

    if (error == NO_ERROR)
    {
        std::wcout << L"InjectDLLProtectionPlugin.dll - Detoured LoadLibraryExW()." << std::endl;
    }
    else
    {
        std::wcout << L"InjectDLLProtectionPlugin.dll - Error detouring LoadLibraryExW(): " << error << std::endl;
    }
}

void UnhookFunction()
{
    DetourTransactionBegin();
    DetourUpdateThread(GetCurrentThread());
    DetourDetach(&(reinterpret_cast<PVOID&>(TrueLoadLibraryExW)), HookLoadLibraryExW);
    const LONG error = DetourTransactionCommit();

    std::wcout << L"InjectDLLProtectionPlugin.dll - Removed LoadLibraryExW(). Result = " << error << std::endl;
    fflush(stdout);
}

bool FileExists(const std::wstring& filePath)
{
    const DWORD fileAttributes = GetFileAttributesW(filePath.c_str());

    return (fileAttributes != INVALID_FILE_ATTRIBUTES) &&
        !(fileAttributes & FILE_ATTRIBUTE_DIRECTORY);
}

bool VerifyDigitalSignature(const wchar_t* filePath)
{
    WINTRUST_FILE_INFO fileInfo = {};
    fileInfo.cbStruct = sizeof(WINTRUST_FILE_INFO);
    fileInfo.pcwszFilePath = filePath;
    fileInfo.hFile = {};

    GUID guidAction = WINTRUST_ACTION_GENERIC_VERIFY_V2;
    WINTRUST_DATA winTrustData = {};
    winTrustData.cbStruct = sizeof(WINTRUST_DATA);
    winTrustData.dwUIChoice = WTD_UI_NONE;
    winTrustData.fdwRevocationChecks = WTD_REVOKE_NONE;
    winTrustData.dwUnionChoice = WTD_CHOICE_FILE;
    winTrustData.pFile = &fileInfo;
    winTrustData.dwProvFlags = WTD_SAFER_FLAG;

    const LONG result = WinVerifyTrust(NULL, &guidAction, &winTrustData);
    return result == ERROR_SUCCESS;
}

std::wstring ToLowerCase(const std::wstring& input)
{
    std::wstring lowerCaseString;
    std::locale loc;

    for (std::wstring::size_type i = 0; i < input.length(); ++i)
    {
        lowerCaseString += std::tolower(input[i], loc);
    }

    return lowerCaseString;
}

bool IsDllInAllowList(const std::wstring& filePath)
{
    // List of allowed system folders where DLLs can be placed.
    // The list of allowed folders can be modified and changed by the user. 
    // Here are the list of folders that the user trusts.
    static const std::vector<std::wstring> allowedFolders =
    {
        L"c:\\windows\\system32",
        L"c:\\windows\\syswow64",       // For 32-bit dlls on 64-bit systems.
        L"c:\\windows\\microsoft.net",  // For .net framework and core folder.
        L"c:\\windows\\assembly",       // For common libraries.
        L"c:\\program files",
        L"c:\\program files (x86)",     // For 32-bit applications on 64-bit systems
        L"c:\\programdata",
        L"c:\\users\\public"
    };

    const std::wstring dllPath = ToLowerCase(filePath);
    for (const auto& allowedFolder : allowedFolders)
    {
        if (dllPath.find(allowedFolder) != std::wstring::npos)
        {
            return true;
        }
    }
    return false;
}
