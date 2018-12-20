# detours.net
Détours.net use CLR as hooking engine. It's based on [Detours](https://github.com/Microsoft/Detours) project from Microsoft, and ability of CLR to generate transition stub for managed function to be called from unmanaged code.

*detours.net* is as simple to use as *DllImport* attribute.

## Generate a plugin

Imagine you want to log all GUID of COM object used by a an application.
First step is to generate a plugin. Plugin is a simple .net Assembly linked with *detoursnet.dll*.

Then you have to tell *detours.net* where is original method and how to call it. You just have to declare a delegate which match your target method signature, and declare your associate hook like this :

```c#
namespace myplugin
{
    public static class Logger
    {
        // Declare your delegate
        public delegate int CoCreateInstanceDelegate(
            Guid rclsid, IntPtr pUnkOuter, 
            int dwClsContext, Guid riid, ref IntPtr ppv
        );

        // And now declare your hook
        [Detours("ole32.dll", typeof(CoCreateInstanceDelegate))]
        public static int CoCreateInstance(
            Guid rclsid, IntPtr pUnkOuter,
	    int dwClsContext, Guid riid, ref IntPtr ppv
        )
        {
            // Call real function
            int result = ((CoCreateInstanceDelegate)DelegateStore.GetReal(MethodInfo.GetCurrentMethod()))(rclsid, pUnkOuter, dwClsContext, riid, ref ppv);

            Console.WriteLine(" {" + rclsid.ToString() + "} {" + riid.ToString() + "} " + result.ToString("x"));
	
            return result;
        }
    }
}
```

That's all. Build your assembly *myplugin.dll*, and run it with *DetoursNetRuntime.exe*.

```bat
.\DetoursNetRuntime myplugin.dll c:\windows\notepad.exe
```

## How does it works ?

### DetoursNetRuntime

*detours.net* is based on detours project from Microsoft, which is mostly used for API hooking. It create a process in suspended mode, and then rewrite the Import Address Table (IAT) to insert a new module at first place. This implies that *Dllmain* of this module will be executed first before all other code in your application. That's was be done by *DetoursNetRuntime.exe*, but inject a special DLL called *DetoursNetCLR.dll* described in next chapter.

### DetoursNetCLR

DetoursNetCLR.dll is in charge to load CLR and the *DetoursNet.dll* assembly in current process. To do that we use CLR hosting with COM Component. But it's forbidden to init CLR from *DllMain* because of *loader lock*. *Loader Lock* is a special lock used to protect module list during process loading. To work around this issue, we used original *Detours* to hook entry point of target process, and load CLR into new *main* function.

To sandbox CLR, to avoid some infinite loop in calling target function, we used IAT unhooking on clr.dll module. But in most of case CLR use *pinvoke* to call native in API, mostly in mscorlib. *pinvoke* use internally *GetProcAddress* function to resolve API. We hook this API only for clr.dll through IAT, and cached real function pointer when clr call this API.

### DetoursNet

*DetoursNet.dll* which have two main roles. On one side is used by plugin developper to use attributes, and retrieve original address of target method. In other side is used by runtime to load assembly and find which method to hook.
