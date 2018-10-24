using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DetoursNet
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DetoursAttribute : Attribute
    {
        public string Module { get; set; }
        public Type DelegateType { get; set; }

        public DetoursAttribute(string module, Type delegateType)
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
