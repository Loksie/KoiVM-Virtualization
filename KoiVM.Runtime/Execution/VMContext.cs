#region

using System.Collections.Generic;
using KoiVM.Runtime.Dynamic;

#endregion

namespace KoiVM.Runtime.Execution
{
    internal class VMContext
    {
        private const int NumRegisters = 16;
        public readonly List<EHFrame> EHStack = new List<EHFrame>();
        public readonly List<EHState> EHStates = new List<EHState>();
        public readonly VMInstance Instance;

        public readonly VMSlot[] Registers = new VMSlot[16];
        public readonly VMStack Stack = new VMStack();

        public VMContext(VMInstance inst)
        {
            Instance = inst;
        }

        public unsafe byte ReadByte()
        {
            var key = Registers[Constants.REG_K1].U4;
            var ip = (byte*) Registers[Constants.REG_IP].U8++;
            var b = (byte) (*ip ^ key);
            key = key * 7 + b;
            Registers[Constants.REG_K1].U4 = key;
            return b;
        }
    }
}