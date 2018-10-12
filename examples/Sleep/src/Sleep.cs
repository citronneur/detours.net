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
           detoursnet.DetoursNet.Real[MethodInfo.GetCurrentMethod()].DynamicInvoke(new object[] { dwMilliseconds });
        }

        public delegate IntPtr HeapAllocDelegate(IntPtr handle, int dwFlags, int dwBytes);

        [detoursnet.DetoursNet("kernel32.dll", typeof(HeapAllocDelegate))]
        public static IntPtr HeapAlloc(IntPtr handle, int dwFlags, int dwBytes)
        {
            return (IntPtr)detoursnet.DetoursNet.Real[MethodInfo.GetCurrentMethod()].DynamicInvoke(new object[] { handle, dwFlags, dwBytes });
        }
    }
}
