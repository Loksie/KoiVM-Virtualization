#region

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Permissions;

#endregion

namespace KoiVM.Runtime.Execution.Internal
{
    internal static class Unverifier
    {
        public static readonly Module Module;

        static Unverifier()
        {
            var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("Fish"), AssemblyBuilderAccess.Run);
            var mod = asm.DefineDynamicModule("Fish");
            var att =
                new CustomAttributeBuilder(typeof(SecurityPermissionAttribute).GetConstructor(new[] {typeof(SecurityAction)}),
                    new object[] {SecurityAction.Assert},
                    new[] {typeof(SecurityPermissionAttribute).GetProperty("SkipVerification")},
                    new object[] {true});
            mod.SetCustomAttribute(att);
            Module = mod.DefineType(" ").CreateType().Module;
        }
    }
}