#include <Windows.h>
#include <winternl.h>
#include "detours.h"
#include <metahost.h>

// All message out of CRT
static const wchar_t* sMessageInitConsole = L"[+] Init console\n";
static const wchar_t* sFailedCreateInstance = L"[!] Failed to create meta host instance\n";
static const wchar_t* sFailedToGetRuntimeVersion = L"[!] Failed to get specified runtime\n";
static const wchar_t* sFailedToInitRuntime = L"[!] Failed to init runtime\n";
static const wchar_t* sFailedToStartRuntime = L"[!] Failed to start runtime\n";
static const wchar_t* sFailedToLoadRuntime = L"[!] Unable to load runtime\n";
static const wchar_t* sFailedToLoadAssembly = L"[!] Failed to load detoursnet assembly\n";

// With detours inject mechanism we need an export
// Detours rewrite import table with target DLL as first entry
// If dll has no export loader throw 0xc000007b
__declspec(dllexport) void Dummy()
{

}

// The main signature
typedef int(*MAIN_FUNCTION)(void);

// Pointer to keep trace of real main
MAIN_FUNCTION Main = NULL;

// New Main!
static int DetourMain(void)
{
	ICLRRuntimeHost *pClrRuntimeHost = NULL;

	// fisrt try to attach console
	if (!AllocConsole()) {
		return -1;
	}

	HANDLE std_out = GetStdHandle(STD_OUTPUT_HANDLE);

	// use write console because no crt is init
	DWORD szMessage;
	WriteConsole(std_out, sMessageInitConsole, sizeof(sMessageInitConsole), &szMessage, NULL);

	// build meta host
	ICLRMetaHost *pMetaHost = NULL;
	if (FAILED(CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost)))) {
		WriteConsole(std_out, sFailedCreateInstance, sizeof(sFailedCreateInstance), &szMessage, NULL);
		return -1;
	}

	// Load runtime
	ICLRRuntimeInfo *pRuntimeInfo = NULL;
	if (FAILED(pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo)))) {
		WriteConsole(std_out, sFailedToGetRuntimeVersion, sizeof(sFailedToGetRuntimeVersion), &szMessage, NULL);
		return -1;
	}

	BOOL isLoadable = FALSE;
	if (FAILED(pRuntimeInfo->IsLoadable(&isLoadable)) || !isLoadable) {
		WriteConsole(std_out, sFailedToLoadRuntime, sizeof(sFailedToLoadRuntime), &szMessage, NULL);
		return -1;
	}

	if (FAILED(pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pClrRuntimeHost)))) {
		WriteConsole(std_out, sFailedToInitRuntime, sizeof(sFailedToInitRuntime), &szMessage, NULL);
		return -1;
	}

	// start runtime
	if (FAILED(pClrRuntimeHost->Start())) {
		WriteConsole(std_out, sFailedToStartRuntime, sizeof(sFailedToStartRuntime), &szMessage, NULL);
		return -1;
	}

	// execute managed assembly
	DWORD pReturnValue;
	if (FAILED(pClrRuntimeHost->ExecuteInDefaultAppDomain(
		L"c:\\dev\\build_x64\\bin\\Debug\\DetoursNet.dll",
		L"detoursnet.DetoursNetLoader",
		L"Start",
		L"",
		&pReturnValue))) {
		WriteConsole(std_out, sFailedToLoadAssembly, sizeof(sFailedToLoadAssembly), &szMessage, NULL);
		return -1;
	}


	// free resources
	pMetaHost->Release();
	pRuntimeInfo->Release();
	pClrRuntimeHost->Release();

	// jump to real main
	return Main();
}

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