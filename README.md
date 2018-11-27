# detours.net
Détours.net use CLR as hooking engine. It's based on detours project from Microsoft and ability of CLR to generate transition stub for managed function to be called from unmanaged code.

*detours.net* is as simple to use as *DllImport* attribute for _pinvoke_ unmanaged code from managed code.

## Generate a plugin

Imagine you want to use *notepad.exe* to see PE header into *notepad.exe*, you have to use *detours.net*. 

First step is to generate a plugin.

Plugin is simple a .net DLL link with *detoursnet.dll* assembly.

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

That's all. Build your assembly, and run it with *detoursnetruntime.exe*.

```bat
.\detoursNetRuntime myhook.dll c:\windows\notepad.exe
```
