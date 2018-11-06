using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;

namespace DetoursNet
{
    public class Loader
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpModuleName);

        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("DetoursDll.dll")]
        internal static extern long DetourAttach(ref IntPtr a, IntPtr b);

        [DllImport("DetoursDll.dll")]
        internal static extern long DetourUpdateThread(IntPtr a);

        [DllImport("DetoursDll.dll")]
        internal static extern long DetourTransactionBegin();

        [DllImport("DetoursDll.dll")]
        internal static extern long DetourTransactionCommit();

        [DllImport("DetoursDll.dll")]
        internal static extern bool DetoursPatchIAT(IntPtr hModule, IntPtr import, IntPtr real);

        [DllImport("DetoursNetPivot.dll", CharSet=CharSet.Ansi)]
        internal static extern void DetoursPivotSetGetProcAddressCache(string procName, IntPtr real);

        /// <summary>
        /// Main entry point of loader
        /// </summary>
        public static int Start(string arguments)
        {
            string assemblyName = System.Environment.GetEnvironmentVariable("DETOURSNET_ASSEMBLY_PLUGIN");
            //Console.WriteLine("[+] Load assembly plugin " + assemblyName);

            Assembly assembly = Assembly.LoadFrom(assemblyName);

            var methods = assembly.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(DetoursAttribute), false).Length > 0)
                .ToArray();

            foreach (var method in methods)
            {
                var attribute = (DetoursAttribute)method.GetCustomAttributes(typeof(DetoursAttribute), false)[0];

                DelegateStore.Mine[method] = Delegate.CreateDelegate(attribute.DelegateType, method);

                IntPtr module = LoadLibrary(attribute.Module);
                if (module == IntPtr.Zero)
                {
                    continue;
                }

                IntPtr real = GetProcAddress(module, method.Name);
                if (real == IntPtr.Zero)
                {
                    continue;
                }

                // record pointer
                IntPtr import = real;

                DetourTransactionBegin();
                DetourUpdateThread(GetCurrentThread());
                DetourAttach(ref real, Marshal.GetFunctionPointerForDelegate(DelegateStore.Mine[method]));
                DetourTransactionCommit();
                DetoursPivotSetGetProcAddressCache(method.Name, real);

                IntPtr hClr = GetModuleHandle("clr.dll");
                if(hClr == IntPtr.Zero)
                {
                    Console.WriteLine("[!] Failed to found clr.dll !!! ");
                }

                if(!DetoursPatchIAT(hClr, import, real))
                {
                    Console.WriteLine("[!] Unable to un sandbox clr.dll !!! ");
                }

                DelegateStore.Real[method] = Marshal.GetDelegateForFunctionPointer(real, attribute.DelegateType);
            }

            return 0;
        }
    }
}
 