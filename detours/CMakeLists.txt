include(${CMAKE_ROOT}/Modules/ExternalProject.cmake)

# Map the CMake target platform to the value Detours expects in
# DETOURS_TARGET_PROCESSOR (X86 / X64 / ARM64). Prefer the generator platform
# selected with -A (Win32 / x64 / ARM64), then the VS platform name, and finally
# fall back on the pointer size of the toolchain.
if(CMAKE_GENERATOR_PLATFORM)
	set(_detours_platform "${CMAKE_GENERATOR_PLATFORM}")
elseif(CMAKE_VS_PLATFORM_NAME)
	set(_detours_platform "${CMAKE_VS_PLATFORM_NAME}")
elseif(CMAKE_SIZEOF_VOID_P EQUAL 8)
	set(_detours_platform "x64")
else()
	set(_detours_platform "Win32")
endif()

if(_detours_platform STREQUAL "x64" OR _detours_platform STREQUAL "X64")
	set(DETOURS_TARGET_PROCESSOR "X64")
elseif(_detours_platform STREQUAL "ARM64")
	set(DETOURS_TARGET_PROCESSOR "ARM64")
else()
	set(DETOURS_TARGET_PROCESSOR "X86")
endif()

message(STATUS "Detours target processor: ${DETOURS_TARGET_PROCESSOR}")

# build detours
ExternalProject_Add(
	Detours_project
	GIT_REPOSITORY "https://github.com/Microsoft/Detours.git"
	GIT_TAG "main"
	CONFIGURE_COMMAND ""
	BINARY_DIR	"${CMAKE_CURRENT_BINARY_DIR}/Detours_project-prefix/src/Detours_project/src"
	BUILD_COMMAND ${CMAKE_COMMAND} -E env DETOURS_TARGET_PROCESSOR=${DETOURS_TARGET_PROCESSOR} nmake
	INSTALL_COMMAND ""
	UPDATE_COMMAND ""
)

set(DETOURS_INCLUDE_DIR ${CMAKE_CURRENT_BINARY_DIR}/Detours_project-prefix/src/Detours_project/include)
set(DETOURS_LIB_DIR ${CMAKE_CURRENT_BINARY_DIR}/Detours_project-prefix/src/Detours_project/lib.${DETOURS_TARGET_PROCESSOR}/)

# use to fix target_include directories
file(MAKE_DIRECTORY ${DETOURS_INCLUDE_DIR})

add_library(Detours STATIC IMPORTED GLOBAL)
add_dependencies(Detours Detours_project)
set_target_properties(Detours PROPERTIES IMPORTED_LOCATION ${DETOURS_LIB_DIR}/detours.lib)
target_include_directories(Detours INTERFACE ${DETOURS_INCLUDE_DIR})
