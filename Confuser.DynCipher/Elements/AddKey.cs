#region

using Confuser.Core.Services;
using Confuser.DynCipher.AST;
using Confuser.DynCipher.Generation;

#endregion

namespace Confuser.DynCipher.Elements
{
    internal class AddKey : CryptoElement
    {
        public AddKey(int index)
            : base(0)
        {
            Index = index;
        }

        public int Index
        {
            get;
        }

        public override void Initialize(RandomGenerator random)
        {
        }

        private void EmitCore(CipherGenContext context)
        {
            var val = context.GetDataExpression(Index);

            context.Emit(new AssignmentStatement
            {
                Value = val ^ context.GetKeyExpression(Index),
                Target = val
            });
        }

        public override void Emit(CipherGenContext context)
        {
            EmitCore(context);
        }

        public override void EmitInverse(CipherGenContext context)
        {
            EmitCore(context);
        }
    }
}