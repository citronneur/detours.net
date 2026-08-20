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
        public long Offset { get; set; }          // 0 => resolve by export name
        public string Section { get; set; }       // section the offset is relative to

        public DetoursAttribute(string module, Type delegateType)
        {
            this.Module = module;
            this.DelegateType = delegateType;
            this.Section = ".text";
        }

        // offset is relative to the given section (".text" by default) and used
        // to hook non-exported functions instead of resolving by export name
        public DetoursAttribute(string module, Type delegateType, long offset, string section = ".text")
            : this(module, delegateType)
        {
            this.Offset = offset;
            this.Section = section;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnInitAttribute : Attribute
    {
    }

    public class DelegateStore
    {
        /// <summary>
        /// Real pointer to function
        /// </summary>
        public static Dictionary<MethodBase, Delegate> Real = new Dictionary<MethodBase, Delegate>();

        /// <summary>
        /// Mine function keep it global durinf lifecycle of application
        /// </summary>
        public static Dictionary<MethodBase, Delegate> Mine = new Dictionary<MethodBase, Delegate>();

        /// <summary>
        /// Retrieve real delegate associate to real function
        /// </summary>
        /// <param name="method">Hook .net function</param>
        /// <returns>Associate native delegate</returns>
        public static Delegate GetReal(MethodBase method)
        {
            return Real[method];
        }
    }
}
