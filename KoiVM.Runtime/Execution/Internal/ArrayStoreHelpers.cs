#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;

#endregion

namespace KoiVM.Runtime.Execution.Internal
{
    internal class ArrayStoreHelpers
    {
        private static readonly Hashtable storeHelpers = new Hashtable();

        public static void SetValue(Array array, int index, object value, Type valueType, Type elemType)
        {
            Debug.Assert(value == null || value.GetType() == valueType);

            var key = new KeyValuePair<Type, Type>(valueType, elemType);
            var helper = storeHelpers[key];
            if(helper == null)
                lock(storeHelpers)
                {
                    helper = storeHelpers[key];
                    if(helper == null)
                    {
                        helper = BuildStoreHelper(valueType, elemType);
                        storeHelpers[key] = helper;
                    }
                }
            ((_SetValue) helper)(array, index, value);
        }

        private static _SetValue BuildStoreHelper(Type valueType, Type elemType)
        {
            var paramTypes = new[] {typeof(Array), typeof(int), typeof(object)};
            var dm = new DynamicMethod("", typeof(void), paramTypes, Unverifier.Module, true);
            var gen = dm.GetILGenerator();

            gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
            gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
            gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_2);
            if(elemType.IsValueType)
                gen.Emit(System.Reflection.Emit.OpCodes.Unbox_Any, valueType);
            gen.Emit(System.Reflection.Emit.OpCodes.Stelem, elemType);
            gen.Emit(System.Reflection.Emit.OpCodes.Ret);

            return (_SetValue) dm.CreateDelegate(typeof(_SetValue));
        }

        private delegate void _SetValue(Array array, int index, object value);
    }
}