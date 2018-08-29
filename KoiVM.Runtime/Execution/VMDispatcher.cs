#region

using System;
using System.Runtime.CompilerServices;
using System.Text;
using KoiVM.Runtime.Data;
using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution.Internal;

#endregion

namespace KoiVM.Runtime.Execution
{
    internal static class VMDispatcher
    {
        private static uint rand_state = (uint) Environment.TickCount;

        public static ExecutionState Run(VMContext ctx)
        {
            var state = ExecutionState.Next;
            var isAbnormal = true;
            do
            {
                try
                {
                    state = RunInternal(ctx);
                    switch(state)
                    {
                        case ExecutionState.Throw:
                        {
                            var sp = ctx.Registers[Constants.REG_SP].U4;
                            var ex = ctx.Stack[sp--];
                            ctx.Registers[Constants.REG_SP].U4 = sp;
                            DoThrow(ctx, ex.O);
                            break;
                        }
                        case ExecutionState.Rethrow:
                        {
                            var sp = ctx.Registers[Constants.REG_SP].U4;
                            var ex = ctx.Stack[sp--];
                            ctx.Registers[Constants.REG_SP].U4 = sp;
                            HandleRethrow(ctx, ex.O);
                            return state;
                        }
                    }
                    isAbnormal = false;
                }
                catch(Exception ex)
                {
                    // Patched to catch object
                    SetupEHState(ctx, ex);
                    isAbnormal = false;
                }
                finally
                {
                    if(isAbnormal)
                    {
                        HandleAbnormalExit(ctx);
                        state = ExecutionState.Exit;
                    }
                    else if(ctx.EHStates.Count > 0)
                    {
                        do
                        {
                            HandleEH(ctx, ref state);
                        } while(state == ExecutionState.Rethrow);
                    }
                }
            } while(state != ExecutionState.Exit);
            return state;
        }

        private static Exception Throw(object obj)
        {
            return null;
        }

        private static ExecutionState RunInternal(VMContext ctx)
        {
            ExecutionState state;
            while(true)
            {
                var op = ctx.ReadByte();
                var p = ctx.ReadByte(); // For key fixup
                OpCodeMap.Lookup(op).Run(ctx, out state);

                if(ctx.Registers[Constants.REG_IP].U8 == 1)
                    state = ExecutionState.Exit;

                if(state != ExecutionState.Next)
                    return state;
            }
        }

        private static void SetupEHState(VMContext ctx, object ex)
        {
            EHState ehState;
            if(ctx.EHStates.Count != 0)
            {
                ehState = ctx.EHStates[ctx.EHStates.Count - 1];
                if(ehState.CurrentFrame != null)
                {
                    if(ehState.CurrentProcess == EHState.EHProcess.Searching) ctx.Registers[Constants.REG_R1].U1 = 0;
                    else if(ehState.CurrentProcess == EHState.EHProcess.Unwinding) ehState.ExceptionObj = ex;
                    return;
                }
            }
            ehState = new EHState
            {
                OldBP = ctx.Registers[Constants.REG_BP],
                OldSP = ctx.Registers[Constants.REG_SP],
                ExceptionObj = ex,
                CurrentProcess = EHState.EHProcess.Searching,
                CurrentFrame = null,
                HandlerFrame = null
            };
            ctx.EHStates.Add(ehState);
        }

        private static void HandleRethrow(VMContext ctx, object ex)
        {
            if(ctx.EHStates.Count > 0)
                SetupEHState(ctx, ex);
            else
                DoThrow(ctx, ex);
        }

        private static unsafe string GetIP(VMContext ctx)
        {
            var ip = (uint) (ctx.Registers[Constants.REG_IP].U8 - (ulong) ctx.Instance.Data.KoiSection);
            ulong key = (uint) (new object().GetHashCode() + Environment.TickCount) | 1;
            return (((ip * key) << 32) | (key & ~1UL)).ToString("x16");
        }

