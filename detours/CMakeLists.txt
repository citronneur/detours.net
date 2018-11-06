include(${CMAKE_ROOT}/Modules/ExternalProject.cmake)

# build detours
ExternalProject_Add(
	Detours_project
	GIT_REPOSITORY "https://github.com/Microsoft/Detours.git"
	CONFIGURE_COMMAND ""
	BINARY_DIR	"${CMAKE_CURRENT_BINARY_DIR}/Detours_project-prefix/src/Detours_project/src"
	BUILD_COMMAND ${CMAKE_COMMAND} -E env DETOURS_TARGET_PROCESSOR=X64 nmake
	INSTALL_COMMAND ""
	UPDATE_COMMAND ""
)

set(DETOURS_INCLUDE_DIR ${CMAKE_CURRENT_BINARY_DIR}/Detours_project-prefix/src/Detours_project/include)
set(DETOURS_LIB_DIR ${CMAKE_CURRENT_BINARY_DIR}/Detours_project-prefix/src/Detours_project/lib.X64/)

# use to fix target_include directories
file(MAKE_DIRECTORY ${DETOURS_INCLUDE_DIR})

add_library(Detours STATIC IMPORTED GLOBAL)
add_dependencies(Detours Detours_project)
set_target_properties(Detours PROPERTIES IMPORTED_LOCATION ${DETOURS_LIB_DIR}/detours.lib)
target_include_directories(Detours INTERFACE ${DETOURS_INCLUDE_DIR})
