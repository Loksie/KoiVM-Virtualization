#region

using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.OpCodes
{
    internal class Nop : IOpCode
    {
        public byte Code => Constants.OP_NOP;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            state = ExecutionState.Next;
        }
    }
}