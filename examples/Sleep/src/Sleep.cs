using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace slowsleep
{
    public class SlowSleep
    {
        
        public delegate void SleepDelegate(int dwMilliseconds);
        
        [detoursnet.DetoursNet("kernel32.dll", typeof(SleepDelegate))]
        public static void Sleep(int dwMilliseconds)
        {
            Console.WriteLine("Sleep");
            detoursnet.DetoursNet.Real[MethodInfo.GetCurrentMethod()].DynamicInvoke(new object[] { dwMilliseconds });
        }

        public delegate IntPtr CreateFileDelegate(
                string lpFileName,
                uint dwDesiredAccess,
                uint dwShareMode,
                IntPtr SecurityAttributes,
                uint dwCreationDisposition,
                uint dwFlagsAndAttributes,
                IntPtr hTemplateFile
                );

        [detoursnet.DetoursNet("kernel32.dll", typeof(CreateFileDelegate))]
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
            Console.WriteLine(lpFileName);
            return (IntPtr)detoursnet.DetoursNet.Real[MethodInfo.GetCurrentMethod()].DynamicInvoke(new object[] { lpFileName, dwDesiredAccess, dwShareMode, SecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile }); ;
        }
    }
}
