# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DETOURSDLL_SRC
	inc/DetoursDll.h
	src/DetoursDll.cpp
	src/DetoursDll.def
)

add_library(DetoursDll SHARED ${DETOURSDLL_SRC})
add_compile_definitions(UNICODE)
target_link_libraries(DetoursDll Detours)
target_include_directories(DetoursDll INTERFACE inc)