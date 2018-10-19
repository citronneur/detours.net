#include <Windows.h>
#include <winternl.h>
#include "detours.h"
#include <metahost.h>


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
	//DetoursNetLoader_Start();
	HRESULT hr;
	ICLRMetaHost *pMetaHost = NULL;
	ICLRRuntimeInfo *pRuntimeInfo = NULL;
	ICLRRuntimeHost *pClrRuntimeHost = NULL;

	// build runtime
	hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost));
	hr = pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo));
	hr = pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pClrRuntimeHost));

	// start runtime
	hr = pClrRuntimeHost->Start();

	// execute managed assembly
	DWORD pReturnValue;
	hr = pClrRuntimeHost->ExecuteInDefaultAppDomain(
		L"c:\\dev\\build_x64\\bin\\Debug\\DetoursNetLoader.dll",
		L"detoursnetloader.DetoursNetLoader",
		L"DetoursNetLoader_Start",
		L"",
		&pReturnValue);

	
	// free resources
	pMetaHost->Release();
	pRuntimeInfo->Release();
	pClrRuntimeHost->Release();

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