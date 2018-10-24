using System;
using System.Reflection;
using System.Runtime.InteropServices;
using DetoursNet;

namespace Proxychains
{
    public class WS2_32
    {
        
        public delegate void SleepDelegate(int dwMilliseconds);
        
        [Detours("kernel32.dll", typeof(SleepDelegate))]
        public static void Sleep(int dwMilliseconds)
        {
            Console.WriteLine("Sleep");
            DetoursNet.DetoursNet.Real[MethodInfo.GetCurrentMethod()].DynamicInvoke(new object[] { dwMilliseconds });
        }

        public delegate IntPtr CreateFileDelegate(
                IntPtr lpFileName,
                uint dwDesiredAccess,
                uint dwShareMode,
                IntPtr SecurityAttributes,
                uint dwCreationDisposition,
                uint dwFlagsAndAttributes,
                IntPtr hTemplateFile
                );

        [Detours("kernel32.dll", typeof(CreateFileDelegate))]
        public static IntPtr CreateFileW(
                IntPtr lpFileName,
                uint dwDesiredAccess,
                uint dwShareMode,
                IntPtr SecurityAttributes,
                uint dwCreationDisposition,
                uint dwFlagsAndAttributes,
                IntPtr hTemplateFile
                )
        {
            string name = Marshal.PtrToStringUni(lpFileName);
            Console.WriteLine(name);
            return (IntPtr)DetoursNet.DetoursNet.Real[MethodInfo.GetCurrentMethod()].DynamicInvoke(new object[] { lpFileName, dwDesiredAccess, dwShareMode, SecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile }); ;
        }
    }
}
