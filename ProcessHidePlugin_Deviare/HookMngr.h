#pragma once

std::wstring GetProcessNameFromPID(const DWORD& pid);

extern "C" HRESULT WINAPI HookFunction();
extern "C" HRESULT WINAPI UnhookFunction();
