
add_library(NtdllTracer SHARED src/NtdllTracer.cs)
target_link_libraries(NtdllTracer DetoursNet)
set_target_properties(NtdllTracer PROPERTIES FOLDER "tools")