#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using KoiVM.Runtime.Data;
using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;
using KoiVM.Runtime.Execution.Internal;

#endregion

namespace KoiVM.Runtime
{
    internal unsafe class VMInstance
    {
        [ThreadStatic] private static Dictionary<Module, VMInstance> instances;
        private static readonly object initLock = new object();
        private static readonly Dictionary<Module, int> initialized = new Dictionary<Module, int>();

        private readonly Stack<VMContext> ctxStack = new Stack<VMContext>();
        private VMContext currentCtx;

        private VMInstance(VMData data)
        {
            Data = data;
        }

        public VMData Data
        {
            get;
        }

        public static VMInstance Instance(Module module)
        {
            VMInstance inst;
            if(instances == null) instances = new Dictionary<Module, VMInstance>();
            if(!instances.TryGetValue(module, out inst))
            {
                inst = new VMInstance(VMData.Instance(module));
                instances[module] = inst;
                lock(initLock)
                {
                    if(!initialized.ContainsKey(module))
                    {
                        inst.Initialize();
                        initialized.Add(module, initialized.Count);
                    }
                }
            }
            return inst;
        }

        public static VMInstance Instance(int id)
        {
            foreach(var entry in initialized)
                if(entry.Value == id)
                    return Instance(entry.Key);
            return null;
        }

        public static int GetModuleId(Module module)
        {
            return initialized[module];
        }

        private void Initialize()
        {
            var initFunc = Data.LookupExport(Constants.HELPER_INIT);
            var codeAddr = (ulong) (Data.KoiSection + initFunc.CodeOffset);
            Run(codeAddr, initFunc.EntryKey, initFunc.Signature, new object[0]);
        }

        public object Run(uint id, object[] arguments)
        {
            var export = Data.LookupExport(id);
            var codeAddr = (ulong) (Data.KoiSection + export.CodeOffset);
            return Run(codeAddr, export.EntryKey, export.Signature, arguments);
        }

        public object Run(ulong codeAddr, uint key, uint sigId, object[] arguments)
        {
            var sig = Data.LookupExport(sigId).Signature;
            return Run(codeAddr, key, sig, arguments);
        }

        public void Run(uint id, void*[] typedRefs, void* retTypedRef)
        {
            var export = Data.LookupExport(id);
            var codeAddr = (ulong) (Data.KoiSection + export.CodeOffset);
            Run(codeAddr, export.EntryKey, export.Signature, typedRefs, retTypedRef);
        }

        public void Run(ulong codeAddr, uint key, uint sigId, void*[] typedRefs, void* retTypedRef)
        {
            var sig = Data.LookupExport(sigId).Signature;
            Run(codeAddr, key, sig, typedRefs, retTypedRef);
        }

        private object Run(ulong codeAddr, uint key, VMFuncSig sig, object[] arguments)
        {
            if(currentCtx != null)
                ctxStack.Push(currentCtx);
            currentCtx = new VMContext(this);

            try
            {
                Debug.Assert(sig.ParamTypes.Length == arguments.Length);
                currentCtx.Stack.SetTopPosition((uint) arguments.Length + 1);
                for(uint i = 0; i < arguments.Length; i++) currentCtx.Stack[i + 1] = VMSlot.FromObject(arguments[i], sig.ParamTypes[i]);
                currentCtx.Stack[(uint) arguments.Length + 1] = new VMSlot {U8 = 1};

                currentCtx.Registers[Constants.REG_K1] = new VMSlot {U4 = key};
                currentCtx.Registers[Constants.REG_BP] = new VMSlot {U4 = 0};
                currentCtx.Registers[Constants.REG_SP] = new VMSlot {U4 = (uint) arguments.Length + 1};
                currentCtx.Registers[Constants.REG_IP] = new VMSlot {U8 = codeAddr};
                VMDispatcher.Run(currentCtx);
                Debug.Assert(currentCtx.EHStack.Count == 0);

                object retVal = null;
                if(sig.RetType != typeof(void))
                {
                    var retSlot = currentCtx.Registers[Constants.REG_R0];
                    if(Type.GetTypeCode(sig.RetType) == TypeCode.String && retSlot.O == null)
                        retVal = Data.LookupString(retSlot.U4);
                    else
                        retVal = retSlot.ToObject(sig.RetType);
                }

                return retVal;
            }
            finally
            {
                currentCtx.Stack.FreeAllLocalloc();

                if(ctxStack.Count > 0)
                    currentCtx = ctxStack.Pop();
            }
        }

        private void Run(ulong codeAddr, uint key, VMFuncSig sig, void*[] arguments, void* retTypedRef)
        {
            if(currentCtx != null)
                ctxStack.Push(currentCtx);
            currentCtx = new VMContext(this);

            try
            {
                Debug.Assert(sig.ParamTypes.Length == arguments.Length);
                currentCtx.Stack.SetTopPosition((uint) arguments.Length + 1);
                for(uint i = 0; i < arguments.Length; i++)
                {
                    var paramType = sig.ParamTypes[i];
                    if(paramType.IsByRef)
                    {
                        currentCtx.Stack[i + 1] = new VMSlot {O = new TypedRef(arguments[i])};
                    }
                    else
                    {
                        var typedRef = *(TypedReference*) arguments[i];
                        currentCtx.Stack[i + 1] = VMSlot.FromObject(TypedReference.ToObject(typedRef), __reftype(typedRef));
                    }
                }
                currentCtx.Stack[(uint) arguments.Length + 1] = new VMSlot {U8 = 1};

                currentCtx.Registers[Constants.REG_K1] = new VMSlot {U4 = key};
                currentCtx.Registers[Constants.REG_BP] = new VMSlot {U4 = 0};
                currentCtx.Registers[Constants.REG_SP] = new VMSlot {U4 = (uint) arguments.Length + 1};
                currentCtx.Registers[Constants.REG_IP] = new VMSlot {U8 = codeAddr};
                VMDispatcher.Run(currentCtx);
                Debug.Assert(currentCtx.EHStack.Count == 0);

                if(sig.RetType != typeof(void))
                    if(sig.RetType.IsByRef)
                    {
                        var retRef = currentCtx.Registers[Constants.REG_R0].O;
                        if(!(retRef is IReference))
                            throw new ExecutionEngineException();
                        ((IReference) retRef).ToTypedReference(currentCtx, retTypedRef, sig.RetType.GetElementType());
                    }
                    else
                    {
                        var retSlot = currentCtx.Registers[Constants.REG_R0];
                        object retVal;
                        if(Type.GetTypeCode(sig.RetType) == TypeCode.String && retSlot.O == null)
                            retVal = Data.LookupString(retSlot.U4);
                        else
                            retVal = retSlot.ToObject(sig.RetType);
                        TypedReferenceHelpers.SetTypedRef(retVal, retTypedRef);
                    }
            }
            finally
            {
                currentCtx.Stack.FreeAllLocalloc();

                if(ctxStack.Count > 0)
                    currentCtx = ctxStack.Pop();
            }
        }
    }
}