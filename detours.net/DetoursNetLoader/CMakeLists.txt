# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DETOURSNETLOADER_SRC
	src/DetoursNetLoader.cs
)

add_library(DetoursNetLoaderInitial SHARED ${DETOURSNETLOADER_SRC})

add_custom_target(DetoursNetLoaderExport ALL
	COMMAND ildasm /OUT=${CMAKE_CURRENT_BINARY_DIR}/DetoursNetLoaderInitial.il /UTF8 ${LIBRARY_OUTPUT_PATH}/Debug/DetoursNetLoaderInitial.dll # Generate intermediate languages
	COMMAND cmake -P ${CMAKE_CURRENT_SOURCE_DIR}/export_load_function.cmake # Patch the intermediate language to export load function
	COMMAND ilasm /PE64 /DLL /X64 /OUTPUT=${LIBRARY_OUTPUT_PATH}/Debug/DetoursNetLoader.dll ${CMAKE_CURRENT_BINARY_DIR}/DetoursNetLoader.patch.il # rebuild library with export
	COMMAND lib /DEF:${CMAKE_CURRENT_SOURCE_DIR}/DetoursNetLoader.def /OUT:${LIBRARY_OUTPUT_PATH}/DetoursNetLoader.lib /MACHINE:X64
	DEPENDS DetoursNetLoaderInitial
	OUTPUT ${LIBRARY_OUTPUT_PATH}/Debug/DetoursNetLoader.dll
)

add_library(DetoursNetLoader STATIC IMPORTED GLOBAL)
add_dependencies(DetoursNetLoader DetoursNetLoaderExport)
set_target_properties(DetoursNetLoader PROPERTIES IMPORTED_LOCATION ${LIBRARY_OUTPUT_PATH}/DetoursNetLoader.lib)

add_subdirectory(tst)