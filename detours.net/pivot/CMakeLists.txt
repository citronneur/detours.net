# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DNLOADER_SRC
	src/pivot.cpp
)

add_library(pivot SHARED ${DNLOADER_SRC})
target_link_libraries(pivot Detours DetoursNet)