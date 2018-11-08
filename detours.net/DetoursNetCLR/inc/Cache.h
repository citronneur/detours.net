#pragma once

#include <Windows.h>
#include <string>
#include <map>
#include <mutex>

namespace pivot
{
	class Cache
	{
	public:
		/*!
		*	@brief	return thread safe instance of cache
		*/
		static Cache& GetInstance();

		/*!
		*	@brief	Add new entry into cache
		*	@param	hModule		source windows module
		*	@param	funcName	name of cached function
		*	@param	pReal		real address of function
		*/
		void update(HMODULE hModule, std::string funcName, PVOID pReal);

		/*!
		*	@brief	Find a real address of function in particular module
		*	@param	hModule		source windows module
		*	@param	pReal		real address of function
		*/
		PVOID find(HMODULE hModule, std::string funcName);

	private:
		/*!
		*	@brief	Cache for all hooked function
		*/
		std::map<std::tuple<HMODULE, std::string>, PVOID> mCache;

		/*!
		*	@brief	Cache access thread safe
		*/
		std::mutex mLock;
	};
}