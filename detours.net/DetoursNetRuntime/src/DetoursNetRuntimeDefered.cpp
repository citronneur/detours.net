#include "DetoursNetRuntimeDefered.h"

namespace detoursnetruntime
{
	Defered::Defered(const std::function<void(void)>& deferedFunction)
		: mDeferedFunction{ deferedFunction }
	{}

	Defered::Defered(std::function<void(void)>&& deferedFunction)
		: mDeferedFunction{ std::move(deferedFunction) }
	{}

	Defered::~Defered()
	{
		mDeferedFunction();
	}
}