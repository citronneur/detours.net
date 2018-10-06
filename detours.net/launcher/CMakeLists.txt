# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DNLOADER_SRC
	src/dnlauncher.cpp
)

add_executable(dnlauncher ${DNLOADER_SRC})
target_link_libraries(dnlauncher Detours)