#region

using System;
using System.Diagnostics;
using System.Threading;

#endregion

namespace Confuser.Runtime
{
    internal static class AntiDebugSafe
    {
        private static void Initialize()
        {
            var x = "COR";
            var env = typeof(Environment);
            var method = env.GetMethod("GetEnvironmentVariable", new[] {typeof(string)});
            if(method != null &&
               "1".Equals(method.Invoke(null, new object[] {x + "_ENABLE_PROFILING"})))
                Environment.FailFast(null);

            var thread = new Thread(Worker);
            thread.IsBackground = true;
            thread.Start(null);
        }

        private static void Worker(object thread)
        {
            var th = thread as Thread;
            if(th == null)
            {
                th = new Thread(Worker);
                th.IsBackground = true;
                th.Start(Thread.CurrentThread);
                Thread.Sleep(500);
            }
            while(true)
            {
                if(Debugger.IsAttached || Debugger.IsLogging())
                    Environment.FailFast(null);

                var pro = Process.GetProcesses();

                for(var x = pro.Length - 1; x >= 0; x--)
                    if(pro[x].MainWindowTitle.ToLower().Contains("cheat") || pro[x].MainWindowTitle.ToLower().Contains("dnspy") || pro[x].MainWindowTitle.ToLower().Contains("dump") || pro[x].MainWindowTitle.ToLower().Contains("olly") || pro[x].MainWindowTitle.ToLower().Contains("de4dot"))
                        Environment.Exit(0);

                if(!th.IsAlive)
                    Environment.FailFast(null);

                Thread.Sleep(1000);
            }
        }
    }
}