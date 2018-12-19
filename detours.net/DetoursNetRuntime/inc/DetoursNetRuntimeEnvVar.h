#pragma once

#include <vector>
#include <string>

namespace detoursnetruntime
{
	///
	///	@brief	Environment variable
	///
	class EnvVar
	{
	public:
		///
		///	@param	ctor
		///
		EnvVar();

		///
		///	@brief	dtor
		///
		virtual ~EnvVar();

		void parseFromString(const std::wstring& allVar);
		void loadFromAPI();
		void add(const std::wstring& name, const std::wstring& value);
		void update(const std::wstring& name, const std::wstring& value);
		std::wstring data() const;

	private:
		///
		///	@brief	all var lines
		///
		std::vector<std::wstring> mEnVarLines;
	};
}