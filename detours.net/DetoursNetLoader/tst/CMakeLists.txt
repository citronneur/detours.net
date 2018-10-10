# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DETOURSNETLOADERTST_SRC
	src/DetoursNetLoaderTst.cs
)

add_executable(DetoursNetLoaderTst ${DETOURSNETLOADERTST_SRC})
target_link_libraries(DetoursNetLoaderTst DetoursNetLoaderInitial)
set_target_properties(DetoursNetLoaderTst PROPERTIES FOLDER "tests")