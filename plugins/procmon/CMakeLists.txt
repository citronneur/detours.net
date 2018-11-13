
add_library(procmon SHARED src/Procmon.cs)
target_link_libraries(procmon DetoursNet)
set_target_properties(procmon PROPERTIES FOLDER "tools")