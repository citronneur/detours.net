#include <Windows.h>
#include <iostream>
#include "detours.h"
int main(int argc, char** argv)
{
	PROCESS_INFORMATION processInfo;
	STARTUPINFO startupInfo = { 0 };
	startupInfo.cb = sizeof(startupInfo);
	if (!DetourCreateProcessWithDll(
			TEXT("C:\\Program Files (x86)\\Mozilla Firefox\\firefox.exe"), 
			NULL, 
			NULL, 
			NULL, 
			FALSE, 
			NULL, 
			NULL, 
			NULL, 
			&startupInfo, 
			&processInfo,
			TEXT("pivot.dll"),
			NULL)
		)
	{
		std::cerr << "[!] cannot create process" << std::endl;
		return 1;
	}
	return 0;
}