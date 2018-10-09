using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace detoursnetloader
{
    public class DetoursNetLoader
    {
        /// <summary>
        /// Main entry point of loader
        /// </summary>
        public static void DetoursNetLoader_Start()
        {
            //System.Diagnostics.Debugger.Launch();
            // Get back the env variables
            Assembly plugin = Assembly.LoadFile("c:\\dev\\build_x64\\bin\\Debug\\TraceCreateFile.dll");
            MethodInfo method = plugin.GetType("tracecreatefile.TraceCreateFile").GetMethod("GetModuleHandleHook");
            Delegate methodDelegate = Delegate.CreateDelegate(method.GetType(), method);
            Marshal.GetFunctionPointerForDelegate(methodDelegate);
        }
    }
}
