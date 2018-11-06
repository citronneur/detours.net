#include <Windows.h>
#include "../inc/DetoursDll.h"

typedef struct _DetoursUnsandboxContext {
	PVOID pImport;
	PVOID pReal;
} DetoursUnsandboxContext;

static BOOL Unsandbox(_In_opt_ PVOID pContext, _In_ DWORD nOrdinal, _In_opt_ LPCSTR pszFunc, _In_opt_ PVOID* ppvFunc)
{
	DetoursUnsandboxContext* context = reinterpret_cast<DetoursUnsandboxContext*>(pContext);
	if (ppvFunc != nullptr && *ppvFunc == context->pImport)
	{
		DWORD oldrights = 0;
		if (VirtualProtect(ppvFunc, sizeof(LPVOID), PAGE_READWRITE, &oldrights))
		{
			*ppvFunc = context->pReal;
			VirtualProtect(ppvFunc, sizeof(LPVOID), oldrights, &oldrights);
		}
		return FALSE;
	}

	return TRUE;
}


BOOL DetoursPatchIAT(HMODULE hModule, PVOID pFunction, PVOID pReal)
{
	DetoursUnsandboxContext context = { pFunction, pReal };
	return DetourEnumerateImportsEx(hModule, &context, nullptr, Unsandbox);
}



