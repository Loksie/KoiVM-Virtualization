#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace KoiVM.Runtime.Execution.Internal
{
    internal static class DirectCall
    {
        public delegate object TypedInvocation(VMContext ctx, IReference[] refs, Type[] types);

        private static readonly Hashtable directProxies = new Hashtable();
        private static readonly Hashtable typedProxies = new Hashtable();
        private static readonly Hashtable constrainedProxies = new Hashtable();
        private static readonly MethodInfo refToTypedRef;
        private static readonly MethodInfo castTypedRef;
        private static readonly ConstructorInfo newTypedRef;

        static DirectCall()
        {
            foreach(var method in typeof(IReference).GetMethods())
            {
                foreach(var param in method.GetParameters())
                    if(param.ParameterType == typeof(TypedRefPtr))
                    {
                        refToTypedRef = method;
                        break;
                    }
                if(refToTypedRef != null)
                    break;
            }
            foreach(var method in typeof(TypedReferenceHelpers).GetMethods())
                if(method.GetParameters()[0].ParameterType == typeof(TypedRefPtr))
                {
                    castTypedRef = method;
                    break;
                }
            foreach(var ctor in typeof(TypedRef).GetConstructors())
            {
                foreach(var param in ctor.GetParameters())
                    if(param.ParameterType == typeof(TypedReference))
                    {
                        newTypedRef = ctor;
                        break;
                    }
                if(newTypedRef != null)
                    break;
            }
        }

        public static MethodBase GetDirectInvocationProxy(MethodBase method)
        {
            var proxy = (MethodBase) directProxies[method];
            if(proxy != null)
                return proxy;

            lock(directProxies)
            {
                proxy = (MethodBase) directProxies[method];
                if(proxy != null)
                    return proxy;

                var parameters = method.GetParameters();
                var paramTypes = new Type[parameters.Length + (method.IsStatic ? 0 : 1)];
                for(var i = 0; i < paramTypes.Length; i++)
                    if(method.IsStatic)
                    {
                        paramTypes[i] = parameters[i].ParameterType;
                    }
                    else
                    {
                        if(i == 0)
                            paramTypes[0] = method.DeclaringType;
                        else
                            paramTypes[i] = parameters[i - 1].ParameterType;
                    }

                var retType = method is MethodInfo ? ((MethodInfo) method).ReturnType : typeof(void);
                var dm = new DynamicMethod("", retType, paramTypes, Unverifier.Module, true);
                var gen = dm.GetILGenerator();
                for(var i = 0; i < paramTypes.Length; i++)
                    if(!method.IsStatic && i == 0 && paramTypes[0].IsValueType)
                        gen.Emit(System.Reflection.Emit.OpCodes.Ldarga, i);
                    else
                        gen.Emit(System.Reflection.Emit.OpCodes.Ldarg, i);
                if(method is MethodInfo)
                    gen.Emit(System.Reflection.Emit.OpCodes.Call, (MethodInfo) method);
                else
                    gen.Emit(System.Reflection.Emit.OpCodes.Call, (ConstructorInfo) method);
                gen.Emit(System.Reflection.Emit.OpCodes.Ret);

                directProxies[method] = dm;
                return dm;
            }
        }

        public static TypedInvocation GetTypedInvocationProxy(MethodBase method, OpCode opCode, Type constrainType)
        {
            Hashtable table;
            object key;
            if(constrainType == null)
            {
                key = new KeyValuePair<MethodBase, OpCode>(method, opCode);
                table = typedProxies;
            }
            else
            {
                key = new KeyValuePair<MethodBase, Type>(method, constrainType);
                table = constrainedProxies;
            }

            var proxy = (TypedInvocation) table[key];
            if(proxy != null)
                return proxy;

            lock(typedProxies)
            {
                proxy = (TypedInvocation) table[key];
                if(proxy != null)
                    return proxy;

                var parameters = method.GetParameters();
                Type[] paramTypes;
                if(opCode != System.Reflection.Emit.OpCodes.Newobj)
                {
                    paramTypes = new Type[parameters.Length + (method.IsStatic ? 0 : 1) + 1];
                    for(var i = 0; i < paramTypes.Length - 1; i++)
                        if(method.IsStatic)
                        {
                            paramTypes[i] = parameters[i].ParameterType;
                        }
                        else
                        {
                            if(i == 0)
                                if(constrainType != null)
                                    paramTypes[0] = constrainType.MakeByRefType();
                                else if(method.DeclaringType.IsValueType)
                                    paramTypes[0] = method.DeclaringType.MakeByRefType();
                                else
                                    paramTypes[0] = method.DeclaringType;
                            else
                                paramTypes[i] = parameters[i - 1].ParameterType;
                        }
                }
                else
                {
                    paramTypes = new Type[parameters.Length + 1];
                    for(var i = 0; i < paramTypes.Length - 1; i++) paramTypes[i] = parameters[i].ParameterType;
                }

                var retType = method is MethodInfo ? ((MethodInfo) method).ReturnType : typeof(void);
                if(opCode == System.Reflection.Emit.OpCodes.Newobj)
                    retType = method.DeclaringType;
                var dm = new DynamicMethod("", typeof(object), new[] {typeof(VMContext), typeof(IReference[]), typeof(Type[])},
                    Unverifier.Module, true);
                var gen = dm.GetILGenerator();

                for(var i = 0; i < paramTypes.Length - 1; i++)
                {
                    var paramType = paramTypes[i];
                    var isByRef = paramType.IsByRef;
                    if(isByRef)
                        paramType = paramType.GetElementType();

                    var typedRefLocal = gen.DeclareLocal(typeof(TypedReference));
                    gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                    gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, i);
                    gen.Emit(System.Reflection.Emit.OpCodes.Ldelem_Ref);

                    gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                    gen.Emit(System.Reflection.Emit.OpCodes.Ldloca, typedRefLocal);

                    gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_2);
                    gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, i);
                    gen.Emit(System.Reflection.Emit.OpCodes.Ldelem_Ref);

                    gen.Emit(System.Reflection.Emit.OpCodes.Callvirt, refToTypedRef);

                    gen.Emit(System.Reflection.Emit.OpCodes.Ldloca, typedRefLocal);
                    gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_2);
                    gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, i);
                    gen.Emit(System.Reflection.Emit.OpCodes.Ldelem_Ref);
                    gen.Emit(System.Reflection.Emit.OpCodes.Call, castTypedRef);

                    gen.Emit(System.Reflection.Emit.OpCodes.Ldloc, typedRefLocal);
                    gen.Emit(System.Reflection.Emit.OpCodes.Refanyval, paramType);

                    if(!isByRef)
                        gen.Emit(System.Reflection.Emit.OpCodes.Ldobj, paramType);
                }

                if(constrainType != null)
                    gen.Emit(System.Reflection.Emit.OpCodes.Constrained, constrainType);

                if(method is MethodInfo)
                    gen.Emit(opCode, (MethodInfo) method);
                else
                    gen.Emit(opCode, (ConstructorInfo) method);

                if(retType.IsByRef)
                {
                    gen.Emit(System.Reflection.Emit.OpCodes.Mkrefany, retType.GetElementType());
                    gen.Emit(System.Reflection.Emit.OpCodes.Newobj, newTypedRef);
                }
                else if(retType == typeof(void))
                {
                    gen.Emit(System.Reflection.Emit.OpCodes.Ldnull);
                }
                else if(retType.IsValueType)
                {
                    gen.Emit(System.Reflection.Emit.OpCodes.Box, retType);
                }

                gen.Emit(System.Reflection.Emit.OpCodes.Ret);

                proxy = (TypedInvocation) dm.CreateDelegate(typeof(TypedInvocation));
                table[key] = proxy;
                return proxy;
            }
        }
    }
}