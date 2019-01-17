using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using DetoursNet;
using proxysocks;

namespace proxysocks
{
    public static class Proxy
    {
        /// <summary>
        /// Delegate of connect function from WS2_32.dll
        /// </summary>
        /// <param name="s">native socket</param>
        /// <param name="name">struct dest connection infos</param>
        /// <param name="namelen">sizeof name</param>
        /// <returns></returns>
        public delegate int ConnectDelegate(IntPtr s, ref Socket.sockaddr_in name, int namelen);

        private class ProxyConfig
        {
            public Socket.in_addr proxyAddress;
            public ushort proxyPort;
        }

        private static ProxyConfig Config = null;

        /// <summary>
        /// Compute proxy config from env var
        /// </summary>
        /// <returns>Config from env var</returns>
        private static ProxyConfig ParseConfig()
        {
            if (Config == null)
            {
                // retrieve Proxy address
                string[] proxyConfig = System.Environment.GetEnvironmentVariable("SOCKS5_PROXY").Split(':');

                if (proxyConfig.Length != 2)
                {
                    throw new Exception("Bad IP format for SOCKS5_PROXY");
                }

                byte[] ip = proxyConfig[0].Split('.').Select(x => Convert.ToByte(x)).ToArray();
                if (ip.Length != 4)
                {
                    throw new Exception("Bad IP format for SOCKS5_PROXY");
                }

                Config = new ProxyConfig()
                {
                    proxyAddress = new Socket.in_addr()
                    {
                        s_b1 = ip[0],
                        s_b2 = ip[1],
                        s_b3 = ip[2],
                        s_b4 = ip[3]
                    },
                    proxyPort = Convert.ToUInt16(proxyConfig[1])
                };
            }
            return Config;
        }

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
            // retrieve real connect function
            var realConnect = (ConnectDelegate)(DelegateStore.GetReal(MethodInfo.GetCurrentMethod()));

            var proxyConfig = ParseConfig();

            var socks = new Socket.sockaddr_in()
            {
                sin_addr = proxyConfig.proxyAddress,
                sin_port = Socket.Hston(proxyConfig.proxyPort),
                sin_family = name.sin_family
            };

            var result = realConnect(s, ref socks, namelen);

            if(result != 0)
            {
                // non blocking socket
                if(Socket.WSAGetLastError() == 10035)
                {
                    Socket.SelectSocket(s, Socket.WaitMode.Write);
                }
                else
                {
                    return result;
                }
            }

            // Connect to socks server
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
