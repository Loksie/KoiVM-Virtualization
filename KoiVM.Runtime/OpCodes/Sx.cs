#region

using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.OpCodes
{
    internal class SxDword : IOpCode
    {
        public byte Code => Constants.OP_SX_DWORD;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var operand = ctx.Stack[sp];
            if((operand.U4 & 0x80000000) != 0)
                operand.U8 = 0xffffffff00000000 | operand.U4;
            ctx.Stack[sp] = operand;

            state = ExecutionState.Next;
        }
    }

    internal class SxWord : IOpCode
    {
        public byte Code => Constants.OP_SX_WORD;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var operand = ctx.Stack[sp];
            if((operand.U2 & 0x8000) != 0)
                operand.U4 = operand.U2 | 0xffff0000;
            ctx.Stack[sp] = operand;

            state = ExecutionState.Next;
        }
    }

    internal class SxByte : IOpCode
    {
        public byte Code => Constants.OP_SX_BYTE;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var operand = ctx.Stack[sp];
            if((operand.U1 & 0x80) != 0)
                operand.U4 = operand.U1 | 0xffffff00;
            ctx.Stack[sp] = operand;

            state = ExecutionState.Next;
        }
    }
}