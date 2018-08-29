#region

using System;
using System.Collections.Generic;

#endregion

namespace KoiVM.Confuser.Processor
{
    internal static class Utils
    {
        public static void Shuffle<T>(this Random random, IList<T> list)
        {
            var n = list.Count;
            while(n > 1)
            {
                n--;
                var k = random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}