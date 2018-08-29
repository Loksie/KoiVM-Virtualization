#region

using System;
using System.Diagnostics;
using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.VCalls
{
    internal class Box : IVCall
    {
        public byte Code => Constants.VCALL_BOX;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var typeSlot = ctx.Stack[sp--];
            var valSlot = ctx.Stack[sp];

            var valType = (Type) ctx.Instance.Data.LookupReference(typeSlot.U4);
            if(Type.GetTypeCode(valType) == TypeCode.String && valSlot.O == null)
            {
                valSlot.O = ctx.Instance.Data.LookupString(valSlot.U4);
            }
            else
            {
                Debug.Assert(valType.IsValueType);
                valSlot.O = valSlot.ToObject(valType);
            }
            ctx.Stack[sp] = valSlot;

            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;
            state = ExecutionState.Next;
        }
    }
}