#region

using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.OpCodes
{
    internal class ShlDword : IOpCode
    {
        public byte Code => Constants.OP_SHL_DWORD;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var op1Slot = ctx.Stack[sp - 1];
            var op2Slot = ctx.Stack[sp];
            sp -= 1;
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            var slot = new VMSlot();
            slot.U4 = op1Slot.U4 << (int) op2Slot.U4;
            ctx.Stack[sp] = slot;

            var mask = (byte) (Constants.FL_ZERO | Constants.FL_SIGN);
            var fl = ctx.Registers[Constants.REG_FL].U1;
            Utils.UpdateFL(op1Slot.U4, op2Slot.U4, slot.U4, slot.U4, ref fl, mask);
            ctx.Registers[Constants.REG_FL].U1 = fl;

            state = ExecutionState.Next;
        }
    }

    internal class ShlQword : IOpCode
    {
        public byte Code => Constants.OP_SHL_QWORD;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var op1Slot = ctx.Stack[sp - 1];
            var op2Slot = ctx.Stack[sp];
            sp -= 1;
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            var slot = new VMSlot();
            slot.U8 = op1Slot.U8 << (int) op2Slot.U4;
            ctx.Stack[sp] = slot;

            var mask = (byte) (Constants.FL_ZERO | Constants.FL_SIGN);
            var fl = ctx.Registers[Constants.REG_FL].U1;
            Utils.UpdateFL(op1Slot.U8, op2Slot.U8, slot.U8, slot.U8, ref fl, mask);
            ctx.Registers[Constants.REG_FL].U1 = fl;

            state = ExecutionState.Next;
        }
    }
}