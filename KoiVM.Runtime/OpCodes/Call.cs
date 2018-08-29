#region

using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.OpCodes
{
    internal class Call : IOpCode
    {
        public byte Code => Constants.OP_CALL;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var slot = ctx.Stack[sp];
            ctx.Stack[sp] = ctx.Registers[Constants.REG_IP];
            ctx.Registers[Constants.REG_IP].U8 = slot.U8;
            state = ExecutionState.Next;
        }
    }
}