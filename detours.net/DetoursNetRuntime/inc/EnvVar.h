#pragma once

#include <vector>
#include <string>

namespace runtime
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

		void ParseFromString(const std::string& allVar);
		void LoadFromAPI();
		void Add(const std::string& name, const std::string& value);
		void Update(const std::string& name, const std::string& value);
		std::string Data() const;

	private:
		///
		///	@brief	all var lines
		///
		std::vector<std::string> mEnVarLines;
	};
}