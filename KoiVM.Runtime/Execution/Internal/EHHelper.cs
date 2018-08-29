#region

using System;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace KoiVM.Runtime.Execution.Internal
{
    internal class EHHelper
    {
        private static Throw rethrow;

        private static readonly object RethrowKey = new object();

        static EHHelper()
        {
            if(BuildInternalPreserve(typeof(Exception)))
                return;
            var type = Type.GetType("System.Runtime.ExceptionServices.ExceptionDispatchInfo");
            if(type != null && BuildExceptionDispatchInfo(type))
                return;
            rethrow = null;
        }

        private static bool BuildExceptionDispatchInfo(Type type)
        {
            try
            {
                var capture = type.GetMethod("Capture");
                var thr = type.GetMethod("Throw");

                var dm = new DynamicMethod("", typeof(void), new[] {typeof(Exception), typeof(string), typeof(bool)});
                var ilGen = dm.GetILGenerator();
                ilGen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Call, capture);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Call, thr);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Ret);

                rethrow = (Throw) dm.CreateDelegate(typeof(Throw));
            }
            catch
            {
                return false;
            }
            return true;
        }

        private static bool BuildInternalPreserve(Type type)
        {
            try
            {
                const BindingFlags fl = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
                var at = (string) typeof(Environment).InvokeMember("GetResourceString", fl, null, null, new object[] {"Word_At"});

                var preserve = type.GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);
                var field = type.GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);
                var stackTrace = type.GetProperty("StackTrace", BindingFlags.Instance | BindingFlags.Public).GetGetMethod();
                var fmt = typeof(string).GetMethod("Format", new[] {typeof(string), typeof(object), typeof(object)});

                var dm = new DynamicMethod("", typeof(void), new[] {typeof(Exception), typeof(string), typeof(bool)}, true);
                var ilGen = dm.GetILGenerator();
                var lbl = ilGen.DefineLabel();
                var lbl2 = ilGen.DefineLabel();
                var lbl3 = ilGen.DefineLabel();

                ilGen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);

                ilGen.Emit(System.Reflection.Emit.OpCodes.Dup);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Dup);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Ldfld, field);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Brtrue, lbl2);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Callvirt, stackTrace);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Br, lbl3);
                ilGen.MarkLabel(lbl2);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Ldfld, field);
                ilGen.MarkLabel(lbl3);

                ilGen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);

                ilGen.Emit(System.Reflection.Emit.OpCodes.Call, preserve);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Stfld, field);

                ilGen.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Brfalse, lbl);

                ilGen.Emit(System.Reflection.Emit.OpCodes.Ldarg_2);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Brtrue, lbl);

                ilGen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);

                ilGen.Emit(System.Reflection.Emit.OpCodes.Dup);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Ldstr,
                    "{1}" + Environment.NewLine + "   " + at + " KoiVM [{0}]" + Environment.NewLine);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Ldfld, field);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Call, fmt);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Stfld, field);


                ilGen.Emit(System.Reflection.Emit.OpCodes.Throw);

                ilGen.MarkLabel(lbl);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                ilGen.Emit(System.Reflection.Emit.OpCodes.Throw);

                rethrow = (Throw) dm.CreateDelegate(typeof(Throw));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }

        public static void Rethrow(Exception ex, string tokens)
        {
            if(tokens == null)
                throw ex;

            var r = ex.Data.Contains(RethrowKey);
            if(!r)
                ex.Data[RethrowKey] = RethrowKey;

            if(rethrow != null)
                rethrow(ex, tokens, r);
            throw ex;
        }

        private delegate void Throw(Exception ex, string ip, bool rethrow);
    }
}