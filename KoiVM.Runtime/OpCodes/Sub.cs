#region

using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.OpCodes
{
    internal class SubR32 : IOpCode
    {
        public byte Code => Constants.OP_SUB_R32;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var op1Slot = ctx.Stack[sp - 1];
            var op2Slot = ctx.Stack[sp];
            sp -= 1;
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            var slot = new VMSlot();
            slot.R4 = op1Slot.R4 - op2Slot.R4;
            ctx.Stack[sp] = slot;

            var mask = (byte) (Constants.FL_ZERO | Constants.FL_SIGN | Constants.FL_OVERFLOW | Constants.FL_CARRY);
            var fl = (byte) (ctx.Registers[Constants.REG_FL].U1 & ~mask);
            if(slot.R4 == 0)
                fl |= Constants.FL_ZERO;
            else if(slot.R4 < 0)
                fl |= Constants.FL_SIGN;
            ctx.Registers[Constants.REG_FL].U1 = fl;

            state = ExecutionState.Next;
        }
    }

    internal class SubR64 : IOpCode
    {
        public byte Code => Constants.OP_SUB_R64;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var op1Slot = ctx.Stack[sp - 1];
            var op2Slot = ctx.Stack[sp];
            sp -= 1;
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            var slot = new VMSlot();
            slot.R8 = op1Slot.R8 - op2Slot.R8;
            ctx.Stack[sp] = slot;

            var mask = (byte) (Constants.FL_ZERO | Constants.FL_SIGN | Constants.FL_OVERFLOW | Constants.FL_CARRY);
            var fl = (byte) (ctx.Registers[Constants.REG_FL].U1 & ~mask);
            if(slot.R8 == 0)
                fl |= Constants.FL_ZERO;
            else if(slot.R8 < 0)
                fl |= Constants.FL_SIGN;
            ctx.Registers[Constants.REG_FL].U1 = fl;

            state = ExecutionState.Next;
        }
    }
}