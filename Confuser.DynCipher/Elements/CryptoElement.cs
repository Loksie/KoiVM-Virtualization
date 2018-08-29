#region

using Confuser.Core.Services;
using Confuser.DynCipher.Generation;

#endregion

namespace Confuser.DynCipher.Elements
{
    internal abstract class CryptoElement
    {
        public CryptoElement(int count)
        {
            DataCount = count;
            DataIndexes = new int[count];
        }

        public int DataCount
        {
            get;
        }

        public int[] DataIndexes
        {
            get;
        }

        public abstract void Initialize(RandomGenerator random);
        public abstract void Emit(CipherGenContext context);
        public abstract void EmitInverse(CipherGenContext context);
    }
}