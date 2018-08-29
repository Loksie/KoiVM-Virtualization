#region

using System;
using System.Collections.Generic;
using Confuser.Core;
using Confuser.Core.Services;
using Confuser.DynCipher;
using Confuser.Renamer;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

#endregion

namespace Confuser.Protections.Constants
{
    internal class CEContext
    {
        public FieldDef BufferField;
        public MethodDef CfgCtxCtor;
        public MethodDef CfgCtxNext;

        public TypeDef CfgCtxType;
        public ConfuserContext Context;
        public FieldDef DataField;
        public TypeDef DataType;

        public int DecoderCount;
        public List<Tuple<MethodDef, DecoderDesc>> Decoders;

        public IDynCipherService DynCipher;

        public EncodeElements Elements;
        public List<uint> EncodedBuffer;
        public MethodDef InitMethod;
        public IMarkerService Marker;

        public Mode Mode;
        public IEncodeMode ModeHandler;
        public ModuleDef Module;
        public INameService Name;
        public ConstantProtection Protection;
        public RandomGenerator Random;
        public Dictionary<MethodDef, List<Tuple<Instruction, uint, IMethod>>> ReferenceRepl;
    }

    internal class DecoderDesc
    {
        public object Data;
        public byte InitializerID;
        public byte NumberID;
        public byte StringID;
    }
}