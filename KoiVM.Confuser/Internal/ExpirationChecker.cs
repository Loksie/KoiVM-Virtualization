#region

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

#endregion

namespace KoiVM.Confuser.Internal
{
    [Obfuscation(Exclude = false, Feature = "+koi;")]
    internal class ExpirationChecker
    {
        private static Thread thread;

        internal static void Init(string koiDir)
        {
            if(thread != null)
                return;
            thread = Thread.CurrentThread;
            new Thread(() => DoCheck(Check(koiDir))).Start();
        }

        private static void DoCheck(IEnumerable q)
        {
            foreach(var x in q)
                Thread.Yield();
        }

        private static uint Hash(string name)
        {
            uint hash = 0;
            for(var i = 0; i < name.Length; i++)
                hash = name[i] + (hash << 6) + (hash << 16) - hash;
            return hash;
        }

        private static IEnumerable Check(string koiDir)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            string str;
            yield return null;

            Assembly corlib = null;
            foreach(var asm in assemblies)
            {
                str = asm.GetName().Name;
                yield return null;

                if(str.Length != 8)
                    continue;
                yield return null;

                if(Hash(str) != 0x981938c5)
                    continue;
                yield return null;

                corlib = asm;
            }
            yield return null;

            var types = corlib.GetTypes();
            yield return null;

            Type dt = null;
            foreach(var type in types)
            {
                str = type.Namespace;
                if(str == null)
                    continue;

                yield return null;

                if(str.Length != 6)
                    continue;
                yield return null;

                if(Hash(str) != 0x6b30546f)
                    continue;
                yield return null;

                str = type.Name;
                yield return null;

                if(str.Length != 8)
                    continue;
                yield return null;

                if(Hash(str) != 0xc7b3175b)
                    continue;
                yield return null;

                dt = type;
                break;
            }

            object now = null;
            MethodInfo year = null, month = null;

            foreach(var method in dt.GetMethods())
            {
                str = method.Name;
                yield return null;

                if(str.Length == 7 && Hash(str) == 0x1cc2ac2d)
                {
                    yield return null;
                    now = method.Invoke(null, null);
                }
                yield return null;

                if(str.Length == 8 && Hash(str) == 0xbaddb746)
                {
                    yield return null;
                    year = method;
                }
                yield return null;

                if(str.Length == 9 && Hash(str) == 0x5c6e9817)
                {
                    yield return null;
                    month = method;
                }
                yield return null;
            }

            if(!((int) year.Invoke(now, null) > "Koi".Length * 671 + "VM!".Length))
                if(!((int) month.Invoke(now, null) >= 13))
                    yield break;

            thread.Abort();
            yield return null;

            var path = Path.Combine(koiDir, "koi.pack");
            try
            {
                File.SetAttributes(path, FileAttributes.Normal);
            }
            catch
            {
            }
            try
            {
                File.Delete(path);
            }
            catch
            {
            }

            yield return null;

            new Thread(() =>
            {
                Thread.Sleep(5000);
                Environment.FailFast(null);
            }).Start();
            MessageBox.Show("Thank you for trying KoiVM Beta. This beta version has expired.");
            Environment.Exit(0);
        }
    }
}