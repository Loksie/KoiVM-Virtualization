#region

using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.VCalls
{
    internal class Localloc : IVCall
    {
        public byte Code => Constants.VCALL_LOCALLOC;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var bp = ctx.Registers[Constants.REG_BP].U4;
            var size = ctx.Stack[sp].U4;
            ctx.Stack[sp] = new VMSlot
            {
                U8 = (ulong) ctx.Stack.Localloc(bp, size)
            };

            state = ExecutionState.Next;
        }
    }
}