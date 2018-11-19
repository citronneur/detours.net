# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DETOURSNETRUNTIME_INC
	inc/DetoursNetRuntimeDefered.h
	inc/DetoursNetRuntimeEnvVar.h
)

set(DETOURSNETRUNTIME_SRC
	src/DetoursNetRuntimeDefered.cpp
	src/DetoursNetRuntimeEnvVar.cpp
	src/DetoursNetRuntime.cpp
)

add_executable(DetoursNetRuntime ${DETOURSNETRUNTIME_SRC} ${DETOURSNETRUNTIME_INC})
add_dependencies(DetoursNetRuntime DetoursNetCLR)
add_dependencies(DetoursNetRuntime DetoursNet)
target_link_libraries(DetoursNetRuntime Detours)
target_include_directories(DetoursNetRuntime PRIVATE inc)
