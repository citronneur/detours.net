# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DETOURSNET_SRC
	src/DetoursNet.cs
)

add_library(detoursnet_initial SHARED ${DETOURSNET_SRC})

add_custom_target(detoursnet_export ALL
	COMMAND ildasm /OUT=${CMAKE_CURRENT_BINARY_DIR}/Debug/detoursnet_initial.il /UTF8 ${CMAKE_CURRENT_BINARY_DIR}/Debug/detoursnet_initial.dll # Generate intermediate languages
	COMMAND cmake -P ${CMAKE_CURRENT_SOURCE_DIR}/export_load_function.cmake # Patch the intermediate language to export load function
	COMMAND ilasm /PE64 /DLL /X64 /OUTPUT=${CMAKE_CURRENT_BINARY_DIR}/Debug/detoursnet.dll ${CMAKE_CURRENT_BINARY_DIR}/Debug/detoursnet.patch.il # rebuild library with export
	COMMAND lib /DEF:${CMAKE_CURRENT_SOURCE_DIR}/detoursnet.def /OUT:${CMAKE_CURRENT_BINARY_DIR}/Debug/detoursnet.lib /MACHINE:X64
	DEPENDS ${CMAKE_CURRENT_BINARY_DIR}/Debug/detoursnet_initial.dll
	OUTPUT ${CMAKE_CURRENT_BINARY_DIR}/Debug/detoursnet.dll
)

add_library(DetoursNet STATIC IMPORTED GLOBAL)
add_dependencies(DetoursNet detoursnet_export)
set_target_properties(DetoursNet PROPERTIES IMPORTED_LOCATION ${CMAKE_CURRENT_BINARY_DIR}/Debug/detoursnet.lib)