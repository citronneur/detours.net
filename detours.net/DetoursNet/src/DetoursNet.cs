using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace detoursnet
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DetoursNetAttribute : Attribute
    {
        public string Module { get; set; }
        public Type DelegateType { get; set; }

        public DetoursNetAttribute(string module, Type delegateType)
        {
            this.Module = module;
            this.DelegateType = delegateType;
        }
    }
    public class DetoursNet
    {
        /// <summary>
        /// Real pointer to function
        /// </summary>
        public static Dictionary<MethodBase, Delegate> Real = new Dictionary<MethodBase, Delegate>();

        /// <summary>
        /// Mine function keep it global durinf lifecycle of application
        /// </summary>
        public static Dictionary<MethodBase, Delegate> Mine = new Dictionary<MethodBase, Delegate>();
    }
}
