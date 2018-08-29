#region

using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.VCalls
{
    internal interface IVCall
    {
        byte Code
        {
            get;
        }

        void Run(VMContext ctx, out ExecutionState state);
    }
}