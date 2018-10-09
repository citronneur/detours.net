# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

# Project

project(detoursnet LANGUAGES C CXX CSharp VERSION 0.1.0)

# Options

option(DETOURSNET_BUILD_TESTS "Build tests" ON)

# Config

set_property(GLOBAL PROPERTY USE_FOLDERS ON)

# define cmake dir
set(DETOURSNET_CMAKE_DIR "${CMAKE_SOURCE_DIR}/cmake")

if (${DETOURSNET_BUILD_TESTS})
    enable_testing()
endif()

SET(EXECUTABLE_OUTPUT_PATH ${CMAKE_BINARY_DIR}/bin)
SET(LIBRARY_OUTPUT_PATH ${CMAKE_BINARY_DIR}/bin)

# Tree processing

add_subdirectory(detours.net)
add_subdirectory(detours)
add_subdirectory(examples)

# Summary

message(STATUS "Configuration summary")
message(STATUS "-- Project name                 : ${PROJECT_NAME}")
message(STATUS "-- Project version              : ${PROJECT_VERSION}")
