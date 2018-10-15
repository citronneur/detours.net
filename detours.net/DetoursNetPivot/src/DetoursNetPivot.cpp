#include <Windows.h>
#include <winternl.h>
#include "detours.h"


extern "C" int DetoursNetLoader_Start();

// With detours inject mechanism we need an export
// Detours rewrite import table with target DLL as first entry
// If dll has no export loader throw 0xc000007b
__declspec(dllexport) void Dummy()
{

}

typedef int(*MAIN_CALLBACK)(void);

MAIN_CALLBACK Real_Main = NULL;

static int MyMain(void)
{
	DetoursNetLoader_Start();
	return Real_Main();
}

BOOL WINAPI DllMain(_In_ HINSTANCE hinstDLL, _In_ DWORD fdwReason, _In_ LPVOID lpvReserved) {
	if (fdwReason == DLL_PROCESS_ATTACH)
	{
		//DetoursNetLoader_Start();
		HMODULE hMainModule = (HMODULE)NtCurrentTeb()->ProcessEnvironmentBlock->Reserved3[1];
		PIMAGE_DOS_HEADER pImgDosHeaders = (PIMAGE_DOS_HEADER)hMainModule;
		PIMAGE_NT_HEADERS pImgNTHeaders = (PIMAGE_NT_HEADERS)((LPBYTE)pImgDosHeaders + pImgDosHeaders->e_lfanew);
		Real_Main = (MAIN_CALLBACK)(pImgNTHeaders->OptionalHeader.AddressOfEntryPoint + (LPBYTE)hMainModule);
		DetourTransactionBegin();
		DetourUpdateThread(GetCurrentThread());
		DetourAttach(&(PVOID&)Real_Main, MyMain);
		DetourTransactionCommit();
	}
		
	return TRUE; 
}