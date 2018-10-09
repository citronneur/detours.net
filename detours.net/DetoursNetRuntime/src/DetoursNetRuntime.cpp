#include <Windows.h>
#include <iostream>
#include <sstream>
#include "detours.h"
#include "Defered.h"
#include "Utils.h"

int main(int argc, char** argv)
{
	PROCESS_INFORMATION processInfo;
	STARTUPINFO startupInfo = { 0 };
	startupInfo.cb = sizeof(startupInfo);
	if (!DetourCreateProcessWithDll(
			TEXT("C:\\Windows\\notepad.exe"), 
			NULL, 
			NULL, 
			NULL, 
			FALSE, 
			NULL, 
			reinterpret_cast<LPVOID>(const_cast<char*>(detoursnetsandbox::utils::UpdateEnvVariableWithPath("c:\\dev\\build_x64\\bin\\Debug").data())),
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
	return 0;
}