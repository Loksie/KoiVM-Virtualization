#region

using System;

#endregion

namespace KoiVM.Runtime
{
    internal static class Platform
    {
        public static readonly bool x64 = IntPtr.Size == 8;
        public static readonly bool LittleEndian = BitConverter.IsLittleEndian;
    }
}