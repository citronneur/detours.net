
add_library(SlowSleep SHARED src/Sleep.cs)
target_link_libraries(SlowSleep DetoursNet)
set_target_properties(SlowSleep PROPERTIES FOLDER "examples")