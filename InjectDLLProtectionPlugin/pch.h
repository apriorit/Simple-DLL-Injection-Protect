// pch.h: This is a precompiled header file.
// Files listed below are compiled only once, improving build performance for future builds.
// This also affects IntelliSense performance, including code completion and many code browsing features.
// However, files listed here are ALL re-compiled if any one of them is updated between builds.
// Do not add files here that you will be updating frequently as this negates the performance advantage.

#ifndef PCH_H
#define PCH_H

// add headers that you want to pre-compile here
#include "framework.h"
#include <iostream>
#include <vector>

#include <detours.h> // Allow calls to the Detours library

#include <wintrust.h> // WinVerifyTrust
#include <softpub.h> // WINTRUST_ACTION_GENERIC_VERIFY_V2

#include <locale> // std::locale, std::tolower
#endif //PCH_H
