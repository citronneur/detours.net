
add_library(Kernel32Trace SHARED src/Kernel32Trace.cs)
target_link_libraries(Kernel32Trace DetoursNet)
set_target_properties(Kernel32Trace PROPERTIES FOLDER "tools")