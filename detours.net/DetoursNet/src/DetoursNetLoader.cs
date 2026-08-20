using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;

namespace DetoursNet
{
    public static class Loader
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
        private static extern long DetourAttach(ref IntPtr a, IntPtr b);

        [DllImport("DetoursDll.dll")]
        private static extern long DetourUpdateThread(IntPtr a);

        [DllImport("DetoursDll.dll")]
        private static extern long DetourTransactionBegin();

        [DllImport("DetoursDll.dll")]
        private static extern long DetourTransactionCommit();

        [DllImport("DetoursDll.dll")]
        private static extern bool DetoursPatchIAT(IntPtr hModule, IntPtr import, IntPtr real);

        [DllImport("DetoursNetCLR.dll", CharSet=CharSet.Ansi)]
        private static extern void DetoursCLRSetGetProcAddressCache(IntPtr hModule, string procName, IntPtr real);

        /// <summary>
        /// Find all static method with custom attribute type
        /// </summary>
        /// <param name="assembly">Assembly object</param>
        /// <param name="attributeType">type of custom attribute</param>
        /// <returns>All method infos</returns>
        private static MethodInfo[] FindAttribute(this Assembly assembly, Type attributeType)
        {
            return assembly.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(attributeType, false).Length > 0)
                .ToArray();
        }

        /// <summary>
        /// Resolve a module by name (loading it if needed) and return where the
        /// given section (e.g. ".text") is loaded in memory.
        /// </summary>
        /// <param name="moduleName">module name, e.g. "winmine.exe"</param>
        /// <param name="sectionName">name of the section, e.g. ".text"</param>
        /// <returns>loaded address of the section, or IntPtr.Zero if not found</returns>
        public static IntPtr GetSectionBase(string moduleName, string sectionName)
        {
            IntPtr module = LoadLibrary(moduleName);
            if (module == IntPtr.Zero) {
                module = GetModuleHandle(moduleName);
            }
            if (module == IntPtr.Zero) {
                return IntPtr.Zero;
            }
            return GetSectionBase(module, sectionName);
        }

        /// <summary>
        /// Walk the PE headers of a mapped module to find where a section
        /// (e.g. ".text") is loaded in memory. The module handle is the load
        /// base, so the returned address is base + section VirtualAddress.
        /// </summary>
        /// <param name="module">module load base (HMODULE)</param>
        /// <param name="sectionName">name of the section, e.g. ".text"</param>
        /// <returns>loaded address of the section, or IntPtr.Zero if not found</returns>
        public static IntPtr GetSectionBase(IntPtr module, string sectionName)
        {
            // IMAGE_DOS_HEADER.e_lfanew is at offset 0x3C
            int lfanew = Marshal.ReadInt32(module, 0x3C);
            IntPtr ntHeaders = module + lfanew;

            // IMAGE_NT_HEADERS = Signature (4) + IMAGE_FILE_HEADER (20) + OptionalHeader
            // IMAGE_FILE_HEADER.NumberOfSections     at file header offset 2  => ntHeaders + 6
            // IMAGE_FILE_HEADER.SizeOfOptionalHeader at file header offset 16 => ntHeaders + 20
            short numberOfSections = Marshal.ReadInt16(ntHeaders, 6);
            short sizeOfOptionalHeader = Marshal.ReadInt16(ntHeaders, 20);

            // section table starts right after the optional header
            IntPtr section = ntHeaders + 4 + 20 + sizeOfOptionalHeader;

            for (int i = 0; i < numberOfSections; i++)
            {
                // IMAGE_SECTION_HEADER is 40 bytes; Name is 8 bytes, null padded
                byte[] name = new byte[8];
                Marshal.Copy(section, name, 0, 8);
                string currentName = System.Text.Encoding.ASCII.GetString(name).TrimEnd('\0');

                if (currentName == sectionName)
                {
                    // IMAGE_SECTION_HEADER.VirtualAddress is at offset 12
                    int virtualAddress = Marshal.ReadInt32(section, 12);
                    return module + virtualAddress;
                }

                section = section + 40;
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Main entry point of loader
        /// </summary>
        public static int Start(string arguments)
        {
            string assemblyName = System.Environment.GetEnvironmentVariable("DETOURSNET_ASSEMBLY_PLUGIN");

            Assembly assembly = Assembly.LoadFrom(assemblyName);

            foreach(var method in assembly.FindAttribute(typeof(OnInitAttribute))) {
                method.Invoke(null, null);
            }

            foreach (var method in assembly.FindAttribute(typeof(DetoursAttribute))) {
                var attribute = (DetoursAttribute)method.GetCustomAttributes(typeof(DetoursAttribute), false)[0];

                DelegateStore.Mine[method] = Delegate.CreateDelegate(attribute.DelegateType, method);

                // LoadLibrary returns NULL for the target's own executable (and
                // some already-mapped modules), so fall back to GetModuleHandle
                IntPtr module = LoadLibrary(attribute.Module);
                if (module == IntPtr.Zero) {
                    module = GetModuleHandle(attribute.Module);
                }
                if (module == IntPtr.Zero) {
                    continue;
                }

                IntPtr real;
                if (attribute.Offset != 0) {
                    // hardcoded offset: resolve relative to the loaded section
                    // (offset is relative to the section start, not the image base)
                    IntPtr sectionBase = GetSectionBase(module, attribute.Section);
                    if (sectionBase == IntPtr.Zero) {
                        continue;
                    }
                    real = sectionBase + (int)attribute.Offset;
                }
                else {
                    real = GetProcAddress(module, method.Name);
                    if (real == IntPtr.Zero) {
                        continue;
                    }
                }

                // record pointer
                IntPtr import = real;

                DetourTransactionBegin();
                DetourUpdateThread(GetCurrentThread());
                DetourAttach(ref real, Marshal.GetFunctionPointerForDelegate(DelegateStore.Mine[method]));
                DetourTransactionCommit();

                // The pinvoke cache and clr.dll IAT patch only matter for
                // exports the CLR itself resolves by name; skip for offset hooks
                if (attribute.Offset == 0) {
                    // Add function to pinvoke cache
                    DetoursCLRSetGetProcAddressCache(module, method.Name, real);

                    // and so on patch IAT of clr module
                    DetoursPatchIAT(GetModuleHandle("clr.dll"), import, real);
                }

                DelegateStore.Real[method] = Marshal.GetDelegateForFunctionPointer(real, attribute.DelegateType);
            }

            return 0;
        }
    }
}
 