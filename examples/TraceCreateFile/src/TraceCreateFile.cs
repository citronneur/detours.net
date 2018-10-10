using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace tracecreatefile
{
    public class TraceCreateFile
    {
        
        public delegate int GetCurrentProcessIdDelegate();
        
        [detoursnet.DetoursNet("kernel32.dll", "GetCurrentProcessId", typeof(GetCurrentProcessIdDelegate))]
        public static int MyGetCurrentProcessId()
        {
            return 1234;
        }
    }
}
