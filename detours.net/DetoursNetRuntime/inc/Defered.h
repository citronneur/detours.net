#pragma once

#include <functional>
namespace runtime
{
	///
	///	@brief	Handle RAI for a lambda function
	///
	class Defered
	{
	public:
		///
		///	@param	deferedFunction function to call when RAI
		///
		Defered(const std::function<void(void)>& deferedFunction);
		Defered(std::function<void(void)>&& deferedFunction);
		
		///
		///	@brief	call when dtor is called
		///
		virtual ~Defered();

	private:
		///
		///	@brief	handle on target fonction
		///
		std::function<void(void)> mDeferedFunction;
	};
}