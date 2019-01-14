using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace proxychains
{
    /// <summary>
    /// Wrapper around Socket Windows
    /// </summary>
    public static class Socket
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct in_addr
        {
            public byte s_b1;
            public byte s_b2;
            public byte s_b3;
            public byte s_b4;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct sockaddr_in
        {
            public short sin_family;
            public ushort sin_port;
            public in_addr sin_addr;
            private Int64 Zero;
        }

        [DllImport("ws2_32.dll")]
        public static extern int send(IntPtr socket, IntPtr buf, int len, int flags);

        [DllImport("ws2_32.dll")]
        public static extern int recv(IntPtr socket, IntPtr buf, int len, int flags);

        [DllImport("ws2_32.dll")]
        public static extern int WSAGetLastError();

        public static UInt16 Ntohs(UInt16 port)
        {
            return (UInt16)(((port << 8) & 0xFF00) | ((port >> 8) & 0xFF));
        }

        public static UInt16 Hston(UInt16 port)
        {
            return (UInt16)(((port << 8) & 0xFF00) | ((port >> 8) & 0xFF));
        }

        public static void Send(IntPtr socket, byte[] payload)
        {
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(payload.Length);
            Marshal.Copy(payload, 0, unmanagedPointer, payload.Length);

            int result = send(socket, unmanagedPointer, payload.Length, 0);

            Marshal.FreeHGlobal(unmanagedPointer);

            if (result < 0)
            {
                throw new Exception("Unable to send message");
            }
        }

        public static byte[] Recv(IntPtr socket, int bufLength)
        {
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(bufLength);
            int length = recv(socket, unmanagedPointer, bufLength, 0);
            if (length <= 0)
            {
                int code = WSAGetLastError();
                throw new Exception("Invalid Read " + code);
            }
            var result = new byte[length];
            Marshal.Copy(unmanagedPointer, result, 0, length);
            Marshal.FreeHGlobal(unmanagedPointer);
            return result;
        }
    }
}
