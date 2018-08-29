#region

using System;
using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.VCalls
{
    internal class Ckfinite : IVCall
    {
        public byte Code => Constants.VCALL_CKFINITE;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var valueSlot = ctx.Stack[sp--];

            var fl = ctx.Registers[Constants.REG_FL].U1;
            if((fl & Constants.FL_UNSIGNED) != 0)
            {
                var v = valueSlot.R4;
                if(float.IsNaN(v) || float.IsInfinity(v))
                    throw new ArithmeticException();
            }
            else
            {
                var v = valueSlot.R8;
                if(double.IsNaN(v) || double.IsInfinity(v))
                    throw new ArithmeticException();
            }

            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;
            state = ExecutionState.Next;
        }
    }
}