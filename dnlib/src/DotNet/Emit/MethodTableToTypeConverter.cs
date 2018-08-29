#region

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using SR = System.Reflection;

#endregion

namespace dnlib.DotNet.Emit
{
    /// <summary>
    ///     Converts a type address to a <see cref="Type" />. The address can be found in
    ///     <c>RuntimeTypeHandle.Value</c> and it's the same address you use with the WinDbg SOS command
    ///     !dumpmt.
    /// </summary>
    internal static class MethodTableToTypeConverter
    {
        private const string METHOD_NAME = "m";
        private static readonly SR.MethodInfo setMethodBodyMethodInfo = typeof(MethodBuilder).GetMethod("SetMethodBody", SR.BindingFlags.DeclaredOnly | SR.BindingFlags.Public | SR.BindingFlags.NonPublic | SR.BindingFlags.Instance);
        private static readonly SR.FieldInfo localSignatureFieldInfo = typeof(ILGenerator).GetField("m_localSignature", SR.BindingFlags.DeclaredOnly | SR.BindingFlags.Public | SR.BindingFlags.NonPublic | SR.BindingFlags.Instance);
        private static readonly SR.FieldInfo sigDoneFieldInfo = typeof(SignatureHelper).GetField("m_sigDone", SR.BindingFlags.DeclaredOnly | SR.BindingFlags.Public | SR.BindingFlags.NonPublic | SR.BindingFlags.Instance);
        private static readonly SR.FieldInfo currSigFieldInfo = typeof(SignatureHelper).GetField("m_currSig", SR.BindingFlags.DeclaredOnly | SR.BindingFlags.Public | SR.BindingFlags.NonPublic | SR.BindingFlags.Instance);
        private static readonly SR.FieldInfo signatureFieldInfo = typeof(SignatureHelper).GetField("m_signature", SR.BindingFlags.DeclaredOnly | SR.BindingFlags.Public | SR.BindingFlags.NonPublic | SR.BindingFlags.Instance);
        private static readonly SR.FieldInfo ptrFieldInfo = typeof(RuntimeTypeHandle).GetField("m_ptr", SR.BindingFlags.DeclaredOnly | SR.BindingFlags.Instance | SR.BindingFlags.NonPublic | SR.BindingFlags.Public);
        private static readonly Dictionary<IntPtr, Type> addrToType = new Dictionary<IntPtr, Type>();
        private static ModuleBuilder moduleBuilder;
        private static int numNewTypes;
#if THREAD_SAFE
		static readonly Lock theLock = Lock.Create();
#endif

        static MethodTableToTypeConverter()
        {
            if(ptrFieldInfo == null)
            {
                var asmb = AppDomain.CurrentDomain.DefineDynamicAssembly(new SR.AssemblyName("DynAsm"), AssemblyBuilderAccess.Run);
                moduleBuilder = asmb.DefineDynamicModule("DynMod");
            }
        }

        /// <summary>
        ///     Converts <paramref name="address" /> to a <see cref="Type" />.
        /// </summary>
        /// <param name="address">Address of type</param>
        /// <returns>The <see cref="Type" /> or <c>null</c></returns>
        public static Type Convert(IntPtr address)
        {
            Type type;
#if THREAD_SAFE
			theLock.EnterWriteLock(); try {
#endif
            if(addrToType.TryGetValue(address, out type))
                return type;

            type = GetTypeNET20(address) ?? GetTypeUsingTypeBuilder(address);
            addrToType[address] = type;
            return type;
#if THREAD_SAFE
			} finally { theLock.ExitWriteLock(); }
#endif
        }

        private static Type GetTypeUsingTypeBuilder(IntPtr address)
        {
            if(moduleBuilder == null)
                return null;

            var tb = moduleBuilder.DefineType(GetNextTypeName());
            var mb = tb.DefineMethod(METHOD_NAME, SR.MethodAttributes.Static, typeof(void), new Type[0]);

            try
            {
                if(setMethodBodyMethodInfo != null)
                    return GetTypeNET45(tb, mb, address);
                return GetTypeNET40(tb, mb, address);
            }
            catch
            {
                moduleBuilder = null;
                return null;
            }
        }

        // .NET 4.5 and later have the documented SetMethodBody() method.
        private static Type GetTypeNET45(TypeBuilder tb, MethodBuilder mb, IntPtr address)
        {
            var code = new byte[1] {0x2A};
            var maxStack = 8;
            var locals = GetLocalSignature(address);
            setMethodBodyMethodInfo.Invoke(mb, new object[5] {code, maxStack, locals, null, null});

            var createdMethod = tb.CreateType().GetMethod(METHOD_NAME, SR.BindingFlags.DeclaredOnly | SR.BindingFlags.Public | SR.BindingFlags.NonPublic | SR.BindingFlags.Static | SR.BindingFlags.Instance);
            return createdMethod.GetMethodBody().LocalVariables[0].LocalType;
        }

        // This code works with .NET 4.0+ but will throw an exception if .NET 2.0 is used
        // ("operation could destabilize the runtime")
        private static Type GetTypeNET40(TypeBuilder tb, MethodBuilder mb, IntPtr address)
        {
            var ilg = mb.GetILGenerator();
            ilg.Emit(System.Reflection.Emit.OpCodes.Ret);

            // We need at least one local to make sure the SignatureHelper from ILGenerator is used.
            ilg.DeclareLocal(typeof(int));

            var locals = GetLocalSignature(address);
            var sigHelper = (SignatureHelper) localSignatureFieldInfo.GetValue(ilg);
            sigDoneFieldInfo.SetValue(sigHelper, true);
            currSigFieldInfo.SetValue(sigHelper, locals.Length);
            signatureFieldInfo.SetValue(sigHelper, locals);

            var createdMethod = tb.CreateType().GetMethod(METHOD_NAME, SR.BindingFlags.DeclaredOnly | SR.BindingFlags.Public | SR.BindingFlags.NonPublic | SR.BindingFlags.Static | SR.BindingFlags.Instance);
            return createdMethod.GetMethodBody().LocalVariables[0].LocalType;
        }

        // .NET 2.0 - 3.5
        private static Type GetTypeNET20(IntPtr address)
        {
            if(ptrFieldInfo == null)
                return null;
            object th = new RuntimeTypeHandle();
            ptrFieldInfo.SetValue(th, address);
            return Type.GetTypeFromHandle((RuntimeTypeHandle) th);
        }

        private static string GetNextTypeName()
        {
            return string.Format("Type{0}", numNewTypes++);
        }

        private static byte[] GetLocalSignature(IntPtr mtAddr)
        {
            var mtValue = (ulong) mtAddr.ToInt64();
            if(IntPtr.Size == 4)
                return new byte[]
                {
                    0x07,
                    0x01,
                    (byte) ElementType.Internal,
                    (byte) mtValue,
                    (byte) (mtValue >> 8),
                    (byte) (mtValue >> 16),
                    (byte) (mtValue >> 24)
                };
            return new byte[]
            {
                0x07,
                0x01,
                (byte) ElementType.Internal,
                (byte) mtValue,
                (byte) (mtValue >> 8),
                (byte) (mtValue >> 16),
                (byte) (mtValue >> 24),
                (byte) (mtValue >> 32),
                (byte) (mtValue >> 40),
                (byte) (mtValue >> 48),
                (byte) (mtValue >> 56)
            };
        }
    }
}