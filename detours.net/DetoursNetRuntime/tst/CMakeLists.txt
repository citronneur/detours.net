# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

add_executable(Sleep Sleep.cpp)
set_target_properties(Sleep PROPERTIES FOLDER "tests")