#include "EnvVar.h"
#include "Defered.h"
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

		for(;;)
		{
			std::getline(iss, envValue, '\0');
			if (envValue.empty())
				break;
			mEnVarLines.push_back(envValue);

		}
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

	void EnvVar::Add(const std::string& name, const std::string& value)
	{
		mEnVarLines.push_back(name + "=" + value);
	}

	void EnvVar::Update(const std::string& name, const std::string& value)
	{
		for (auto& line : mEnVarLines)
		{
			if (iequals(line.substr(0, name.length()), name))
			{
				line += ";" + value;
			}
		}
	}

	std::string EnvVar::Data() const
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