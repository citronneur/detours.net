using System;
using System.Reflection;
using System.Runtime.InteropServices;
using DetoursNet;

namespace ftrace
{
    public static class Kernel32
    {
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
        public delegate IntPtr CreateFileDelegate(
            IntPtr lpFileName,
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
        [Detours("kernel32.dll", typeof(CreateFileDelegate))]
        public static IntPtr CreateFileW(
            IntPtr lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr SecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
        ) {
            string name = Marshal.PtrToStringUni(lpFileName);
            
            IntPtr result = ((CreateFileDelegate)DelegateStore.GetReal(MethodInfo.GetCurrentMethod()))(lpFileName, dwDesiredAccess, dwShareMode, SecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

            if((int)result == -1)
            {
                Console.WriteLine("CreateFile " + name + " " + "FAILED");
            }
            else
            {
                Console.WriteLine("CreateFile " + name + " " + "SUCCESS");
            }

            return result;
        }

        public delegate int RegOpenKeyExDelegate(IntPtr hKey, IntPtr lpSubKey, int ulOptions, int samDesired, IntPtr phkResult);

        [Detours("advapi.dll", typeof(RegOpenKeyExDelegate))]
        public static int RegOpenKeyExW(IntPtr hKey, IntPtr lpSubKey, int ulOptions, int samDesired, IntPtr phkResult)
        {
            Console.WriteLine("RegOpenKey ");
            return ((RegOpenKeyExDelegate)DelegateStore.GetReal(MethodInfo.GetCurrentMethod()))(hKey, lpSubKey, ulOptions, samDesired, phkResult);
        }
    }
}
