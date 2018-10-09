#include "Utils.h"
#include "Defered.h"
#include <sstream>
#include <Windows.h>
#include <algorithm>

namespace
{
	static bool iequals(const std::string& a, const std::string& b)
	{
		return std::equal(a.begin(), a.end(),
			b.begin(), b.end(),
			[](char a, char b) {
			return tolower(a) == tolower(b);
		});
	}
}

namespace detoursnetsandbox
{
	namespace utils
	{
		std::string UpdateEnvVariableWithPath(const std::string& newPath)
		{
			LPCH environmentStrings = GetEnvironmentStrings();
			detoursnetsandbox::Defered guard([&environmentStrings]() {
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
			std::string environmentBlock(environmentStrings, envSize - 1);
			std::istringstream iss(environmentBlock);
			std::string envValue;
			std::string updatePath;
			do
			{
				std::getline(iss, envValue, '\0');
				if (iequals(envValue.substr(0, 4), "Path"))
					envValue += ";" + newPath;
				updatePath.append(envValue);
				updatePath.append("\0", 1);
			} while (!envValue.empty());
			updatePath.append("\0", 1);

			return updatePath;
		}
	}
}