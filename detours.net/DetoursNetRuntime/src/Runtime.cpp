#include <Windows.h>
#include <iostream>
#include <sstream>
#include "detours.h"
#include "Defered.h"
#include "Utils.h"
#include "EnvVar.h"

int main(int argc, char** argv)
{
	runtime::EnvVar vars;
	vars.LoadFromAPI();

	// Get current path
	char sCurrentDirectory[MAX_PATH];
	if (GetCurrentDirectory(MAX_PATH, sCurrentDirectory) == 0) {
		std::cerr << "[!] cannot get current directory" << std::endl;
		return 1;
	}

	vars.Update("PATH", sCurrentDirectory);
	vars.Add("DETOURSNET_ASSEMBLY_PLUGIN", argv[1]);

	PROCESS_INFORMATION processInfo;
	STARTUPINFO startupInfo = { 0 };
	startupInfo.cb = sizeof(startupInfo);
	if (!DetourCreateProcessWithDll(
			argv[2],
			NULL, 
			NULL, 
			NULL, 
			FALSE, 
			CREATE_SUSPENDED, 
			reinterpret_cast<LPVOID>(const_cast<char*>(vars.Data().data())),
			NULL, 
			&startupInfo, 
			&processInfo,
			TEXT("DetoursNetPivot.dll"),
			NULL)
		)
	{
		std::cerr << "[!] cannot create process" << std::endl;
		return 1;
	}
	ResumeThread(processInfo.hThread);
	return 0;
} 