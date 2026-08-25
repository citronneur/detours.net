# detours.net

Détours.net uses the CLR as its hooking engine. It is based on Microsoft's [Detours](https://github.com/Microsoft/Detours) project, combined with the CLR's ability to generate transition stubs so that managed functions can be called from unmanaged code.

*detours.net* is as simple to use as the *DllImport* attribute.

## How do I build it?

```
git clone https://github.com/citronneur/detours.net
mkdir build
cd build
cmake -G "Visual Studio 17 2022" -A x64 ..\detours.net
```

Then open the generated solution in Visual Studio and build it. This produces four main binaries:

* `DetoursNetRuntime.exe` — the launcher
* `DetoursNetCLR.dll` — the loader
* `DetoursNet.dll` — the interface
* `DetoursDll.dll` — the hooking engine

## How do I hook a native function from managed code?

In this example, we want to log the GUID of every COM object used by an application, taking advantage of the powerful .NET console API.

To do this, create a C# DLL project in Visual Studio, reference the *DetoursNet.dll* assembly, and name it *myplugin*.

You then have to tell *detours.net* where the original method lives and how to call it. Declare a delegate that matches the target method's signature, and declare the associated hook like this:

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
            // Call the real function
            int result = ((CoCreateInstanceDelegate)DelegateStore.GetReal(MethodInfo.GetCurrentMethod()))(rclsid, pUnkOuter, dwClsContext, riid, ref ppv);

            Console.WriteLine(" {" + rclsid.ToString() + "} {" + riid.ToString() + "} " + result.ToString("x"));
	
            return result;
        }
    }
}
```

That's all. Build your *myplugin.dll* assembly and run it with *DetoursNetRuntime.exe*:

```bat
.\DetoursNetRuntime myplugin.dll c:\windows\notepad.exe
```

## How do I hook a non-exported function?

Resolving a hook by export name only works for functions the target module actually exports. Many interesting functions are internal and have no export entry, so there is no name to match. For these, *detours.net* can hook a function directly by its **offset from a section** (typically `.text`).

The `[Detours]` attribute has an overload that takes an offset and, optionally, the section the offset is relative to (it defaults to `.text`):

```c#
// resolve by export name (default)
[Detours("kernel32.dll", typeof(FindResourceWDelegate))]

// resolve by offset from .text (for non-exported functions)
[Detours("winmine.exe", typeof(GameOverDelegate), 0x247C)]

// resolve by offset from an explicit section
[Detours("winmine.exe", typeof(GameOverDelegate), 0x247C, ".text")]
```

The offset is added to the load address of the section, so the runtime hooks `(section base + offset)`. You typically find such offsets with a disassembler (IDA, Ghidra, x64dbg, …) by locating the function and subtracting the section base from its virtual address.

For an internal function you must also declare the correct calling convention on the delegate, since there is no import metadata to describe it. Non-exported Windows functions are usually `stdcall`:

```c#
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate void GameOverDelegate(uint status);

// winmine.exe does not export GameOver, so we hook it at .text + 0x247C
[Detours("winmine.exe", typeof(GameOverDelegate), 0x247C)]
public static void GameOver(uint status)
{
    // ... your logic ...

    // call the real function so the program keeps behaving normally
    ((GameOverDelegate)DelegateStore.GetReal(MethodInfo.GetCurrentMethod()))(status);
}
```

The same section-resolution mechanism is available to your own code: it is handy for reading or writing global state inside the target. For example, you can locate the base of the `.data` section, then read or write a global variable at a known offset:

```c#
IntPtr dataBase = GetSectionBase("winmine.exe", ".data");
int difficulty = Marshal.ReadInt16(dataBase + 0x6A0);
```

Finally, a method marked with `[OnInit]` is called once when the plugin is loaded, which is a good place to set up your plugin (allocate a console, initialize network settings, print a banner, …):

```c#
[OnInit]
public static void OnInit()
{
    AllocConsole();
    Console.WriteLine("mineguard loaded");
}
```

## How does it work?

*detours.net* is split into three parts.

### DetoursNetRuntime.exe

*detours.net* is based on Microsoft's Detours project, which is mostly used for API hooking. Detours creates a process in suspended mode and rewrites the Import Address Table (IAT) to insert a new module at the first position. This means that the *DllMain* of this module runs before any other code in the application. This is handled by *DetoursNetRuntime.exe*, which can be seen as a launcher for your target program that injects a special DLL called *DetoursNetCLR.dll*, described in the next section.

### DetoursNetCLR.dll

*DetoursNetCLR.dll* is responsible for loading the CLR (Common Language Runtime) and the *DetoursNet.dll* assembly into the current process. It does so by hosting the CLR through COM. As explained above, the *DllMain* of *DetoursNetCLR.dll* is the first code to run in the target process. However, initializing the CLR from *DllMain* is forbidden because of the *loader lock* — a lock the loader uses to protect the module list while a process is loading. To work around this, we use the original *Detours* library to hook the entry point of the target process and initialize the CLR from a new *main* function.

To sandbox the CLR and avoid infinite loops when calling target functions, we use IAT (un)hooking on the *clr.dll* module. First, we cache the real function pointers, then we hook the *GetProcAddress* function. In most cases the CLR uses *p/invoke* to call native APIs, mostly in mscorlib, and *p/invoke* internally uses *GetProcAddress* to resolve those APIs. When the CLR calls *GetProcAddress* to retrieve a native API, we check whether it is a hooked function, and if so we return the real pointer.

### DetoursNet.dll

*DetoursNet.dll* has two main roles. On one side, it is used by plugin developers: it provides the attributes used to declare each function hook, and it lets you retrieve the real address of a hooked method. On the other side, it is used by the runtime to load the plugin assembly and find every method to hook, thanks to the attributes provided by the plugin developer.

## Plugins

Plugins are hooking DLLs written for a particular purpose and contributed by the community. All plugins live under the *plugins* directory:

* **procmon** — logs a large number of native Windows APIs
* **proxysocks** — routes any Windows application that uses sockets through a SOCKS proxy
