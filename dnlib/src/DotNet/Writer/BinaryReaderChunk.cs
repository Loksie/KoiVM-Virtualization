#region

using System.IO;
using dnlib.IO;
using dnlib.PE;

#endregion

namespace dnlib.DotNet.Writer
{
    /// <summary>
    ///     A <see cref="IBinaryReader" /> chunk
    /// </summary>
    public class BinaryReaderChunk : IChunk
    {
        private readonly uint virtualSize;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="data">The data</param>
        public BinaryReaderChunk(IBinaryReader data)
            : this(data, (uint) data.Length)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="data">The data</param>
        /// <param name="virtualSize">Virtual size of <paramref name="data" /></param>
        public BinaryReaderChunk(IBinaryReader data, uint virtualSize)
        {
            Data = data;
            this.virtualSize = virtualSize;
        }

        /// <summary>
        ///     Gets the data
        /// </summary>
        public IBinaryReader Data
        {
            get;
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
            return (uint) Data.Length;
        }

        /// <inheritdoc />
        public uint GetVirtualSize()
        {
            return virtualSize;
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            Data.Position = 0;
            Data.WriteTo(writer);
        }
    }
}