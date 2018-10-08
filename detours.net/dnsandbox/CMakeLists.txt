# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(DNSANDBOX_INC
	inc/defered.h
)

set(DNSANDBOX_SRC
	src/dnsandbox.cpp
)

add_executable(DNSandbox ${DNSANDBOX_SRC})
target_link_libraries(DNSandbox Detours)