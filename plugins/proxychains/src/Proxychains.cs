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

        [DllImport("ws2_32.dll")]
        public static extern int send(IntPtr Socket, IntPtr buff, int len, int flags);

        /// <summary>
        /// Delegate of connect function from WS2_32.dll
        /// </summary>
        /// <param name="s">native socket</param>
        /// <param name="name">struct dest connection infos</param>
        /// <param name="namelen">sizeof name</param>
        /// <returns></returns>
        public delegate int ConnectDelegate(IntPtr s, ref sockaddr_in name, int namelen);

        /// <summary>
        /// Hook of native connect function from WS2_32.dll
        /// </summary>
        /// <param name="s">native socket</param>
        /// <param name="name">struct dest connection infos</param>
        /// <param name="namelen">sizeof name</param>
        /// <returns></returns>
        [Detours("WS2_32.dll", typeof(ConnectDelegate))]
        public static int connect(IntPtr s, ref sockaddr_in name, int namelen)
        {
            Console.WriteLine("connect hooked !!!! family: " + name.sin_addr.s_b1 + "." + name.sin_addr.s_b2 + "." + name.sin_addr.s_b3 + "." + name.sin_addr.s_b4 + " port:" + (UInt16)(name.sin_port << 8 | name.sin_port >> 8));
            var connectCallback = (ConnectDelegate)(DelegateStore.GetReal(MethodInfo.GetCurrentMethod()));

            var socks = new sockaddr_in()
            {
                sin_addr = new in_addr()
                {
                    s_b1 = 192,
                    s_b2 = 168,
                    s_b3 = 0,
                    s_b4 = 11
                },
                sin_port = (UInt16)((9050 << 8 & 0xFF00) | (9050 >> 8 & 0xFF)),
                sin_family = name.sin_family
            };

            var result = connectCallback(s, ref socks, namelen);
            if(result != 0)
            {
                return result;
            }

            //send(s, )

            return 0;
        }
    }
}
