using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace proxychains
{
    /// <summary>
    /// Implement SOCKS5 client protocol
    /// </summary>
    public class Socks5
    {
        /// <summary>
        /// Basic SOCKS5 authenticate with None authentication
        /// </summary>
        /// <param name="socket">Socket use for socks</param>
        static void Authenticate(IntPtr socket)
        {
            Socket.Send(socket, new byte[]
            {
                0x05,
                0x01,
                0x00    // none auth method
            });

            var response = Socket.Recv(socket, 2);

            if(response[0] != 0x05)
            {
                throw new Exception("Invalid SOCKS version");
            }

            if(response[1] != 0x00)
            {
                throw new Exception("Invalid Auth method selected");
            }
        }

        /// <summary>
        /// Create a connection with a SOCKS server
        /// </summary>
        /// <param name="socket">socket</param>
        /// <param name="ip">dest ip over SOCKS</param>
        /// <param name="port">dest port over SOCKS</param>
        public static void Connect(IntPtr socket, Socket.in_addr ip, UInt16 port)
        {
            Authenticate(socket);

            Socket.Send(socket, new byte[]
            {
                0x05,
                0x01,   // TCP connection
                0X00,
                0x01,   // IPv4
                ip.s_b1,
                ip.s_b2,
                ip.s_b3,
                ip.s_b4,
                (byte)(port >> 8 & 0xFF),
                (byte)(port & 0xFF)
            });

            // Always wait for 10 bytes
            var response = Socket.Recv(socket, 10);

            if(response[0] != 0x05)
            {
                throw new Exception("Invalid version protocol");
            }

            // All other code is error code
            if(response[1] != 0)
            {
                throw new Exception("SOCKS : server error : " + response[1]);
            }
        }
    }
}
