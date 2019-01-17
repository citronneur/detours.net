using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace proxysocks
{
    /// <summary>
    /// Wrapper around Socket Windows
    /// </summary>
    public static class Socket
    {
        /// <summary>
        /// IPv4 address type
        /// </summary>
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

        [StructLayout(LayoutKind.Sequential)]
        public struct fd_set
        {
            public uint fd_count;
            [MarshalAs(UnmanagedType.ByValArray)]
            public IntPtr[] fd_array;
        }

        [DllImport("Ws2_32.dll")]
        public static extern int select(int nfds, ref fd_set readfds, ref fd_set writefds, ref fd_set exceptfds, IntPtr timeout);

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

        public enum WaitMode
        {
            Read,
            Write
        }

        /// <summary>
        /// block until state is available
        /// </summary>
        /// <param name="socket">blocking or non blocking socket</param>
        /// <param name="mode">blocking state mode</param>
        public static void SelectSocket(IntPtr socket, WaitMode mode)
        {
            var readFDs = new fd_set();
            var exceptFDs = new fd_set();
            var writeFDs = new fd_set();

            switch(mode)
            {
            case WaitMode.Read:
                readFDs.fd_count = 1;
                readFDs.fd_array = new IntPtr[] { socket };
                break;
            case WaitMode.Write:
                writeFDs.fd_count = 1;
                writeFDs.fd_array = new IntPtr[] { socket };
                break;
            };

            int result = select(0, ref readFDs, ref writeFDs, ref exceptFDs, IntPtr.Zero);
            if(result < 0)
            {
                throw new Exception("Invalid select call");
            }
        }

        /// <summary>
        /// Wrapper around send function of ws2_32.dll module
        /// </summary>
        /// <param name="socket">connected socket</param>
        /// <param name="payload">data to send</param>
        public static void Send(IntPtr socket, byte[] payload)
        {
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(payload.Length);
            Marshal.Copy(payload, 0, unmanagedPointer, payload.Length);
            int sendLength = 0;
            while(sendLength < payload.Length)
            {
                int result = send(socket, unmanagedPointer + sendLength, payload.Length - sendLength, 0);
                if (result < 0)
                {
                    Marshal.FreeHGlobal(unmanagedPointer);
                    throw new Exception("Unable to send message " + WSAGetLastError());
                }
                sendLength += result;
            }
            Marshal.FreeHGlobal(unmanagedPointer);
        }

        /// <summary>
        /// Send a structure type to network
        /// </summary>
        /// <typeparam name="T">type of param</typeparam>
        /// <param name="socket">socket use to send structure</param>
        /// <param name="payload">structure treat as payload</param>
        public static void Send<T>(IntPtr socket, T payload) where T : struct
        {
            int size = Marshal.SizeOf(payload);
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(payload, unmanagedPointer, false);

            int sendLength = 0;

            while(sendLength < size)
            {
                int result = send(socket, unmanagedPointer + sendLength, size - sendLength, 0);
                if (result < 0)
                {
                    Marshal.FreeHGlobal(unmanagedPointer);
                    throw new Exception("Unable to send message " + WSAGetLastError());
                }
                sendLength += result;
            }
            Marshal.FreeHGlobal(unmanagedPointer);
        }

        /// <summary>
        /// Recv data from a particular socket
        /// </summary>
        /// <param name="socket">connected socket</param>
        /// <param name="readLength">length to read</param>
        /// <returns></returns>
        public static byte[] Recv(IntPtr socket, int readLength)
        {
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(readLength);
            int recvLength = 0;
            while(recvLength < readLength)
            {
                SelectSocket(socket, WaitMode.Read);
                int length = recv(socket, unmanagedPointer + recvLength, readLength - recvLength, 0);
                if (length <= 0)
                {
                    Marshal.FreeHGlobal(unmanagedPointer);
                    throw new Exception("Invalid Read " + WSAGetLastError());
                }
                recvLength += length;
            }
            
            var result = new byte[readLength];
            Marshal.Copy(unmanagedPointer, result, 0, readLength);
            Marshal.FreeHGlobal(unmanagedPointer);
            return result;
        }

        /// <summary>
        /// Try to receive and convert a particular structure type from network
        /// </summary>
        /// <typeparam name="T">Type of structure</typeparam>
        /// <param name="socket">connected socket</param>
        /// <returns>Expected Structure mapped</returns>
        public static T Recv<T>(IntPtr socket) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(size);

            int recvLength = 0;
            while(recvLength < size)
            {
                SelectSocket(socket, WaitMode.Read);
                int length = recv(socket, unmanagedPointer + recvLength, size - recvLength, 0);
                if (length <= 0)
                {
                    Marshal.FreeHGlobal(unmanagedPointer);
                    throw new Exception("Invalid Read " + WSAGetLastError());
                }
                recvLength += length;
            }
            
            var result = (T)Marshal.PtrToStructure(unmanagedPointer, typeof(T));
            
            return result;
        }
    }
}
