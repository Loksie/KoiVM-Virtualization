#region

using System;
using System.Reflection;

#endregion

namespace KoiVM.Runtime.Data
{
    internal class VMFuncSig
    {
        private readonly int[] paramToks;
        private readonly int retTok;

        public byte Flags;

        private readonly Module module;
        private Type[] paramTypes;
        private Type retType;

        public unsafe VMFuncSig(ref byte* ptr, Module module)
        {
            this.module = module;

            Flags = *ptr++;
            paramToks = new int[Utils.ReadCompressedUInt(ref ptr)];
            for(var i = 0; i < paramToks.Length; i++) paramToks[i] = (int) Utils.FromCodedToken(Utils.ReadCompressedUInt(ref ptr));
            retTok = (int) Utils.FromCodedToken(Utils.ReadCompressedUInt(ref ptr));
        }

        public Type[] ParamTypes
        {
            get
            {
                if(paramTypes != null)
                    return paramTypes;

                var p = new Type[paramToks.Length];
                for(var i = 0; i < p.Length; i++) p[i] = module.ResolveType(paramToks[i]);
                paramTypes = p;
                return p;
            }
        }

        public Type RetType => retType ?? (retType = module.ResolveType(retTok));
    }
}