using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace detoursnet
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DetoursNetAttribute : Attribute
    {
        public string Module { get; set; }
        public Type DelegateType { get; set; }
        public Delegate Real { get; set; }
        public Delegate Mine { get; set; }

        public DetoursNetAttribute(string module, Type delegateType)
        {
            this.Module = module;
            this.DelegateType = delegateType;
        }
    }
}
