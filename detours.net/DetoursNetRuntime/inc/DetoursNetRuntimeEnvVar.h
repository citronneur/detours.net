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

		void parseFromString(const std::string& allVar);
		void loadFromAPI();
		void add(const std::string& name, const std::string& value);
		void update(const std::string& name, const std::string& value);
		std::string data() const;

	private:
		///
		///	@brief	all var lines
		///
		std::vector<std::string> mEnVarLines;
	};
}