#region

using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.OpCodes
{
    internal class Pop : IOpCode
    {
        public byte Code => Constants.OP_POP;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var slot = ctx.Stack[sp];
            ctx.Stack.SetTopPosition(--sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            var regId = ctx.ReadByte();
            if((regId == Constants.REG_SP || regId == Constants.REG_BP) && slot.O is StackRef)
                ctx.Registers[regId] = new VMSlot {U4 = ((StackRef) slot.O).StackPos};
            else
                ctx.Registers[regId] = slot;
            state = ExecutionState.Next;
        }
    }
}