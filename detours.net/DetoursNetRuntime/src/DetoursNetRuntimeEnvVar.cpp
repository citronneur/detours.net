#include "DetoursNetRuntimeEnvVar.h"
#include "DetoursNetRuntimeDefered.h"
#include <sstream>
#include <Windows.h>

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

namespace detoursnetruntime
{
	EnvVar::EnvVar()
	{}

	EnvVar::~EnvVar()
	{
	}

	void EnvVar::parseFromString(const std::string& environmentBlock)
	{
		std::istringstream iss(environmentBlock);
		std::string envValue;

		for(;;)
		{
			std::getline(iss, envValue, '\0');
			if (envValue.empty())
				break;
			mEnVarLines.push_back(envValue);

		}
	}

	void EnvVar::loadFromAPI()
	{
		LPCH environmentStrings = GetEnvironmentStrings();
		detoursnetruntime::Defered guard([&environmentStrings]() {
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
		this->parseFromString(std::string(environmentStrings, envSize - 1));
	}

	void EnvVar::add(const std::string& name, const std::string& value)
	{
		mEnVarLines.push_back(name + "=" + value);
	}

	void EnvVar::update(const std::string& name, const std::string& value)
	{
		for (auto& line : mEnVarLines)
		{
			if (iequals(line.substr(0, name.length()), name))
			{
				line += ";" + value;
			}
		}
	}

	std::string EnvVar::data() const
	{
		std::string result;
		for (auto line : mEnVarLines)
		{
			result.append(line);
			result.append("\0", 1);
		}
		result.append("\0", 1);
		return result;
	}
}