#region

using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.OpCodes
{
    internal class IConvPtr : IOpCode
    {
        public byte Code => Constants.OP_ICONV_PTR;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var valueSlot = ctx.Stack[sp];

            var fl = (byte) (ctx.Registers[Constants.REG_FL].U1 & ~Constants.FL_OVERFLOW);
            if(!Platform.x64 && valueSlot.U8 >> 32 != 0)
                fl |= Constants.FL_OVERFLOW;
            ctx.Registers[Constants.REG_FL].U1 = fl;

            ctx.Stack[sp] = valueSlot;

            state = ExecutionState.Next;
        }
    }

    internal class IConvR64 : IOpCode
    {
        public byte Code => Constants.OP_ICONV_R64;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            // coreclr/src/vm/jithelpers.cpp JIT_Dbl2ULngOvf & JIT_Dbl2LngOvf

            var sp = ctx.Registers[Constants.REG_SP].U4;
            var valueSlot = ctx.Stack[sp];

            const double two63 = 2147483648.0 * 4294967296.0;
            const double two64 = 4294967296.0 * 4294967296.0;

            var value = valueSlot.R8;
            valueSlot.U8 = (ulong) (long) value;
            var fl = (byte) (ctx.Registers[Constants.REG_FL].U1 & ~Constants.FL_OVERFLOW);

            if((fl & Constants.FL_UNSIGNED) != 0)
            {
                if(!(value > -1.0 && value < two64))
                    fl |= Constants.FL_OVERFLOW;

                if(!(value < two63))
                    valueSlot.U8 = (ulong) ((long) value - two63) + 0x8000000000000000UL;
            }
            else
            {
                if(!(value > -two63 - 0x402 && value < two63))
                    fl |= Constants.FL_OVERFLOW;
            }

            ctx.Registers[Constants.REG_FL].U1 = (byte) (fl & ~Constants.FL_UNSIGNED);

            ctx.Stack[sp] = valueSlot;

            state = ExecutionState.Next;
        }
    }
}