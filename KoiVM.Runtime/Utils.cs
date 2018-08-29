#region

using KoiVM.Runtime.Dynamic;

#endregion

namespace KoiVM.Runtime
{
    internal static unsafe class Utils
    {
        public static uint ReadCompressedUInt(ref byte* ptr)
        {
            uint num = 0;
            var shift = 0;
            do
            {
                num |= (*ptr & 0x7fu) << shift;
                shift += 7;
            } while((*ptr++ & 0x80) != 0);
            return num;
        }

        public static uint FromCodedToken(uint codedToken)
        {
            var rid = codedToken >> 3;
            switch(codedToken & 7)
            {
                case 1:
                    return rid | 0x02000000;
                case 2:
                    return rid | 0x01000000;
                case 3:
                    return rid | 0x1b000000;
                case 4:
                    return rid | 0x0a000000;
                case 5:
                    return rid | 0x06000000;
                case 6:
                    return rid | 0x04000000;
                case 7:
                    return rid | 0x2b000000;
            }
            return rid;
        }

        // https://github.com/copy/v86/blob/master/src/misc_instr.macro.js
        public static void UpdateFL(uint op1, uint op2, uint flResult, uint result, ref byte fl, byte mask)
        {
            const ulong SignMask = 1U << 31;
            byte flag = 0;
            if(result == 0)
                flag |= Constants.FL_ZERO;
            if((result & SignMask) != 0)
                flag |= Constants.FL_SIGN;
            if(((op1 ^ flResult) & (op2 ^ flResult) & SignMask) != 0)
                flag |= Constants.FL_OVERFLOW;
            if(((op1 ^ ((op1 ^ op2) & (op2 ^ flResult))) & SignMask) != 0)
                flag |= Constants.FL_CARRY;
            fl = (byte) ((fl & ~mask) | (flag & mask));
        }

        public static void UpdateFL(ulong op1, ulong op2, ulong flResult, ulong result, ref byte fl, byte mask)
        {
            const ulong SignMask = 1U << 63;
            byte flag = 0;
            if(result == 0)
                flag |= Constants.FL_ZERO;
            if((result & SignMask) != 0)
                flag |= Constants.FL_SIGN;
            if(((op1 ^ flResult) & (op2 ^ flResult) & SignMask) != 0)
                flag |= Constants.FL_OVERFLOW;
            if(((op1 ^ ((op1 ^ op2) & (op2 ^ flResult))) & SignMask) != 0)
                flag |= Constants.FL_CARRY;
            fl = (byte) ((fl & ~mask) | (flag & mask));
        }
    }
}