        private static unsafe string StackWalk(VMContext ctx)
        {
            var ip = (uint) (ctx.Registers[Constants.REG_IP].U8 - (ulong) ctx.Instance.Data.KoiSection);
            var bp = ctx.Registers[Constants.REG_BP].U4;
            var sb = new StringBuilder();
            do
            {
                rand_state = rand_state * 1664525 + 1013904223;
                ulong key = rand_state | 1;
                sb.AppendFormat("|{0:x16}", ((ip * key) << 32) | (key & ~1UL));
                if(bp > 1)
                {
                    ip = (uint) (ctx.Stack[bp - 1].U8 - (ulong) ctx.Instance.Data.KoiSection);
                    var bpRef = ctx.Stack[bp].O as StackRef;
                    if(bpRef == null)
                        break;
                    bp = bpRef.StackPos;
                }
                else
                {
                    break;
                }
            } while(bp > 0);
            return sb.ToString(1, sb.Length - 1);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void DoThrow(VMContext ctx, object ex)
        {
            if(ex is Exception) EHHelper.Rethrow((Exception) ex, GetIP(ctx));
            throw Throw(ex);
        }

        private static void HandleEH(VMContext ctx, ref ExecutionState state)
        {
            var ehState = ctx.EHStates[ctx.EHStates.Count - 1];
            switch(ehState.CurrentProcess)
            {
                case EHState.EHProcess.Searching:
                {
                    if(ehState.CurrentFrame != null)
                    {
                        // Return from filter
                        var filterResult = ctx.Registers[Constants.REG_R1].U1 != 0;
                        if(filterResult)
                        {
                            ehState.CurrentProcess = EHState.EHProcess.Unwinding;
                            ehState.HandlerFrame = ehState.CurrentFrame;
                            ehState.CurrentFrame = ctx.EHStack.Count;
                            state = ExecutionState.Next;
                            goto case EHState.EHProcess.Unwinding;
                        }
                        ehState.CurrentFrame--;
                    }
                    else
                    {
                        ehState.CurrentFrame = ctx.EHStack.Count - 1;
                    }

                    var exType = ehState.ExceptionObj.GetType();
                    for(; ehState.CurrentFrame >= 0 && ehState.HandlerFrame == null; ehState.CurrentFrame--)
                    {
                        var frame = ctx.EHStack[ehState.CurrentFrame.Value];
                        if(frame.EHType == Constants.EH_FILTER)
                        {
                            // Run filter
                            var sp = ehState.OldSP.U4;
                            ctx.Stack.SetTopPosition(++sp);
                            ctx.Stack[sp] = new VMSlot {O = ehState.ExceptionObj};
                            ctx.Registers[Constants.REG_K1].U1 = 0;
                            ctx.Registers[Constants.REG_SP].U4 = sp;
                            ctx.Registers[Constants.REG_BP] = frame.BP;
                            ctx.Registers[Constants.REG_IP].U8 = frame.FilterAddr;
                            break;
                        }
                        if(frame.EHType == Constants.EH_CATCH)
                            if(frame.CatchType.IsAssignableFrom(exType))
                            {
                                ehState.CurrentProcess = EHState.EHProcess.Unwinding;
                                ehState.HandlerFrame = ehState.CurrentFrame;
                                ehState.CurrentFrame = ctx.EHStack.Count;
                                goto case EHState.EHProcess.Unwinding;
                            }
                    }
                    if(ehState.CurrentFrame == -1 && ehState.HandlerFrame == null)
                    {
                        ctx.EHStates.RemoveAt(ctx.EHStates.Count - 1);
                        state = ExecutionState.Rethrow;
                        if(ctx.EHStates.Count == 0)
                            HandleRethrow(ctx, ehState.ExceptionObj);
                    }
                    else
                    {
                        state = ExecutionState.Next;
                    }
                    break;
                }
                case EHState.EHProcess.Unwinding:
                {
                    ehState.CurrentFrame--;
                    int i;
                    for(i = ehState.CurrentFrame.Value; i > ehState.HandlerFrame.Value; i--)
                    {
                        var frame = ctx.EHStack[i];
                        ctx.EHStack.RemoveAt(i);
                        if(frame.EHType == Constants.EH_FAULT || frame.EHType == Constants.EH_FINALLY)
                        {
                            // Run finally
                            SetupFinallyFrame(ctx, frame);
                            break;
                        }
                    }
                    ehState.CurrentFrame = i;

                    if(ehState.CurrentFrame == ehState.HandlerFrame)
                    {
                        var frame = ctx.EHStack[ehState.HandlerFrame.Value];
                        ctx.EHStack.RemoveAt(ehState.HandlerFrame.Value);
                        // Run handler
                        frame.SP.U4++;
                        ctx.Stack.SetTopPosition(frame.SP.U4);
                        ctx.Stack[frame.SP.U4] = new VMSlot {O = ehState.ExceptionObj};

                        ctx.Registers[Constants.REG_K1].U1 = 0;
                        ctx.Registers[Constants.REG_SP] = frame.SP;
                        ctx.Registers[Constants.REG_BP] = frame.BP;
                        ctx.Registers[Constants.REG_IP].U8 = frame.HandlerAddr;

                        ctx.EHStates.RemoveAt(ctx.EHStates.Count - 1);
                    }
                    state = ExecutionState.Next;
                    break;
                }
                default:
                    throw new ExecutionEngineException();
            }
        }

        private static void HandleAbnormalExit(VMContext ctx)
        {
            var oldBP = ctx.Registers[Constants.REG_BP];
            var oldSP = ctx.Registers[Constants.REG_SP];

            for(var i = ctx.EHStack.Count - 1; i >= 0; i--)
            {
                var frame = ctx.EHStack[i];
                if(frame.EHType == Constants.EH_FAULT || frame.EHType == Constants.EH_FINALLY)
                {
                    SetupFinallyFrame(ctx, frame);
                    Run(ctx);
                }
            }
            ctx.EHStack.Clear();
        }

        private static void SetupFinallyFrame(VMContext ctx, EHFrame frame)
        {
            frame.SP.U4++;
            ctx.Registers[Constants.REG_K1].U1 = 0;
            ctx.Registers[Constants.REG_SP] = frame.SP;
            ctx.Registers[Constants.REG_BP] = frame.BP;
            ctx.Registers[Constants.REG_IP].U8 = frame.HandlerAddr;

            ctx.Stack[frame.SP.U4] = new VMSlot {U8 = 1};
        }
    }
}