#include "../inc/Cache.h"


namespace pivot
{
	Cache& Cache::GetInstance()
	{
		static Cache cache;
		return cache;
	}

	void Cache::update(HMODULE hModule, std::string funcName, PVOID pReal)
	{
		std::lock_guard<std::mutex> guard(mLock);
		mCache[std::make_tuple(hModule, funcName)] = pReal;
	}

	PVOID Cache::find(HMODULE hModule, std::string funcName)
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
