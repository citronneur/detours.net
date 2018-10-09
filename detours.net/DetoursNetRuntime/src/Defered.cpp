#include "Defered.h"

namespace detoursnetsandbox
{
	Defered::Defered(std::function<void(void)>&& deferedFunction)
		: mDeferedFunction{ deferedFunction }
	{}

	Defered::~Defered()
	{
		mDeferedFunction();
	}
}