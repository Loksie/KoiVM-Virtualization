#region

using System.Collections.Generic;
using dnlib.DotNet.Emit;

#endregion

namespace Confuser.Protections.ControlFlow
{
    internal interface IPredicate
    {
        void Init(CilBody body);
        void EmitSwitchLoad(IList<Instruction> instrs);
        int GetSwitchKey(int key);
    }
}