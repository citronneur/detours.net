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
        public static int Start(string arguments)
        {
            string assemblyName = System.Environment.GetEnvironmentVariable("DETOURSNET_ASSEMBLY_PLUGIN");
            Console.WriteLine("[+] Load assembly plugin " + assemblyName);

            Assembly assembly = Assembly.LoadFrom(assemblyName);

            var methods = assembly.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(DetoursAttribute), false).Length > 0)
                .ToArray();

            foreach (var method in methods)
            {
                var attribute = (DetoursAttribute)method.GetCustomAttributes(typeof(DetoursAttribute), false)[0];

                Console.WriteLine("[+] Hooking function " + method.Name + " from " + attribute.Module);

                DelegateStore.Mine[method] = Delegate.CreateDelegate(attribute.DelegateType, method);

                IntPtr module = LoadLibrary(attribute.Module);
                if (module == IntPtr.Zero)
                {
                    Console.WriteLine("[!] Failed to load " + attribute.Module);
                    continue;
                }

                IntPtr real = GetProcAddress(module, method.Name);
                if (real == IntPtr.Zero)
                {
                    Console.WriteLine("[!] Failed to found " + method.Name + " from " + attribute.Module);
                    continue;
                }
                    
                DetourTransactionBegin();
                DetourUpdateThread(GetCurrentThread());
                DetourAttach(ref real, Marshal.GetFunctionPointerForDelegate(DelegateStore.Mine[method]));
                DetourTransactionCommit();

                DelegateStore.Real[method] = Marshal.GetDelegateForFunctionPointer(real, attribute.DelegateType);
            }

            return 0;
            
        }
    }
}
 