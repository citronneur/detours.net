#include <Windows.h>
#include <iostream>
#include <sstream>
#include "detours.h"
#include "DetoursNetRuntimeDefered.h"
#include "DetoursNetRuntimeEnvVar.h"


void usage()
{
	std::cout << "DetoursNetRuntime.exe plugin.dll application.exe [args]" << std::endl;
}

int wmain(int argc, wchar_t** argv)
{
	if (argc < 3)
	{
		usage();
		return 1;
	}

	detoursnetruntime::EnvVar vars;
	vars.loadFromAPI();

	// Get current path
	wchar_t sCurrentDirectory[MAX_PATH];
	if (GetCurrentDirectory(MAX_PATH, sCurrentDirectory) == 0) {
		std::cerr << "[!] cannot get current directory" << std::endl;
		return 1;
	}

	vars.update(TEXT("PATH"), sCurrentDirectory);
	vars.add(TEXT("DETOURSNET_ASSEMBLY_PLUGIN"), argv[1]);

	// build command line
	std::wstring commandLine = argv[2];
	for (int i = 3; i < argc; i++)
	{
		commandLine += TEXT(" ") + std::wstring(argv[i]);
	}

	PROCESS_INFORMATION processInfo;
	STARTUPINFO startupInfo = { 0 };
	startupInfo.cb = sizeof(startupInfo);
	if (!DetourCreateProcessWithDll(
			NULL,
			const_cast<wchar_t*>(commandLine.c_str()),
			NULL, 
			NULL, 
			FALSE, 
			CREATE_SUSPENDED | CREATE_UNICODE_ENVIRONMENT,
			reinterpret_cast<LPVOID>(const_cast<wchar_t*>(vars.data().data())),
			NULL, 
			&startupInfo, 
			&processInfo,
			"DetoursNetCLR.dll",
			NULL)
		)
	{
		std::cerr << "[!] cannot create process" << std::endl;
		return 1;
	}
	ResumeThread(processInfo.hThread);
	return 0;
} 