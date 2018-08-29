#region

using System;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Confuser.Runtime
{
    internal class RefProxyKey : Attribute
    {
        private readonly int key;

        public RefProxyKey(int key)
        {
            this.key = Mutation.Placeholder(key);
        }

        public override int GetHashCode()
        {
            return key;
        }
    }

    internal static class RefProxyStrong
    {
        internal static void Initialize(RuntimeFieldHandle field, byte opKey)
        {
            var fieldInfo = FieldInfo.GetFieldFromHandle(field);
            var sig = fieldInfo.Module.ResolveSignature(fieldInfo.MetadataToken);
            var len = sig.Length;
            var key = fieldInfo.GetOptionalCustomModifiers()[0].MetadataToken;

            key += (fieldInfo.Name[Mutation.KeyI0] ^ sig[--len]) << Mutation.KeyI4;
            key += (fieldInfo.Name[Mutation.KeyI1] ^ sig[--len]) << Mutation.KeyI5;
            key += (fieldInfo.Name[Mutation.KeyI2] ^ sig[--len]) << Mutation.KeyI6;
            len--;
            key += (fieldInfo.Name[Mutation.KeyI3] ^ sig[--len]) << Mutation.KeyI7;

            var token = Mutation.Placeholder(key);
            token *= fieldInfo.GetCustomAttributes(false)[0].GetHashCode();

            var method = fieldInfo.Module.ResolveMethod(token);
            var delegateType = fieldInfo.FieldType;
            if(method.IsStatic)
            {
                fieldInfo.SetValue(null, Delegate.CreateDelegate(delegateType, (MethodInfo) method));
            }

            else
            {
                DynamicMethod dm = null;
                Type[] argTypes = null;

                foreach(var invoke in fieldInfo.FieldType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
                    if(invoke.DeclaringType == delegateType)
                    {
                        var paramTypes = invoke.GetParameters();
                        argTypes = new Type[paramTypes.Length];
                        for(var i = 0; i < argTypes.Length; i++)
                            argTypes[i] = paramTypes[i].ParameterType;

                        var declType = method.DeclaringType;
                        dm = new DynamicMethod("", invoke.ReturnType, argTypes, declType.IsInterface || declType.IsArray ? delegateType : declType, true);
                        break;
                    }

                var info = dm.GetDynamicILInfo();
                info.SetLocalSignature(new byte[] {0x7, 0x0});
                var code = new byte[(2 + 5) * argTypes.Length + 6];
                var index = 0;
                var mParams = method.GetParameters();
                var mIndex = method.IsConstructor ? 0 : -1;
                for(var i = 0; i < argTypes.Length; i++)
                {
                    code[index++] = 0x0e;
                    code[index++] = (byte) i;

                    var mType = mIndex == -1 ? method.DeclaringType : mParams[mIndex].ParameterType;
                    if(mType.IsClass && !(mType.IsPointer || mType.IsByRef))
                    {
                        var cToken = info.GetTokenFor(mType.TypeHandle);
                        code[index++] = 0x74;
                        code[index++] = (byte) cToken;
                        code[index++] = (byte) (cToken >> 8);
                        code[index++] = (byte) (cToken >> 16);
                        code[index++] = (byte) (cToken >> 24);
                    }
                    else
                    {
                        index += 5;
                    }
                    mIndex++;
                }
                code[index++] = (byte) ((byte) fieldInfo.Name[Mutation.KeyI8] ^ opKey);
                var dmToken = info.GetTokenFor(method.MethodHandle);
                code[index++] = (byte) dmToken;
                code[index++] = (byte) (dmToken >> 8);
                code[index++] = (byte) (dmToken >> 16);
                code[index++] = (byte) (dmToken >> 24);
                code[index] = 0x2a;
                info.SetCode(code, argTypes.Length + 1);

                fieldInfo.SetValue(null, dm.CreateDelegate(delegateType));
            }
        }
    }
}