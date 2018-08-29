#region

using System;

#endregion

namespace KoiVM.Runtime
{
    public class VMEntry
    {
        public static object Run(RuntimeTypeHandle type, uint id, object[] args)
        {
            var module = Type.GetTypeFromHandle(type).Module;
            return VMInstance.Instance(module).Run(id, args);
        }

        public static unsafe void Run(RuntimeTypeHandle type, uint id, void*[] typedRefs, void* retTypedRef)
        {
            var module = Type.GetTypeFromHandle(type).Module;
            VMInstance.Instance(module).Run(id, typedRefs, retTypedRef);
        }

        internal static object RunInternal(int moduleId, ulong codeAddr, uint key, uint sigId, object[] args)
        {
            return VMInstance.Instance(moduleId).Run(codeAddr, key, sigId, args);
        }

        internal static unsafe void RunInternal(int moduleId, ulong codeAddr, uint key, uint sigId, void*[] typedRefs,
            void* retTypedRef)
        {
            VMInstance.Instance(moduleId).Run(codeAddr, key, sigId, typedRefs, retTypedRef);
        }
    }
}