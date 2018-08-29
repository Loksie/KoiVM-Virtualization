#region

using System.IO;
using dnlib.IO;
using dnlib.PE;

#endregion

namespace dnlib.DotNet.Writer
{
    /// <summary>
    ///     Strong name signature chunk
    /// </summary>
    public sealed class StrongNameSignature : IChunk
    {
        private readonly int size;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="size">Size of strong name signature</param>
        public StrongNameSignature(int size)
        {
            this.size = size;
        }

        /// <inheritdoc />
        public FileOffset FileOffset
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public RVA RVA
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public void SetOffset(FileOffset offset, RVA rva)
        {
            FileOffset = offset;
            RVA = rva;
        }

        /// <inheritdoc />
        public uint GetFileLength()
        {
            return (uint) size;
        }

        /// <inheritdoc />
        public uint GetVirtualSize()
        {
            return GetFileLength();
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            writer.WriteZeros(size);
        }
    }
}