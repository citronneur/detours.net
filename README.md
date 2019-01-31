# detours.net
Détours.net use CLR as hooking engine. It's based on [Detours](https://github.com/Microsoft/Detours) project from Microsoft, and ability of CLR to generate transition stub for managed function to be called from unmanaged code.

*detours.net* is as simple to use as *DllImport* attribute.

## How to build it ?

```
git clone https://github.com/citronneur/detours.net
mkdir build
cd build
cmake -G "Visual Studio 15 2017 Win64" ..\detours.net
```

Build solution with Visual Studio. This will produce four main executables:
* DetoursNetRuntime.exe which is the launcher
* DetoursNetCLR.dll which is the loader
* DetoursNet.dll which is the interface
* DetoursDll.dll which is the hooker

## How to hook anything native to managed ?

In this exemple, we want to log all GUID of COM object used by a an application using powerfull .Net API for console application.
To do it we create a C# DLL project with visual studio, linked with *DetoursNet.dll* assembly, named *myplugin*.

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

*detours.net* is splitted into three part :

### DetoursNetRuntime.exe

*detours.net* is based on detours project from Microsoft, which is mostly used for API hooking. It create a process in suspended mode, and then rewrite the Import Address Table (IAT) to insert a new module at first place. This implies that *Dllmain* of this module will be executed first before all other code in your application. That's was be done by *DetoursNetRuntime.exe*, which could be view as a launcher of your targeted program then inject a special DLL called *DetoursNetCLR.dll* described in next chapter.

### DetoursNetCLR.dll

DetoursNetCLR.dll is in charge to load CLR (Common Language Runtime) and the *DetoursNet.dll* assembly in current process. To do that we use CLR hosting through COM. As we seen in last chapter, the DllMain function of *DetoursNetCLR.dll* will be the fisrt code run in your target process. But it's forbidden to init CLR from *DllMain* because of *Loader lock*. *Loader Lock* is a special lock used by the loader to protect module list during process loading. To work around this issue, we used original *Detours* library to hook entry point of target process, and load CLR into new *main* function.

To sandbox CLR, and avoid some infinite loop in calling target function, we used IAT (un)hooking on *clr.dll* module. First of all, we cache real functions pointer, then, we hook *GetProcAddress* function. In most case, CLR use *pinvoke* to call native API, mostly in mscorlib. *pinvoke* use internally *GetProcAddress* function to resolve API. When CLR call *GetProcAddress* to retrieve native API, we check if it's a hooked function, and if it's true, we return real pointer.

### DetoursNet.dll

*DetoursNet.dll* which have two main roles. On one side is used by plugin developper, firstly to use attributes to indicate all function hook, secondly to retrieve real address of hooked method. In other side is used by runtime to load plugin assembly and find all method to hook, thanks to attributes provided by plugin developper.

## Plugins

Plugins are hooking dll use for a particular purpose, and provided by community. All plugins are available under *plugin* directory:

* procmon log a lot of windows native API
* proxysocks convert any windows application using socket to pass through a proxy socks


