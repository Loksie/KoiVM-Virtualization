#region

using System;
using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.OpCodes
{
    internal class Leave : IOpCode
    {
        public byte Code => Constants.OP_LEAVE;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var handler = ctx.Stack[sp--].U8;

            var frameIndex = ctx.EHStack.Count - 1;
            var frame = ctx.EHStack[frameIndex];

            if(frame.HandlerAddr != handler)
                throw new InvalidProgramException();
            ctx.EHStack.RemoveAt(frameIndex);

            if(frame.EHType == Constants.EH_FINALLY)
            {
                ctx.Stack[++sp] = ctx.Registers[Constants.REG_IP];
                ctx.Registers[Constants.REG_K1].U1 = 0;
                ctx.Registers[Constants.REG_IP].U8 = frame.HandlerAddr;
            }

            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            state = ExecutionState.Next;
        }
    }
}