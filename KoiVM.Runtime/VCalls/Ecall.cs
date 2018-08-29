#region

using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;
using KoiVM.Runtime.Execution.Internal;

#endregion

namespace KoiVM.Runtime.VCalls
{
    internal unsafe class Ecall : IVCall
    {
        public byte Code => Constants.VCALL_ECALL;

        public void Run(VMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[Constants.REG_SP].U4;
            var mSlot = ctx.Stack[sp--];

            var mId = mSlot.U4 & 0x3fffffff;
            var opCode = (byte) (mSlot.U4 >> 30);
            var targetMethod = (MethodBase) ctx.Instance.Data.LookupReference(mId);
            var typedInvoke = opCode == Constants.ECALL_CALLVIRT_CONSTRAINED;
            if(!typedInvoke)
                typedInvoke = NeedTypedInvoke(ctx, sp, targetMethod, opCode == Constants.ECALL_NEWOBJ);

            if(typedInvoke)
                InvokeTyped(ctx, targetMethod, opCode, ref sp, out state);
            else
                InvokeNormal(ctx, targetMethod, opCode, ref sp, out state);
        }

        private static object PopObject(VMContext ctx, Type type, ref uint sp)
        {
            var arg = ctx.Stack[sp--];
            if(Type.GetTypeCode(type) == TypeCode.String && arg.O == null)
                return ctx.Instance.Data.LookupString(arg.U4);
            return arg.ToObject(type);
        }

        private static IReference PopRef(VMContext ctx, Type type, ref uint sp)
        {
            var arg = ctx.Stack[sp];

            if(type.IsByRef)
            {
                sp--;
                type = type.GetElementType();
                if(arg.O is Pointer)
                {
                    var ptr = Pointer.Unbox(arg.O);
                    return new PointerRef(ptr);
                }
                if(arg.O is IReference) return (IReference) arg.O;
                return new PointerRef((void*) arg.U8);
            }
            if(Type.GetTypeCode(type) == TypeCode.String && arg.O == null)
            {
                arg.O = ctx.Instance.Data.LookupString(arg.U4);
                ctx.Stack[sp] = arg;
            }
            return new StackRef(sp--);
        }

        private static bool NeedTypedInvoke(VMContext ctx, uint sp, MethodBase method, bool isNewObj)
        {
            if(!isNewObj && !method.IsStatic)
                if(method.DeclaringType.IsValueType)
                    return true;
            foreach(var param in method.GetParameters())
                if(param.ParameterType.IsByRef)
                    return true;
            if(method is MethodInfo && ((MethodInfo) method).ReturnType.IsByRef)
                return true;
            return false;
        }

        private void InvokeNormal(VMContext ctx, MethodBase targetMethod, byte opCode, ref uint sp, out ExecutionState state)
        {
            var _sp = sp;
            var parameters = targetMethod.GetParameters();
            object self = null;
            var args = new object[parameters.Length];
            if(opCode == Constants.ECALL_CALL && targetMethod.IsVirtual)
            {
                var indexOffset = targetMethod.IsStatic ? 0 : 1;
                args = new object[parameters.Length + indexOffset];
                for(var i = parameters.Length - 1; i >= 0; i--)
                    args[i + indexOffset] = PopObject(ctx, parameters[i].ParameterType, ref sp);
                if(!targetMethod.IsStatic)
                    args[0] = PopObject(ctx, targetMethod.DeclaringType, ref sp);

                targetMethod = DirectCall.GetDirectInvocationProxy(targetMethod);
            }
            else
            {
                args = new object[parameters.Length];
                for(var i = parameters.Length - 1; i >= 0; i--)
                    args[i] = PopObject(ctx, parameters[i].ParameterType, ref sp);
                if(!targetMethod.IsStatic && opCode != Constants.ECALL_NEWOBJ)
                {
                    self = PopObject(ctx, targetMethod.DeclaringType, ref sp);

                    if(self != null && !targetMethod.DeclaringType.IsInstanceOfType(self))
                    {
                        // ConfuserEx sometimes produce this to circumvent peverify (see ref proxy)
                        // Reflection won't allow it, so use typed invoke
                        InvokeTyped(ctx, targetMethod, opCode, ref _sp, out state);
                        return;
                    }
                }
            }

            object result;
            if(opCode == Constants.ECALL_NEWOBJ)
            {
                try
                {
                    result = ((ConstructorInfo) targetMethod).Invoke(args);
                }
                catch(TargetInvocationException ex)
                {
                    EHHelper.Rethrow(ex.InnerException, null);
                    throw;
                }
            }
            else
            {
                if(!targetMethod.IsStatic && self == null)
                    throw new NullReferenceException();

                Type selfType;
                if(self != null && (selfType = self.GetType()).IsArray && targetMethod.Name == "SetValue")
                {
                    Type valueType;
                    if(args[0] == null)
                        valueType = selfType.GetElementType();
                    else
                        valueType = args[0].GetType();
                    ArrayStoreHelpers.SetValue((Array) self, (int) args[1], args[0], valueType, selfType.GetElementType());
                    result = null;
                }
                else
                {
                    try
                    {
                        result = targetMethod.Invoke(self, args);
                    }
                    catch(TargetInvocationException ex)
                    {
                        VMDispatcher.DoThrow(ctx, ex.InnerException);
                        throw;
                    }
                }
            }

            if(targetMethod is MethodInfo && ((MethodInfo) targetMethod).ReturnType != typeof(void)) ctx.Stack[++sp] = VMSlot.FromObject(result, ((MethodInfo) targetMethod).ReturnType);
            else if(opCode == Constants.ECALL_NEWOBJ) ctx.Stack[++sp] = VMSlot.FromObject(result, targetMethod.DeclaringType);

            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;
            state = ExecutionState.Next;
        }

