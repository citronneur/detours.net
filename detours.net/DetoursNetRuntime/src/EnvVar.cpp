#include "EnvVar.h"
#include "Defered.h"
#include <sstream>
#include <Windows.h>

namespace runtime
{
	EnvVar::EnvVar()
	{}

	EnvVar::~EnvVar()
	{
	}

	void EnvVar::ParseFromString(const std::string& environmentBlock)
	{
		std::istringstream iss(environmentBlock);
		std::string envValue;

		do
		{
			std::getline(iss, envValue, '\0');
			mEnVarLines.push_back(envValue);

		} while (!envValue.empty());
	}

	void EnvVar::LoadFromAPI()
	{
		LPCH environmentStrings = GetEnvironmentStrings();
		runtime::Defered guard([&environmentStrings]() {
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

		// Remove last \0
		this->ParseFromString(std::string(environmentStrings, envSize - 1));
	}
}