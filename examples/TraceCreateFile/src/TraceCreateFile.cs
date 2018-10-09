using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace tracecreatefile
{
    public class TraceCreateFile
    {
        [detoursnet.DetoursNet("kernel32", "GetModuleHandle")]
        internal static int GetModuleHandleHook(IntPtr lpModuleName)
        {
            return 0;
        }
    }
}
