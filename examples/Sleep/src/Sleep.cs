using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace slowsleep
{
    public class SlowSleep
    {
        
        public delegate void SleepDelegate(int dwMilliseconds);
        internal static SleepDelegate RealSleep;
        
        [detoursnet.DetoursNet("kernel32.dll", typeof(SleepDelegate))]
        public static void Sleep(int dwMilliseconds)
        {
            var attribute = (detoursnet.DetoursNetAttribute)typeof(SlowSleep).GetMethod("Sleep").GetCustomAttributes(typeof(detoursnet.DetoursNetAttribute), false)[0];
            Console.WriteLine("Hooked!!!!!   hhh" + attribute.Module);

            attribute.Real.DynamicInvoke(new object[] { dwMilliseconds });
        }
    }
}
