#include <Windows.h>
#include <winternl.h>
#include "DetoursDll.h"
#include <metahost.h>
#include "../inc/Cache.h"

namespace {
	/*!
	*	@brief	Hook GetProcAddress called from clr.dll module
	*	@param	hModule	module of target function
	*	@param	lpProcName name of function
	*/
	static FARPROC WINAPI GetProcAddressCLR(HMODULE module, LPCSTR funcName)
	{
		auto real = pivot::Cache::GetInstance().find(module, funcName);
		// already hooked
		if (real != nullptr) {
			return reinterpret_cast<FARPROC>(real);
		}

		return GetProcAddress(module, funcName);
	}


	// With detours inject technic we need an export
	// Detours rewrite import table with target DLL as first entry
	// If dll has no export loader throw 0xc000007b

	/*!
	 *	@brief	Function from Detours.dll (managed) to indicate new hook
	 *	@param	hModule		source module
	 *	@param	lpProcName	name of function
	 *	@param	pReal		real address
	 */
	extern "C"
		__declspec(dllexport) void DetoursCLRSetGetProcAddressCache(HMODULE module, LPCSTR funcName, PVOID real)
	{
		pivot::Cache::GetInstance().update(module, funcName, real);
	}


	using MainFunction_T = int(*)(void);

	// Pointer to keep trace of real main
	MainFunction_T gMain = NULL;

	// New Main!
	static int DetourMain(void)
	{
		ICLRRuntimeHost *pClrRuntimeHost = NULL;

		// build meta host
		ICLRMetaHost *pMetaHost = NULL;
		if (FAILED(CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost)))) {
			return -1;
		}

		// Load runtime
		ICLRRuntimeInfo *pRuntimeInfo = NULL;
		if (FAILED(pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo)))) {
			return -1;
		}

		BOOL isLoadable = FALSE;
		if (FAILED(pRuntimeInfo->IsLoadable(&isLoadable)) || !isLoadable) {
			return -1;
		}

		if (FAILED(pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pClrRuntimeHost)))) {
			return -1;
		}

		// start runtime
		if (FAILED(pClrRuntimeHost->Start())) {
			return -1;
		}

		// patch iat to handle pinvoke from mscorlib
		DetoursPatchIAT(GetModuleHandle(TEXT("clr.dll")), GetProcAddress, GetProcAddressCLR);

		// execute managed assembly
		DWORD pReturnValue;
		if (FAILED(pClrRuntimeHost->ExecuteInDefaultAppDomain(
			L"c:\\dev\\build_x64\\bin\\Debug\\DetoursNet.dll",
			L"DetoursNet.Loader",
			L"Start",
			L"",
			&pReturnValue))) {
			return -1;
		}

		// free resources
		pMetaHost->Release();
		pRuntimeInfo->Release();
		pClrRuntimeHost->Release();

		// jump to real main
		return gMain();
	}
}

// Dll main fist code execute in current process
BOOL WINAPI DllMain(_In_ HINSTANCE hinstDLL, _In_ DWORD fdwReason, _In_ LPVOID lpvReserved) {
	if (fdwReason == DLL_PROCESS_ATTACH)
	{
		// Get the main module base address
		auto hMainModule = reinterpret_cast<HMODULE>(NtCurrentTeb()->ProcessEnvironmentBlock->Reserved3[1]);

		// Parse the dos header
		auto pImgDosHeaders = reinterpret_cast<PIMAGE_DOS_HEADER>(hMainModule);
		if (pImgDosHeaders->e_magic != IMAGE_DOS_SIGNATURE) {
			return TRUE;
		}

		// Parse the NT header
		auto pImgNTHeaders = reinterpret_cast<PIMAGE_NT_HEADERS>((reinterpret_cast<LPBYTE>(pImgDosHeaders) + pImgDosHeaders->e_lfanew));
		if (pImgNTHeaders->Signature != IMAGE_NT_SIGNATURE) {
			return TRUE;
		}
		// compute main function address
		gMain = reinterpret_cast<MainFunction_T>(pImgNTHeaders->OptionalHeader.AddressOfEntryPoint + reinterpret_cast<LPBYTE>(hMainModule));

		// Detour main function
		DetourTransactionBegin();
		DetourUpdateThread(GetCurrentThread());
		DetourAttach(&reinterpret_cast<PVOID&>(gMain), DetourMain);
		DetourTransactionCommit();
	}
		
	return TRUE; 
}