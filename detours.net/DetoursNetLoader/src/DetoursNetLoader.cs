using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;

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
        public static int DetoursNetLoader_Start(string arguments)
        {

            Assembly assembly = Assembly.LoadFile("c:\\dev\\build_x64\\bin\\Debug\\SlowSleep.dll");

            var methods = assembly.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(detoursnet.DetoursNetAttribute), false).Length > 0)
                .ToArray();

            foreach (var method in methods)
            {
                var attribute = (detoursnet.DetoursNetAttribute)method.GetCustomAttributes(typeof(detoursnet.DetoursNetAttribute), false)[0];
                detoursnet.DetoursNet.Mine[method] = Delegate.CreateDelegate(attribute.DelegateType, method);

                IntPtr module = GetModuleHandle(attribute.Module);
                if (module == IntPtr.Zero)
                    continue;

                IntPtr real = GetProcAddress(module, method.Name);
                if (real == IntPtr.Zero)
                    continue;

                DetourTransactionBegin();
                DetourUpdateThread(GetCurrentThread());
                DetourAttach(ref real, Marshal.GetFunctionPointerForDelegate(detoursnet.DetoursNet.Mine[method]));
                DetourTransactionCommit();

                detoursnet.DetoursNet.Real[method] = Marshal.GetDelegateForFunctionPointer(real, attribute.DelegateType);
            }

            return 0;
            
        }
    }
}
 