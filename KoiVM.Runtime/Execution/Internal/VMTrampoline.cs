#region

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using KoiVM.Runtime.Data;

#endregion

namespace KoiVM.Runtime.Execution.Internal
{
    internal static class VMTrampoline
    {
        private static readonly GetMethodDescriptor getDesc;
        private static readonly MethodInfo entryStubNormal;
        private static readonly MethodInfo entryStubTyped;

        private static readonly Hashtable trampolines = new Hashtable();

        static VMTrampoline()
        {
            foreach(var method in typeof(VMEntry).GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
                if(method.ReturnType != typeof(void) && method.GetParameters().Length > 4)
                    entryStubNormal = method;
                else
                    entryStubTyped = method;
            getDesc = (GetMethodDescriptor) Delegate.CreateDelegate(typeof(GetMethodDescriptor),
                typeof(DynamicMethod).GetMethod("GetMethodDescriptor", BindingFlags.Instance | BindingFlags.NonPublic));
        }

        public static IntPtr CreateTrampoline(Module module, ulong codeAdr, uint key, VMFuncSig sig, uint sigId)
        {
            var dm = trampolines[codeAdr];
            if(dm != null)
                return getDesc((DynamicMethod) dm).GetFunctionPointer();

            lock(trampolines)
            {
                dm = (DynamicMethod) trampolines[codeAdr];
                if(dm != null)
                    return getDesc((DynamicMethod) dm).GetFunctionPointer();

                if(ShouldBeTyped(sig))
                    dm = CreateTrampolineTyped(VMInstance.GetModuleId(module), codeAdr, key, sig, sigId);
                else
                    dm = CreateTrampolineNormal(VMInstance.GetModuleId(module), codeAdr, key, sig, sigId);
                trampolines[codeAdr] = dm;
                return getDesc((DynamicMethod) dm).GetFunctionPointer();
            }
        }

        private static bool ShouldBeTyped(VMFuncSig sig)
        {
            foreach(var param in sig.ParamTypes)
                if(param.IsByRef)
                    return true;
            return sig.RetType.IsByRef;
        }

        private static DynamicMethod CreateTrampolineNormal(int moduleId, ulong codeAdr, uint key, VMFuncSig sig, uint sigId)
        {
            var dm = new DynamicMethod("", sig.RetType, sig.ParamTypes, Unverifier.Module, true);

            var gen = dm.GetILGenerator();
            gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, moduleId);
            gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I8, (long) codeAdr);
            gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, (int) key);
            gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, (int) sigId);
            gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, sig.ParamTypes.Length);
            gen.Emit(System.Reflection.Emit.OpCodes.Newarr, typeof(object));
            for(var i = 0; i < sig.ParamTypes.Length; i++)
            {
                gen.Emit(System.Reflection.Emit.OpCodes.Dup);
                gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, i);
                gen.Emit(System.Reflection.Emit.OpCodes.Ldarg, i);
                if(sig.ParamTypes[i].IsValueType)
                    gen.Emit(System.Reflection.Emit.OpCodes.Box, sig.ParamTypes[i]);
                gen.Emit(System.Reflection.Emit.OpCodes.Stelem_Ref);
            }

            gen.Emit(System.Reflection.Emit.OpCodes.Call, entryStubNormal);

            if(sig.RetType == typeof(void))
                gen.Emit(System.Reflection.Emit.OpCodes.Pop);
            else if(sig.RetType.IsValueType)
                gen.Emit(System.Reflection.Emit.OpCodes.Unbox_Any, sig.RetType);
            else
                gen.Emit(System.Reflection.Emit.OpCodes.Castclass, sig.RetType);

            gen.Emit(System.Reflection.Emit.OpCodes.Ret);

            return dm;
        }

        private static DynamicMethod CreateTrampolineTyped(int moduleId, ulong codeAdr, uint key, VMFuncSig sig, uint sigId)
        {
            var dm = new DynamicMethod("", sig.RetType, sig.ParamTypes, Unverifier.Module, true);

            var gen = dm.GetILGenerator();
            gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, moduleId);
            gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I8, (long) codeAdr);
            gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, (int) key);
            gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, (int) sigId);
            gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, sig.ParamTypes.Length);
            gen.Emit(System.Reflection.Emit.OpCodes.Newarr, typeof(void*));
            for(var i = 0; i < sig.ParamTypes.Length; i++)
            {
                gen.Emit(System.Reflection.Emit.OpCodes.Dup);
                gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, i);
                if(sig.ParamTypes[i].IsByRef)
                {
                    gen.Emit(System.Reflection.Emit.OpCodes.Ldarg, i);
                    gen.Emit(System.Reflection.Emit.OpCodes.Mkrefany, sig.ParamTypes[i].GetElementType());
                }
                else
                {
                    gen.Emit(System.Reflection.Emit.OpCodes.Ldarga, i);
                    gen.Emit(System.Reflection.Emit.OpCodes.Mkrefany, sig.ParamTypes[i]);
                }
                var local = gen.DeclareLocal(typeof(TypedReference));
                gen.Emit(System.Reflection.Emit.OpCodes.Stloc, local);
                gen.Emit(System.Reflection.Emit.OpCodes.Ldloca, local);
                gen.Emit(System.Reflection.Emit.OpCodes.Conv_I);
                gen.Emit(System.Reflection.Emit.OpCodes.Stelem_I);
            }

            if(sig.RetType != typeof(void))
            {
                var retVar = gen.DeclareLocal(sig.RetType);
                var retRef = gen.DeclareLocal(typeof(TypedReference));
                gen.Emit(System.Reflection.Emit.OpCodes.Ldloca, retVar);
                gen.Emit(System.Reflection.Emit.OpCodes.Mkrefany, sig.RetType);
                gen.Emit(System.Reflection.Emit.OpCodes.Stloc, retRef);
                gen.Emit(System.Reflection.Emit.OpCodes.Ldloca, retRef);
                gen.Emit(System.Reflection.Emit.OpCodes.Call, entryStubTyped);

                gen.Emit(System.Reflection.Emit.OpCodes.Ldloc, retVar);
            }
            else
            {
                gen.Emit(System.Reflection.Emit.OpCodes.Ldnull);
                gen.Emit(System.Reflection.Emit.OpCodes.Call, entryStubTyped);
            }
            gen.Emit(System.Reflection.Emit.OpCodes.Ret);

            return dm;
        }

        private delegate RuntimeMethodHandle GetMethodDescriptor(DynamicMethod dm);
    }
}