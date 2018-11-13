#pragma once

#include <Windows.h>
#include "detours.h"

#ifdef __cplusplus
extern "C"
{
#endif

__declspec(dllexport) BOOL DetoursPatchIAT(HMODULE hModule, PVOID pFunction, PVOID pReal);

#ifdef __cplusplus
}
#endif