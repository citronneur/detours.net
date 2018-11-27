# detours.net
Détours.net use CLR as hooking engine. It's based on detours project from Microsoft and ability of CLR to generate transition stub for managed function to be called from unmanaged code.

*detours.net* is as simple to use as *DllImport* attribute for _pinvoke_ unmanaged code from managed code.

## Generate a plugin

Imagine you want to use *notepad.exe* to see particular binary file into *notepad.exe*, or if you want to analyse so dropping file from malware, you have to use *detours.net* to generate a *plugin*. 

First step is to generate a plugin. Plugin is simple a .net DLL link with *detoursnet.dll* assembly.

Then you have to tell *detours.net* how and from which API you want to hook. You just have to declare a delegate which match your target function signature, and declare your associate hook like this :

```c#
// Declare your delegate
public delegate IntPtr CreateFileWDelegate(
    [MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
    uint dwDesiredAccess,
    uint dwShareMode,
    IntPtr SecurityAttributes,
    uint dwCreationDisposition,
    uint dwFlagsAndAttributes,
    IntPtr hTemplateFile
);

// And now declare your hook
[Detours("kernel32.dll", typeof(CreateFileWDelegate))]
public static IntPtr CreateFileW(
    string lpFileName,
    uint dwDesiredAccess,
    uint dwShareMode,
    IntPtr SecurityAttributes,
    uint dwCreationDisposition,
    uint dwFlagsAndAttributes,
    IntPtr hTemplateFile
)
{
...
}
```

That's all. Build your assembly *myplugin.dll*, and run it with *detoursnetruntime.exe*.

```bat
.\detoursNetRuntime myplugin.dll c:\windows\notepad.exe
```

## How does it works ?

### DetoursNetRuntime

*detours.net* is based on detours project from Microsoft, which is mostly use in API hooking. It create a process in suspended mode, and then rewrete the IAT to insert a new dll at first place to be sure *Dllmain* of this dll will be execute first before all other code in your application. That's was be done by detoursNetRuntime, but inject a special DLL called detoursNetCLR.dll described in next chapter.

### DetoursNetCLR

DetoursNetCLR.dll is in charge to load CLR and the DetoursNet.dll assembly in current process. To do that we use CLR hosting from COM Component. But this forbidden from *DllMain* because of *loader lock*. To work around this issue, we use *Detours* from microsoft to hook entry point of target process, and load CLR into new *main* function.

### DetoursNet
