#region

using System;
using System.Collections.Generic;
using System.Reflection;
using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;
using KoiVM.Runtime.Execution.Internal;

#endregion

namespace KoiVM.Runtime.VCalls
{
    internal class Ldftn : IVCall
    {
        public byte Code => Constants.VCALL_LDFTN;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var methodSlot = ctx.Stack[sp--];
            var objectSlot = ctx.Stack[sp];

            if(objectSlot.O != null)
            {
                var method = (MethodInfo) ctx.Instance.Data.LookupReference(methodSlot.U4);
                var type = objectSlot.O.GetType();

                var baseTypes = new List<Type>();
                do
                {
                    baseTypes.Add(type);
                    type = type.BaseType;
                } while(type != null && type != method.DeclaringType);
                baseTypes.Reverse();

                var found = method;
                const BindingFlags fl = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                foreach(var baseType in baseTypes)
                foreach(var m in baseType.GetMethods(fl))
                    if(m.GetBaseDefinition() == found)
                    {
                        found = m;
                        break;
                    }

                ctx.Stack[sp] = new VMSlot {U8 = (ulong) found.MethodHandle.GetFunctionPointer()};
            }
            if(objectSlot.U8 != 0)
            {
                // intra linking
                var entryKey = ctx.Stack[--sp].U4;
                var codeAdr = methodSlot.U8;
                var sig = ctx.Instance.Data.LookupExport(objectSlot.U4).Signature;
                var ptr = VMTrampoline.CreateTrampoline(ctx.Instance.Data.Module, codeAdr, entryKey, sig, objectSlot.U4);
                ctx.Stack[sp] = new VMSlot {U8 = (ulong) ptr};
            }
            else
            {
                var method = (MethodBase) ctx.Instance.Data.LookupReference(methodSlot.U4);
                ctx.Stack[sp] = new VMSlot {U8 = (ulong) method.MethodHandle.GetFunctionPointer()};
            }

            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;
            state = ExecutionState.Next;
        }
    }
}