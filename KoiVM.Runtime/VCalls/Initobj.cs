#region

using System;
using System.Runtime.Serialization;
using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;
using KoiVM.Runtime.Execution.Internal;

#endregion

namespace KoiVM.Runtime.VCalls
{
    internal class Initobj : IVCall
    {
        public byte Code => Constants.VCALL_INITOBJ;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var typeSlot = ctx.Stack[sp--];
            var addrSlot = ctx.Stack[sp--];

            var type = (Type) ctx.Instance.Data.LookupReference(typeSlot.U4);
            if(addrSlot.O is IReference)
            {
                var reference = (IReference) addrSlot.O;
                var slot = new VMSlot();
                if(type.IsValueType)
                {
                    object def = null;
                    if(Nullable.GetUnderlyingType(type) == null)
                        def = FormatterServices.GetUninitializedObject(type);
                    slot.O = ValueTypeBox.Box(def, type);
                }
                else
                {
                    slot.O = null;
                }
                reference.SetValue(ctx, slot, PointerType.OBJECT);
            }
            else
            {
                throw new NotSupportedException();
            }

            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;
            state = ExecutionState.Next;
        }
    }
}