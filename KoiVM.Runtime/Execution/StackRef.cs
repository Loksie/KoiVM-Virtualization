#region

using System;
using KoiVM.Runtime.Execution.Internal;

#endregion

namespace KoiVM.Runtime.Execution
{
    internal class StackRef : IReference
    {
        public StackRef(uint pos)
        {
            StackPos = pos;
        }

        public uint StackPos
        {
            get;
            set;
        }

        public VMSlot GetValue(VMContext ctx, PointerType type)
        {
            var slot = ctx.Stack[StackPos];
            if(type == PointerType.BYTE)
                slot.U8 = slot.U1;
            else if(type == PointerType.WORD)
                slot.U8 = slot.U2;
            else if(type == PointerType.DWORD)
                slot.U8 = slot.U4;
            else if(slot.O is IValueTypeBox) slot.O = ((IValueTypeBox) slot.O).Clone();
            return slot;
        }

        public void SetValue(VMContext ctx, VMSlot slot, PointerType type)
        {
            if(type == PointerType.BYTE)
                slot.U8 = slot.U1;
            else if(type == PointerType.WORD)
                slot.U8 = slot.U2;
            else if(type == PointerType.DWORD)
                slot.U8 = slot.U4;
            ctx.Stack[StackPos] = slot;
        }

        public IReference Add(uint value)
        {
            return new StackRef(StackPos + value);
        }

        public IReference Add(ulong value)
        {
            return new StackRef(StackPos + (uint) (long) value);
        }

        public void ToTypedReference(VMContext ctx, TypedRefPtr typedRef, Type type)
        {
            ctx.Stack.ToTypedReference(StackPos, typedRef, type);
        }
    }
}