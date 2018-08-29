#region

using System;
using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.OpCodes
{
    internal class SindByte : IOpCode
    {
        public byte Code => Constants.OP_SIND_BYTE;

        public unsafe void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var adrSlot = ctx.Stack[sp--];
            var valSlot = ctx.Stack[sp--];
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            if(adrSlot.O is IReference)
            {
                ((IReference) adrSlot.O).SetValue(ctx, valSlot, PointerType.BYTE);
            }
            else
            {
                var value = valSlot.U1;
                var ptr = (byte*) adrSlot.U8;
                *ptr = value;
            }
            state = ExecutionState.Next;
        }
    }

    internal class SindWord : IOpCode
    {
        public byte Code => Constants.OP_SIND_WORD;

        public unsafe void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var adrSlot = ctx.Stack[sp--];
            var valSlot = ctx.Stack[sp--];
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            if(adrSlot.O is IReference)
            {
                ((IReference) adrSlot.O).SetValue(ctx, valSlot, PointerType.WORD);
            }
            else
            {
                var value = valSlot.U2;
                var ptr = (ushort*) adrSlot.U8;
                *ptr = value;
            }
            state = ExecutionState.Next;
        }
    }

    internal class SindDword : IOpCode
    {
        public byte Code => Constants.OP_SIND_DWORD;

        public unsafe void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var adrSlot = ctx.Stack[sp--];
            var valSlot = ctx.Stack[sp--];
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            if(adrSlot.O is IReference)
            {
                ((IReference) adrSlot.O).SetValue(ctx, valSlot, PointerType.DWORD);
            }
            else
            {
                var value = valSlot.U4;
                var ptr = (uint*) adrSlot.U8;
                *ptr = value;
            }
            state = ExecutionState.Next;
        }
    }

    internal class SindQword : IOpCode
    {
        public byte Code => Constants.OP_SIND_QWORD;

        public unsafe void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var adrSlot = ctx.Stack[sp--];
            var valSlot = ctx.Stack[sp--];
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            if(adrSlot.O is IReference)
            {
                ((IReference) adrSlot.O).SetValue(ctx, valSlot, PointerType.QWORD);
            }
            else
            {
                var value = valSlot.U8;
                var ptr = (ulong*) adrSlot.U8;
                *ptr = value;
            }
            state = ExecutionState.Next;
        }
    }

    internal class SindObject : IOpCode
    {
        public byte Code => Constants.OP_SIND_OBJECT;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var adrSlot = ctx.Stack[sp--];
            var valSlot = ctx.Stack[sp--];
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            if(adrSlot.O is IReference) ((IReference) adrSlot.O).SetValue(ctx, valSlot, PointerType.OBJECT);
            else throw new ExecutionEngineException();
            state = ExecutionState.Next;
        }
    }

    internal class SindPtr : IOpCode
    {
        public byte Code => Constants.OP_SIND_PTR;

        public unsafe void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var adrSlot = ctx.Stack[sp--];
            var valSlot = ctx.Stack[sp--];
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;

            if(adrSlot.O is IReference)
            {
                ((IReference) adrSlot.O).SetValue(ctx, valSlot, Platform.x64 ? PointerType.QWORD : PointerType.DWORD);
            }
            else
            {
                if(Platform.x64)
                {
                    var ptr = (ulong*) adrSlot.U8;
                    *ptr = valSlot.U8;
                }
                else
                {
                    var ptr = (uint*) adrSlot.U8;
                    *ptr = valSlot.U4;
                }
            }
            state = ExecutionState.Next;
        }
    }
}