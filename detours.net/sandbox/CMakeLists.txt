# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DNLOADER_SRC
	src/sandbox.cpp
)

add_executable(sandbox ${DNLOADER_SRC})
target_link_libraries(sandbox Detours)