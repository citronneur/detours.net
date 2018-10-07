cmake_policy(SET CMP0012 NEW)


set(IL_FILE_PATCH "${CMAKE_CURRENT_BINARY_DIR}/Debug/detoursnet.patch.il")

message(STATUS "Patch intermediate language")
file(STRINGS "${CMAKE_CURRENT_BINARY_DIR}/Debug/detoursnet_initial.il" IL_FILE_CONTENTS)
file(WRITE ${IL_FILE_PATCH} "")
foreach(LINE ${IL_FILE_CONTENTS})
	if(${LINE} MATCHES "^.corflags 0x00000001 *")
		file(APPEND ${IL_FILE_PATCH} ".corflags 0x00000000\n.vtfixup [1] int64 fromunmanaged at VT_01\n.data VT_01 = int64(0)\n")
	elseif(${LINE} STREQUAL "serv")		# noise generate from console
	elseif(${LINE} STREQUAL "s.")		# noise generate from console
	elseif(${LINE} STREQUAL	"  .method public hidebysig static void  Load() cil managed")
		file(APPEND ${IL_FILE_PATCH} "${LINE}\n")
		set(LOAD_HEADER true)
	elseif(${LINE} MATCHES " *{$")
		file(APPEND ${IL_FILE_PATCH} "${LINE}\n")
		if(${LOAD_HEADER})
			file(APPEND ${IL_FILE_PATCH} ".export [1] as Load\n.vtentry 1 : 1\n")
			set(LOAD_HEADER false)
		endif()
	else()
		file(APPEND ${IL_FILE_PATCH} "${LINE}\n")
	endif()
endforeach()