#include <Windows.h>
#include <winternl.h>
#include "DetoursDll.h"
#include <metahost.h>
#include <map>
#include <string>
#include <tuple>
#include <mutex>

namespace pivot
{
	class Cache
	{
	public:
		//
		//	@brief	return thread safe instance of cache
		//
		static Cache& getInstance()
		{
			static Cache cache;
			return cache;
		}

		void update(HMODULE hModule, std::string funcName, PVOID pReal)
		{
			std::lock_guard<std::mutex> guard(mLock);
			mCache[std::make_tuple(hModule, funcName)] = pReal;
		}

		PVOID find(HMODULE hModule, std::string funcName)
		{
			std::lock_guard<std::mutex> guard(mLock);
			auto real = mCache.find(std::make_tuple(hModule, funcName));
			if (real == mCache.end())
			{
				return nullptr;
			}

			return real->second;
		}
	private:
		// 
		//	@brief	Cache for all hooked function
		//
		std::map<std::tuple<HMODULE, std::string>, PVOID> mCache;

		//
		//	@brief	Cache access thread safe
		//
		std::mutex mLock;
	};
	

	/*
		@brief	Hook GetProcAddress called from clr.dll module
		@param	hModule	module of target function
		@param	lpProcName name of function
	 */
	static FARPROC WINAPI GetProcAddressCLR(_In_ HMODULE hModule, _In_ LPCSTR lpProcName)
	{
		auto real = Cache::getInstance().find(hModule, lpProcName);
		// already hooked
		if (real != nullptr) {
			return reinterpret_cast<FARPROC>(real);
		}

		return GetProcAddress(hModule, lpProcName);
	}
}

// With detours inject technic we need an export
// Detours rewrite import table with target DLL as first entry
// If dll has no export loader throw 0xc000007b

extern "C"
__declspec(dllexport) void DetoursPivotSetGetProcAddressCache(HMODULE hModule, LPCSTR lpProcName, PVOID pReal)
{
	pivot::Cache::getInstance().update(hModule, lpProcName, pReal);
}



// The main signature
typedef int(*MAIN_FUNCTION)(void);

// Pointer to keep trace of real main
MAIN_FUNCTION Main = NULL;

// Non CRT Print function
static void Print(const wchar_t* message)
{
	HANDLE std_out = GetStdHandle(STD_OUTPUT_HANDLE);

	// use write console API because no crt is init
	DWORD szMessage;
	WriteConsole(std_out, message, lstrlenW(message), &szMessage, NULL);
	FlushFileBuffers(std_out);
}

// New Main!
static int DetourMain(void)
{
	ICLRRuntimeHost *pClrRuntimeHost = NULL;
	
	// fisrt try to attach console
	AllocConsole();

	// use write console because no crt is init
	Print(L"[+] Init console\n");

	// build meta host
	ICLRMetaHost *pMetaHost = NULL;
	if (FAILED(CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost)))) {
		Print(L"[!] Failed to create meta host instance\n");
		return -1;
	}

	// Load runtime
	ICLRRuntimeInfo *pRuntimeInfo = NULL;
	if (FAILED(pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo)))) {
		Print(L"[!] Failed to get specified runtime\n");
		return -1;
	}

	BOOL isLoadable = FALSE;
	if (FAILED(pRuntimeInfo->IsLoadable(&isLoadable)) || !isLoadable) {
		Print(L"[!] Unable to load runtime\n");
		return -1;
	}

	if (FAILED(pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pClrRuntimeHost)))) {
		Print(L"[!] Failed to init runtime\n");
		return -1;
	}

	// start runtime
	if (FAILED(pClrRuntimeHost->Start())) {
		Print(L"[!] Failed to start runtime\n");
		return -1;
	}

	// patch iat to handle pinvoke from mscorlib
	DetoursPatchIAT(GetModuleHandle(TEXT("clr.dll")), GetProcAddress, pivot::GetProcAddressCLR);

	// execute managed assembly
	DWORD pReturnValue;
	if (FAILED(pClrRuntimeHost->ExecuteInDefaultAppDomain(
		L"c:\\dev\\build_x64\\bin\\Debug\\DetoursNet.dll",
		L"DetoursNet.Loader",
		L"Start",
		L"",
		&pReturnValue))) {
		Print(L"[!] Failed to load DetoursNet assembly\n");
		return -1;
	}

	// free resources
	pMetaHost->Release();
	pRuntimeInfo->Release();
	pClrRuntimeHost->Release();

	// jump to real main
	return Main();
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
		Main = reinterpret_cast<MAIN_FUNCTION>(pImgNTHeaders->OptionalHeader.AddressOfEntryPoint + reinterpret_cast<LPBYTE>(hMainModule));

		// Detour main function
		DetourTransactionBegin();
		DetourUpdateThread(GetCurrentThread());
		DetourAttach(&reinterpret_cast<PVOID&>(Main), DetourMain);
		DetourTransactionCommit();
	}
		
	return TRUE; 
}