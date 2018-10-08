# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DETOURSNET_SRC
	src/DetoursNet.cs
)

add_library(detoursnet_initial SHARED ${DETOURSNET_SRC})

add_custom_target(detoursnet_export ALL
	COMMAND ildasm /OUT=${CMAKE_CURRENT_BINARY_DIR}/detoursnet_initial.il /UTF8 ${LIBRARY_OUTPUT_PATH}/Debug/detoursnet_initial.dll # Generate intermediate languages
	COMMAND cmake -P ${CMAKE_CURRENT_SOURCE_DIR}/export_load_function.cmake # Patch the intermediate language to export load function
	COMMAND ilasm /PE64 /DLL /X64 /OUTPUT=${LIBRARY_OUTPUT_PATH}/Debug/detoursnet.dll ${CMAKE_CURRENT_BINARY_DIR}/detoursnet.patch.il # rebuild library with export
	COMMAND lib /DEF:${CMAKE_CURRENT_SOURCE_DIR}/detoursnet.def /OUT:${LIBRARY_OUTPUT_PATH}/detoursnet.lib /MACHINE:X64
	DEPENDS ${LIBRARY_OUTPUT_PATH}/Debug/detoursnet_initial.dll
	OUTPUT ${LIBRARY_OUTPUT_PATH}/Debug/detoursnet.dll
)

add_library(DetoursNet STATIC IMPORTED GLOBAL)
add_dependencies(DetoursNet detoursnet_export)
set_target_properties(DetoursNet PROPERTIES IMPORTED_LOCATION ${LIBRARY_OUTPUT_PATH}/detoursnet.lib)