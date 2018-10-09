# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(TRACECREATEFILE_SRC
	src/TraceCreateFile.cs
)

add_library(TraceCreateFile SHARED ${TRACECREATEFILE_SRC})
target_link_libraries(TraceCreateFile DetoursNet)
set_target_properties(TraceCreateFile PROPERTIES FOLDER "examples")