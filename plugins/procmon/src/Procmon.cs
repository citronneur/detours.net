using System;
using System.Reflection;
using System.Runtime.InteropServices;
using DetoursNet;

namespace procmon
{
    public static class Kernel32
    {
        /// <summary>
        /// Just for sync console
        /// </summary>
        private static object sSync = new object();

        /// <summary>
        /// Write message to console in selected color
        /// </summary>
        /// <param name="message">message to write</param>
        /// <param name="color">output color</param>
        private static void WriteColor(ConsoleColor color, string message)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = old;
        }

        [DllImport("kernel32.dll")]
        private extern static bool AllocConsole();

        /// <summary>
        /// Init funciton which allocate a new console
        /// </summary>
        [OnInit]
        public static void OnInit()
        {
            AllocConsole();
        }

        /// <summary>
        /// Create file delegate
        /// </summary>
        /// <param name="lpFileName"></param>
        /// <param name="dwDesiredAccess"></param>
        /// <param name="dwShareMode"></param>
        /// <param name="SecurityAttributes"></param>
        /// <param name="dwCreationDisposition"></param>
        /// <param name="dwFlagsAndAttributes"></param>
        /// <param name="hTemplateFile"></param>
        /// <returns></returns>
        public delegate IntPtr CreateFileWDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr SecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        /// <summary>
        /// Create file hook log all file created
        /// </summary>
        /// <param name="lpFileName"></param>
        /// <param name="dwDesiredAccess"></param>
        /// <param name="dwShareMode"></param>
        /// <param name="SecurityAttributes"></param>
        /// <param name="dwCreationDisposition"></param>
        /// <param name="dwFlagsAndAttributes"></param>
        /// <param name="hTemplateFile"></param>
        /// <returns></returns>
        [Detours("kernel32.dll", typeof(CreateFileWDelegate))]
        public static IntPtr CreateFileW(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr SecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
        )
        {
            IntPtr result = ((CreateFileWDelegate)DelegateStore.GetReal(MethodInfo.GetCurrentMethod()))(lpFileName, dwDesiredAccess, dwShareMode, SecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

            lock(sSync)
            {
                WriteColor(ConsoleColor.Yellow, "CreateFile");
                Console.Write(" {" + lpFileName + "} ");

                if ((int)result == -1)
                {
                    WriteColor(ConsoleColor.Red, "FAILED");
                }
                else
                {
                    WriteColor(ConsoleColor.Green, "SUCCESS");
                }

                Console.WriteLine();
            }
            
            return result;
        }

        /// <summary>
        /// Open a registry key delegate
        /// </summary>
        /// <param name="hKey"></param>
        /// <param name="lpSubKey"></param>
        /// <param name="ulOptions"></param>
        /// <param name="samDesired"></param>
        /// <param name="phkResult"></param>
        /// <returns></returns>
        public delegate int RegOpenKeyExWDelegate(
            IntPtr hKey, 
            [MarshalAs(UnmanagedType.LPWStr)] string lpSubKey, 
            int ulOptions, 
            int samDesired, 
            IntPtr phkResult
        );

        /// <summary>
        /// Hook of open registry key function
        /// </summary>
        /// <param name="hKey"></param>
        /// <param name="lpSubKey"></param>
        /// <param name="ulOptions"></param>
        /// <param name="samDesired"></param>
        /// <param name="phkResult"></param>
        /// <returns></returns>
        [Detours("advapi32.dll", typeof(RegOpenKeyExWDelegate))]
        public static int RegOpenKeyExW(
            IntPtr hKey, 
            string lpSubKey, 
            int ulOptions, 
            int samDesired, 
            IntPtr phkResult
        )
        {
            int result = ((RegOpenKeyExWDelegate)DelegateStore.GetReal(MethodInfo.GetCurrentMethod()))(hKey, lpSubKey, ulOptions, samDesired, phkResult);

            if(lpSubKey != null && lpSubKey.Length > 0)
            {
                lock(sSync)
                {
                    WriteColor(ConsoleColor.Yellow, "RegOpenKey");

                    Console.Write(" {" + lpSubKey + "} ");

                    switch (result)
                    {
                        case 0:
                            WriteColor(ConsoleColor.Green, "SUCCESS");
                            break;
                        case 5:
                            WriteColor(ConsoleColor.Red, "ACCESS DENIED");
                            break;
                        default:
                            WriteColor(ConsoleColor.Red, "UNKNOWN (" + result + ")");
                            break;
                    }

                    Console.WriteLine();
                }
            }
            
            return result;
        }
    }
}
