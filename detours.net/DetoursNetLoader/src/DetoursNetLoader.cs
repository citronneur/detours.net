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

        delegate int GetCurrentProcessIdDelegate();
        /// <summary>
        /// Main entry point of loader
        /// </summary>
        public static void DetoursNetLoader_Start()
        {

            Assembly plugin = Assembly.LoadFile("c:\\dev\\build_x64\\bin\\Debug\\TraceCreateFile.dll");
            MethodInfo method = plugin.GetType("tracecreatefile.TraceCreateFile").GetMethod("MyGetCurrentProcessId");
            
            Delegate methodDelegate = Delegate.CreateDelegate(typeof(GetCurrentProcessIdDelegate), method);
            
            IntPtr a = Marshal.GetFunctionPointerForDelegate(methodDelegate);
            IntPtr kernel32 = GetModuleHandle("kernel32");
            IntPtr pGetCurrentProcessId = GetProcAddress(kernel32, "GetCurrentProcessId");
            DetourTransactionBegin();
            DetourUpdateThread(GetCurrentThread());
            DetourAttach(ref pGetCurrentProcessId, a);
            long ret =  DetourTransactionCommit();
        }
    }
}
