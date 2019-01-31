# proxysocks

Proxysocks is a *detours.net* plugin which intend to convert all windows application, that use socket, to be comptaible with a proxy socks.

This plugin just hook the *connect* function from WS2_32.dll and send a SOCKS handshake just after each connect call.