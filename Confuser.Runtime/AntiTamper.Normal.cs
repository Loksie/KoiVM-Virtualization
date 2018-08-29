#region

using System;
using System.Runtime.InteropServices;

#endregion

namespace Confuser.Runtime
{
    internal static class AntiTamperNormal
    {
        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);

        private static unsafe void Initialize()
        {
            var m = typeof(AntiTamperNormal).Module;
            var n = m.FullyQualifiedName;
            var f = n.Length > 0 && n[0] == '<';
            var b = (byte*) Marshal.GetHINSTANCE(m);
            var p = b + *(uint*) (b + 0x3c);
            var s = *(ushort*) (p + 0x6);
            var o = *(ushort*) (p + 0x14);

            uint* e = null;
            uint l = 0;
            var r = (uint*) (p + 0x18 + o);
            var z = (uint) Mutation.KeyI1 - 5;
            var x = (uint) Mutation.KeyI2;
            var c = (uint) Mutation.KeyI3;
            var v = (uint) Mutation.KeyI4;
            z = z + 5;
            for(var i = 0; i < s; i++)
            {
                var g = *r++ * *r++;
                if(g == (uint) Mutation.KeyI0)
                {
                    e = (uint*) (b + (f ? *(r + 3) : *(r + 1)));
                    l = (f ? *(r + 2) : *(r + 0)) >> 2;
                }
                else if(g != 0)
                {
                    var q = (uint*) (b + (f ? *(r + 3) : *(r + 1)));
                    var j = *(r + 2) >> 2;
                    for(uint k = 0; k < j; k++)
                    {
                        var t = (z ^ *q++) + x + c * v;
                        z = x;
                        x = c;
                        x = v;
                        v = t;
                    }
                }
                r += 8;
            }

            uint[] y = new uint[0x10], d = new uint[0x10];
            for(var i = 0; i < 0x10; i++)
            {
                y[i] = v;
                d[i] = x;
                z = (x >> 5) | (x << 27);
                x = (c >> 3) | (c << 29);
                c = (v >> 7) | (v << 25);
                v = (z >> 11) | (z << 21);
            }
            Mutation.Crypt(y, d);

            uint w = 0x40;
            VirtualProtect((IntPtr) e, l << 2, w, out w);

            if(w == 0x40)
                return;

            uint h = 0;
            for(uint i = 0; i < l; i++)
            {
                *e ^= y[h & 0xf];
                y[h & 0xf] = (y[h & 0xf] ^ *e++) + 0x3dbb2819;
                h++;
            }
        }
    }
}