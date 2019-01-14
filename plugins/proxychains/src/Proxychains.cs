using System;
using System.Reflection;
using System.Runtime.InteropServices;
using DetoursNet;
using proxychains;

namespace Proxychains
{
    public class Hooks
    {
        /// <summary>
        /// Delegate of connect function from WS2_32.dll
        /// </summary>
        /// <param name="s">native socket</param>
        /// <param name="name">struct dest connection infos</param>
        /// <param name="namelen">sizeof name</param>
        /// <returns></returns>
        public delegate int ConnectDelegate(IntPtr s, ref Socket.sockaddr_in name, int namelen);

        /// <summary>
        /// Hook of native connect function from WS2_32.dll
        /// </summary>
        /// <param name="s">native socket</param>
        /// <param name="name">struct dest connection infos</param>
        /// <param name="namelen">sizeof name</param>
        /// <returns></returns>
        [Detours("WS2_32.dll", typeof(ConnectDelegate))]
        public static int connect(IntPtr s, ref Socket.sockaddr_in name, int namelen)
        {
            Console.WriteLine("connect hooked !!!! family: " + name.sin_addr.s_b1 + "." + name.sin_addr.s_b2 + "." + name.sin_addr.s_b3 + "." + name.sin_addr.s_b4 + " port:" + (UInt16)(name.sin_port << 8 | name.sin_port >> 8));
            var connectCallback = (ConnectDelegate)(DelegateStore.GetReal(MethodInfo.GetCurrentMethod()));

            var socks = new Socket.sockaddr_in()
            {
                sin_addr = new Socket.in_addr()
                {
                    s_b1 = 127,
                    s_b2 = 0,
                    s_b3 = 0,
                    s_b4 = 1
                },
                sin_port = Socket.Hston(9090),
                sin_family = name.sin_family
            };

            var result = connectCallback(s, ref socks, namelen);
            if(result != 0)
            {
                return result;
            }

            // Authenticate client  
            try
            {
                Socks5.Connect(s, name.sin_addr, Socket.Ntohs(name.sin_port));
            }
            catch(Exception e)
            {
                Console.WriteLine("Error during SOCKS connection : " + e);
                return -1;
            }

            return 0;
        }
    }
}
