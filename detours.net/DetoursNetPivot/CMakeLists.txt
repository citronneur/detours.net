# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DETOURSNETPIVOT_SRC
	src/DetoursNetPivot.cpp
)

add_library(DetoursNetPivot SHARED ${DETOURSNETPIVOT_SRC})
target_link_libraries(DetoursNetPivot Detours DetoursNetLoader mscoree)