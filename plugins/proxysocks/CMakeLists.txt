
add_library(proxysocks SHARED src/Proxy.cs src/Socks5.cs src/Socket.cs)
target_link_libraries(proxysocks DetoursNet)
set_target_properties(proxysocks PROPERTIES FOLDER "plugins")