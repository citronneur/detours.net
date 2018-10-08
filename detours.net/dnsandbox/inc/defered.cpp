#include <Windows.h>
#include <iostream>
#include <sstream>
#include <functional>
#include "detours.h"

class Defered
{
public:
	Defered(std::function<void(void)>&& deferedFunction)
		: mDeferedFunction{ deferedFunction }
	{}

	virtual ~Defered()
	{
		mDeferedFunction();
	}

private:
	std::function<void(void)> mDeferedFunction;
};

int main(int argc, char** argv)
{
	LPCH environmentStrings = GetEnvironmentStrings();
	Defered guard([&environmentStrings]() {
		FreeEnvironmentStrings(environmentStrings);
	});

	// compute the size environments strings
	size_t envSize = 0;
	while (true)
	{
		if (environmentStrings[envSize] == '\0' && environmentStrings[envSize + 1] == '\0')
		{
			envSize += 2;
			break;
		}
		envSize++;
	}

	std::string environmentBlock(environmentStrings, envSize - 1);
	std::istringstream iss(environmentBlock);
	std::string envValue;
	std::string updatePath;
	do
	{
		std::getline(iss, envValue, '\0');
		if (envValue.substr(0, 4) == "Path")
			envValue += ";C:\\dev\\build_x64\\detours.net\\pivot\\Debug;C:\\dev\\build_x64\\detours.net\\loader\\Debug";
		updatePath.append(envValue);
		updatePath.append("\0", 1);
	}
	while (!envValue.empty());
	updatePath.append("\0", 1);
		
	
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
			reinterpret_cast<LPVOID>(const_cast<char*>(updatePath.data())),
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