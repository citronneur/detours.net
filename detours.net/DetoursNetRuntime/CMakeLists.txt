# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DETOURSNETRUNTIME_INC
	inc/Defered.h
	inc/EnvVar.h
	inc/Utils.h
)

set(DETOURSNETRUNTIME_SRC
	src/Defered.cpp
	src/EnvVar.cpp
	src/Runtime.cpp
	src/Utils.cpp
)

add_executable(DetoursNetRuntime ${DETOURSNETRUNTIME_SRC} ${DETOURSNETRUNTIME_INC})
add_dependencies(DetoursNetRuntime DetoursNetPivot)
add_dependencies(DetoursNetRuntime DetoursNet)
target_link_libraries(DetoursNetRuntime Detours)
target_include_directories(DetoursNetRuntime PRIVATE inc)

add_subdirectory(tst)