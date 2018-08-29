#region

using System.IO;
using dnlib.IO;
using dnlib.PE;

#endregion

namespace dnlib.DotNet.Writer
{
    /// <summary>
    ///     Import address table chunk
    /// </summary>
    public sealed class ImportAddressTable : IChunk
    {
        /// <summary>
        ///     Gets/sets the <see cref="ImportDirectory" />
        /// </summary>
        public ImportDirectory ImportDirectory
        {
            get;
            set;
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
            return 8;
        }

        /// <inheritdoc />
        public uint GetVirtualSize()
        {
            return GetFileLength();
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write((uint) ImportDirectory.CorXxxMainRVA);
            writer.Write(0);
        }
    }
}