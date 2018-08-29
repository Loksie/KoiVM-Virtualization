#region

using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.OpCodes
{
    internal class RemDword : IOpCode
    {
        public byte Code => Constants.OP_REM_DWORD;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var op1Slot = ctx.Stack[sp - 1];
            var op2Slot = ctx.Stack[sp];
            sp -= 1;
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            var fl = ctx.Registers[Constants.REG_FL].U1;

            var slot = new VMSlot();
            if((fl & Constants.FL_UNSIGNED) != 0)
                slot.U4 = op1Slot.U4 % op2Slot.U4;
            else
                slot.U4 = (uint) ((int) op1Slot.U4 % (int) op2Slot.U4);
            ctx.Stack[sp] = slot;

            var mask = (byte) (Constants.FL_ZERO | Constants.FL_SIGN | Constants.FL_UNSIGNED);
            Utils.UpdateFL(op1Slot.U4, op2Slot.U4, slot.U4, slot.U4, ref fl, mask);
            ctx.Registers[Constants.REG_FL].U1 = fl;

            state = ExecutionState.Next;
        }
    }

    internal class RemQword : IOpCode
    {
        public byte Code => Constants.OP_REM_QWORD;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var op1Slot = ctx.Stack[sp - 1];
            var op2Slot = ctx.Stack[sp];
            sp -= 1;
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            var fl = ctx.Registers[Constants.REG_FL].U1;

            var slot = new VMSlot();
            if((fl & Constants.FL_UNSIGNED) != 0)
                slot.U8 = op1Slot.U8 % op2Slot.U8;
            else
                slot.U8 = (ulong) ((long) op1Slot.U8 % (long) op2Slot.U8);
            ctx.Stack[sp] = slot;

            var mask = (byte) (Constants.FL_ZERO | Constants.FL_SIGN | Constants.FL_UNSIGNED);
            Utils.UpdateFL(op1Slot.U8, op2Slot.U8, slot.U8, slot.U8, ref fl, mask);
            ctx.Registers[Constants.REG_FL].U1 = fl;

            state = ExecutionState.Next;
        }
    }

    internal class RemR32 : IOpCode
    {
        public byte Code => Constants.OP_REM_R32;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var op1Slot = ctx.Stack[sp - 1];
            var op2Slot = ctx.Stack[sp];
            sp -= 1;
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            var slot = new VMSlot();
            slot.R4 = op2Slot.R4 % op1Slot.R4;
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

    internal class RemR64 : IOpCode
    {
        public byte Code => Constants.OP_REM_R64;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var op1Slot = ctx.Stack[sp - 1];
            var op2Slot = ctx.Stack[sp];
            sp -= 1;
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            var slot = new VMSlot();
            slot.R8 = op2Slot.R8 % op1Slot.R8;
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