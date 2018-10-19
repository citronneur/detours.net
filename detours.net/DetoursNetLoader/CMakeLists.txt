# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DETOURSNETLOADER_SRC
	src/DetoursNetLoader.cs
)

add_library(DetoursNetLoader SHARED ${DETOURSNETLOADER_SRC})
target_link_libraries(DetoursNetLoader DetoursNet)

add_subdirectory(tst)