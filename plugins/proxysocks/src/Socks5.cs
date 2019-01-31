using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace proxysocks
{
    /// <summary>
    /// Implement SOCKS5 client protocol
    /// </summary>
    public class Socks5
    {
        private static readonly byte SOCKS_V5 = 0x05;
        /// <summary>
        /// Authentication header for SOCKSv5 protocol
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct AuthHeader
        {
            public byte version;
            public byte nbAuthMethod;
            [MarshalAs(UnmanagedType.ByValArray)]
            public byte[] method;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AuthResponse
        {
            public byte version;
            public byte returnCode;
        }

        private enum ConnectionType : byte
        {
            TcpIpStream = 0x01,
            TcpIpPortBinding = 0x02,
            UdpPort = 0x03
        }

        private enum AddressType : byte
        {
            IPv4 = 0x01,
            DomainName = 0x03,
            IPv6 = 0x04
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IPv4ConnectRequest
        {
            public byte version;
            public ConnectionType type;
            public byte reserved;
            public AddressType addressType;
            public Socket.in_addr ip;
            public ushort port;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IPv4ConnectResponse
        {
            public byte version;
            public byte status;
            public byte reserved;
            public AddressType addressType;
            public Socket.in_addr ip;
            public ushort port;
        }

        /// <summary>
        /// Basic SOCKS5 authenticate with None authentication
        /// </summary>
        /// <param name="socket">Socket use for socks</param>
        static void Authenticate(IntPtr socket)
        {
            // Send header
            Socket.Send(socket, new AuthHeader()
            {
                version = SOCKS_V5,
                nbAuthMethod = 0x01,
                method = new byte[] { 0x00 } // None auth method selected
            });

            var response = Socket.Recv<AuthResponse>(socket);

            if(response.version != SOCKS_V5)
            {
                throw new Exception("Invalid SOCKS version");
            }

            if(response.returnCode != 0x00)
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
            // in SOCKS5 there is an auth step before connection
            Authenticate(socket);
            Socket.Send(socket, new IPv4ConnectRequest()
            {
                version = SOCKS_V5,
                type = ConnectionType.TcpIpStream,
                reserved = 0x00,
                addressType = AddressType.IPv4,
                ip = ip,
                port = Socket.Hston(port)
            });
            
            // Wait for an IPv4 connect response
            var response = Socket.Recv<IPv4ConnectResponse>(socket);

            if(response.version != SOCKS_V5)
            {
                throw new Exception("Invalid version protocol");
            }

            // All other code is error code
            if(response.status != 0)
            {
                throw new Exception("SOCKS : server error : " + response.status);
            }
        }
    }
}
