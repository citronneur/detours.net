# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DNLOADER_SRC
	src/dninject.cpp
)

add_library(dninject SHARED ${DNLOADER_SRC})
target_link_libraries(dninject Detours)