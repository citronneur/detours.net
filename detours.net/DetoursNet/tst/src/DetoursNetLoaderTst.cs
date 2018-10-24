using System;
using System.Reflection;
using System.Runtime.InteropServices;
using DetoursNet;
namespace DetoursNetLoaderTst
{
    public class DetoursNetLoaderTst
    {
        static void Main(string[] args)
        {
            Loader.Start("foo");
        }
    }
}
