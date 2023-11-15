#pragma once

// Handler for the hooked LoadLibraryExW function calls.
HMODULE WINAPI HookLoadLibraryExW(
    IN LPCWSTR lpLibFileName,
    HANDLE  hFile,
    IN DWORD   dwFlags
);

// Attach a hook to a target function.
void HookFunction();

// Detach a hook from a target function.
void UnhookFunction();

// Checks if the file exists in the system.
bool FileExists(const std::wstring& filePath);

// Function to verify the digital signature of a file.
// Returns "true" if verified. Otherwise returns "false".
// Read article: "Example C Program: Verifying the Signature of a PE File" to get more information: 
// https://learn.microsoft.com/en-us/windows/win32/seccrypto/example-c-program--verifying-the-signature-of-a-pe-file?redirectedfrom=MSDN
bool VerifyDigitalSignature(const wchar_t* filePath);

// Transoforms string to lowercase string.
std::wstring ToLowerCase(const std::wstring& input);

// Checks if a file is in the allow list.
bool IsDllInAllowList(const std::wstring& filePath);