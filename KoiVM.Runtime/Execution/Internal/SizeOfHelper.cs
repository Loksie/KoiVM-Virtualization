#region

using System;
using System.Collections;
using System.Reflection.Emit;

#endregion

namespace KoiVM.Runtime.Execution.Internal
{
    internal class SizeOfHelper
    {
        private static readonly Hashtable sizes = new Hashtable();

        public static int SizeOf(Type type)
        {
            var size = sizes[type];
            if(size == null)
                lock(sizes)
                {
                    size = sizes[type];
                    if(size == null)
                    {
                        size = GetSize(type);
                        sizes[type] = size;
                    }
                }
            return (int) size;
        }

        private static int GetSize(Type type)
        {
            var dm = new DynamicMethod("", typeof(int), Type.EmptyTypes, Unverifier.Module, true);
            var gen = dm.GetILGenerator();

            gen.Emit(System.Reflection.Emit.OpCodes.Sizeof, type);
            gen.Emit(System.Reflection.Emit.OpCodes.Ret);

            return (int) dm.Invoke(null, null);
        }
    }
}