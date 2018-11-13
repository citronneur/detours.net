#include "../inc/DetoursNetCLRPInvokeCache.h"


namespace detoursnetclr
{
	PInvokeCache& PInvokeCache::GetInstance()
	{
		static PInvokeCache cache;
		return cache;
	}

	void PInvokeCache::update(HMODULE hModule, std::string funcName, PVOID pReal)
	{
		std::lock_guard<std::mutex> guard(mLock);
		mCache[std::make_tuple(hModule, funcName)] = pReal;
	}

	PVOID PInvokeCache::find(HMODULE hModule, std::string funcName)
	{
		std::lock_guard<std::mutex> guard(mLock);
		auto real = mCache.find(std::make_tuple(hModule, funcName));
		if (real == mCache.end())
		{
			return nullptr;
		}

		return real->second;
	}
}
