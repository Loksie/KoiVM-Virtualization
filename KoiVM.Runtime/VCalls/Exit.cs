#region

using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.VCalls
{
    internal class Exit : IVCall
    {
        public byte Code => Constants.VCALL_EXIT;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            state = ExecutionState.Exit;
        }
    }
}