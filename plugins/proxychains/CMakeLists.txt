
add_library(proxychains SHARED src/Proxychains.cs src/Socks5.cs src/Socket.cs)
target_link_libraries(proxychains DetoursNet)
set_target_properties(proxychains PROPERTIES FOLDER "plugins")