using System;
using System.Reflection;
using System.Runtime.InteropServices;
using DetoursNet;

namespace Proxychains
{
    public class WS2_32
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

        public delegate int ConnectDelegate(int s, ref sockaddr_in name, int namelen);
        
        [Detours("WS2_32.dll", typeof(ConnectDelegate))]
        public static int connect(int s, ref sockaddr_in name, int namelen)
        {
            Console.WriteLine("connect hooked !!!! family: "+name.sin_family+" port:" + name.sin_port);
            return (int)DetoursNet.DetoursNet.Real[MethodInfo.GetCurrentMethod()].DynamicInvoke(new object[] { s, name, namelen });
        }
    }
}
