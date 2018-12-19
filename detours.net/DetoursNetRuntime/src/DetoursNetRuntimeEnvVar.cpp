#include "DetoursNetRuntimeEnvVar.h"
#include "DetoursNetRuntimeDefered.h"
#include <sstream>
#include <Windows.h>

namespace
{
	static bool iequals(const std::wstring& a, const std::wstring& b)
	{
		return std::equal(a.begin(), a.end(),
			b.begin(), b.end(),
			[](wchar_t a, wchar_t b) {
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

	void EnvVar::parseFromString(const std::wstring& environmentBlock)
	{
		using wistringstream = std::basic_istringstream<wchar_t>;
		std::wistringstream iss(environmentBlock);
		std::wstring envValue;

		for(;;)
		{
			std::getline<wchar_t>(iss, envValue, '\0');
			if (envValue.empty())
				break;
			mEnVarLines.push_back(envValue);

		}
	}

	void EnvVar::loadFromAPI()
	{
		LPWCH environmentStrings = GetEnvironmentStrings();
		detoursnetruntime::Defered guard([&environmentStrings]() {
			FreeEnvironmentStrings(environmentStrings);
		});

		// compute the size environments strings
		size_t envSize = 0;
		while (true)
		{
			if (environmentStrings[envSize] == L'\0' && environmentStrings[envSize + 1] == L'\0')
			{
				envSize += 2;
				break;
			}
			envSize++;
		}

		// Remove last \0
		this->parseFromString(std::wstring(environmentStrings, envSize - 1));
	}

	void EnvVar::add(const std::wstring& name, const std::wstring& value)
	{
		mEnVarLines.push_back(name + TEXT("=") + value);
	}

	void EnvVar::update(const std::wstring& name, const std::wstring& value)
	{
		for (auto& line : mEnVarLines)
		{
			if (iequals(line.substr(0, name.length()), name))
			{
				line += TEXT(";") + value;
			}
		}
	}

	std::wstring EnvVar::data() const
	{
		std::wstring result;
		for (auto line : mEnVarLines)
		{
			result.append(line);
			result.append(TEXT("\0"), 1);
		}
		result.append(TEXT("\0"), 1);
		return result;
	}
}