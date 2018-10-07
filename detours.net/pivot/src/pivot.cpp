#include <Windows.h>
#include "detours.h"

extern "C" int Load();

BOOL WINAPI DllMain(_In_ HINSTANCE hinstDLL, _In_ DWORD fdwReason, _In_ LPVOID lpvReserved) {
	Load();
}