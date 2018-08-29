#region

using System;

#endregion

namespace Confuser.Runtime
{
    internal static partial class AntiDebugAntinet
    {
        private static void Initialize()
        {
            if(!InitializeAntiDebugger())
                Environment.FailFast(null);
            InitializeAntiProfiler();
            if(IsProfilerAttached)
            {
                Environment.FailFast(null);
                PreventActiveProfilerFromReceivingProfilingMessages();
            }
        }
    }
}