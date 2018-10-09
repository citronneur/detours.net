using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace detoursnet
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DetoursNetAttribute : Attribute
    {
        public string Module { get; set; }
        public string Function { get; set; }

        public DetoursNetAttribute(string module, string function)
        {
            this.Module = module;
            this.Function = function;
        }
    }
}
