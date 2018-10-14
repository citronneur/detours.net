#include <Windows.h>
#include "detours.h"


extern "C" int DetoursNetLoader_Start();

// With detours inject mechanism we need an export
// Detours rewrite import table with target DLL as first entry
// If dll has no export loader throw 0xc000007b
__declspec(dllexport) void Dummy()
{

}

typedef void(*INITERM_CALLBACK)(void* start, void* end);
extern "C" void _initterm(void* start, void* end);

INITERM_CALLBACK Real_InitTerm = _initterm;

static void MyInitTerm(void* start, void* end)
{
	DetoursNetLoader_Start();
	Real_InitTerm(start, end);
}

BOOL WINAPI DllMain(_In_ HINSTANCE hinstDLL, _In_ DWORD fdwReason, _In_ LPVOID lpvReserved) {
	if (fdwReason == DLL_PROCESS_ATTACH)
	{
		//DetoursNetLoader_Start();
		DetourTransactionBegin();
		DetourUpdateThread(GetCurrentThread());
		DetourAttach(&(PVOID&)Real_InitTerm, MyInitTerm);
		DetourTransactionCommit();
	}
		
	return TRUE; 
}