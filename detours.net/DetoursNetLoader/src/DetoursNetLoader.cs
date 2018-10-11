using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace detoursnetloader
{
    public class DetoursNetLoader
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleA", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("Detours.dll")]
        internal static extern long DetourAttach(ref IntPtr a, IntPtr b);

        [DllImport("Detours.dll")]
        internal static extern long DetourUpdateThread(IntPtr a);

        [DllImport("Detours.dll")]
        internal static extern long DetourTransactionBegin();

        [DllImport("Detours.dll")]
        internal static extern long DetourTransactionCommit();


        /// <summary>
        /// Main entry point of loader
        /// </summary>
        public static void DetoursNetLoader_Start()
        {

            Assembly plugin = Assembly.LoadFile("c:\\dev\\build_x64\\bin\\Debug\\SlowSleep.dll");
            MethodInfo method = plugin.GetType("slowsleep.SlowSleep").GetMethod("Sleep");
            var attribute = (detoursnet.DetoursNetAttribute)method.GetCustomAttributes(typeof(detoursnet.DetoursNetAttribute), false)[0];


            attribute.Mine = Delegate.CreateDelegate(attribute.DelegateType, method);
                
            IntPtr kernel32 = GetModuleHandle(attribute.Module);
            IntPtr real = GetProcAddress(kernel32, "Sleep");

            DetourTransactionBegin();
            DetourUpdateThread(GetCurrentThread());
            DetourAttach(ref real, Marshal.GetFunctionPointerForDelegate(attribute.Mine));
            DetourTransactionCommit();

            attribute.Real = Marshal.GetDelegateForFunctionPointer(real, attribute.DelegateType);
        }
    }
}
