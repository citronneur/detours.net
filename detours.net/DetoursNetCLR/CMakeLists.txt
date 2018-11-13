# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DETOURSNETCLR_SRC
	src/DetoursNetCLR.cpp
	inc/DetoursNetCLRError.h
	inc/DetoursNetCLRPInvokeCache.h
	src/DetoursNetCLRPInvokeCache.cpp
)

add_library(DetoursNetCLR SHARED ${DETOURSNETCLR_SRC})
add_compile_definitions(UNICODE)
target_link_libraries(DetoursNetCLR DetoursDll DetoursNet mscoree)