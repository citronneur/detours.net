# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DETOURSNET_SRC
	src/DetoursNet.cs
)

add_library(DetoursNet SHARED ${DETOURSNET_SRC})