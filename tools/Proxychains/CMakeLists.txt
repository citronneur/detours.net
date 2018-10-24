
add_library(Proxychains SHARED src/Proxychains.cs)
target_link_libraries(Proxychains DetoursNet)
set_target_properties(Proxychains PROPERTIES FOLDER "tools")