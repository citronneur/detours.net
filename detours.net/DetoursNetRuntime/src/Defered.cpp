#include "Defered.h"

namespace detoursnetsandbox
{
	Defered::Defered(std::function<void(void)> deferedFunction)
		: mDeferedFunction{ std::forward<std::function<void(void)>>(deferedFunction) }
	{}

	Defered::~Defered()
	{
		mDeferedFunction();
	}
}