        private void InvokeTyped(VMContext ctx, MethodBase targetMethod, byte opCode, ref uint sp, out ExecutionState state)
        {
            var parameters = targetMethod.GetParameters();
            var paramCount = parameters.Length;
            if(!targetMethod.IsStatic && opCode != Constants.ECALL_NEWOBJ)
                paramCount++;

            Type constrainType = null;
            if(opCode == Constants.ECALL_CALLVIRT_CONSTRAINED) constrainType = (Type) ctx.Instance.Data.LookupReference(ctx.Stack[sp--].U4);

            var indexOffset = targetMethod.IsStatic || opCode == Constants.ECALL_NEWOBJ ? 0 : 1;
            var references = new IReference[paramCount];
            var types = new Type[paramCount];
            for(var i = paramCount - 1; i >= 0; i--)
            {
                Type paramType;
                if(!targetMethod.IsStatic && opCode != Constants.ECALL_NEWOBJ)
                    if(i == 0)
                    {
                        if(!targetMethod.IsStatic)
                        {
                            var thisSlot = ctx.Stack[sp];
                            if(thisSlot.O is ValueType && !targetMethod.DeclaringType.IsValueType)
                            {
                                Debug.Assert(targetMethod.DeclaringType.IsInterface);
                                Debug.Assert(opCode == Constants.ECALL_CALLVIRT);
                                // Interface dispatch on valuetypes => use constrained. invocation
                                constrainType = thisSlot.O.GetType();
                            }
                        }

                        if(constrainType != null)
                            paramType = constrainType.MakeByRefType();
                        else if(targetMethod.DeclaringType.IsValueType)
                            paramType = targetMethod.DeclaringType.MakeByRefType();
                        else
                            paramType = targetMethod.DeclaringType;
                    }
                    else
                    {
                        paramType = parameters[i - 1].ParameterType;
                    }
                else paramType = parameters[i].ParameterType;
                references[i] = PopRef(ctx, paramType, ref sp);
                if(paramType.IsByRef)
                    paramType = paramType.GetElementType();
                types[i] = paramType;
            }

            OpCode callOp;
            Type retType;
            if(opCode == Constants.ECALL_CALL)
            {
                callOp = System.Reflection.Emit.OpCodes.Call;
                retType = targetMethod is MethodInfo ? ((MethodInfo) targetMethod).ReturnType : typeof(void);
            }
            else if(opCode == Constants.ECALL_CALLVIRT ||
                    opCode == Constants.ECALL_CALLVIRT_CONSTRAINED)
            {
                callOp = System.Reflection.Emit.OpCodes.Callvirt;
                retType = targetMethod is MethodInfo ? ((MethodInfo) targetMethod).ReturnType : typeof(void);
            }
            else if(opCode == Constants.ECALL_NEWOBJ)
            {
                callOp = System.Reflection.Emit.OpCodes.Newobj;
                retType = targetMethod.DeclaringType;
            }
            else
            {
                throw new InvalidProgramException();
            }
            var proxy = DirectCall.GetTypedInvocationProxy(targetMethod, callOp, constrainType);

            var result = proxy(ctx, references, types);

            if(retType != typeof(void)) ctx.Stack[++sp] = VMSlot.FromObject(result, retType);
            else if(opCode == Constants.ECALL_NEWOBJ) ctx.Stack[++sp] = VMSlot.FromObject(result, retType);

            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[Constants.REG_SP].U4 = sp;
            state = ExecutionState.Next;
        }
    }
}