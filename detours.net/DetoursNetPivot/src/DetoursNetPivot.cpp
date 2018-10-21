#include <Windows.h>
#include <winternl.h>
#include "detours.h"
#include <metahost.h>
#include <mscoree.h>


#import "mscorlib.tlb" raw_interfaces_only				\
    high_property_prefixes("_get","_put","_putref")		\
    rename("ReportEvent", "InteropServices_ReportEvent")
using namespace mscorlib;

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
	// build runtime
	ICLRMetaHost *pMetaHost = NULL;
	if (FAILED(CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost))))
	{
		// exit with error code
		return -1;
	}

	ICLRRuntimeInfo *pRuntimeInfo = NULL;
	if (FAILED(pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo))))
	{
		// exit with error code
		return -1;
	}

	// Check if the specified runtime can be loaded into the process.
	BOOL loadable;
	if (FAILED(pRuntimeInfo->IsLoadable(&loadable) || !loadable))
	{
		// exit with error code
		return -1;
	}
	
	ICorRuntimeHost *pRuntimeHost = NULL;
	if (FAILED(pRuntimeInfo->GetInterface(CLSID_CorRuntimeHost, IID_PPV_ARGS(&pRuntimeHost))))
	{
		// exit with error code
		return -1;
	}

	// start runtime
	if (FAILED(pRuntimeHost->Start()))
	{
		// exit with error code
		return -1;
	}

	IUnknown* pAppDomain = NULL;
	if (FAILED(pRuntimeHost->GetDefaultDomain(&pAppDomain)))
	{
		return -1;
	}

	_AppDomain* pDefaultAppDomain = NULL;
	if (FAILED(pAppDomain->QueryInterface(IID_PPV_ARGS(&pDefaultAppDomain))))
	{
		return -1;
	}

	_Assembly* pDetoursNet;
	if (FAILED(pDefaultAppDomain->Load_2(L"c:\\dev\\build_x64\\bin\\Debug\\DetoursNet.dll", &pDetoursNet)))
	{
		return -1;
	}

	_Assembly* pDetoursNetLoader;
	if (FAILED(pDefaultAppDomain->Load_2(L"c:\\dev\\build_x64\\bin\\Debug\\DetoursNetLoader.dll", &pDetoursNetLoader)))
	{
		return -1;
	}


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