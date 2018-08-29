#region

using System.IO;
using dnlib.IO;
using dnlib.PE;

#endregion

namespace dnlib.DotNet.Writer
{
    /// <summary>
    ///     Stores a byte array
    /// </summary>
    public sealed class ByteArrayChunk : IChunk
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="array">
        ///     The data. It will be owned by this instance and can't be modified by
        ///     other code if this instance is inserted as a <c>key</c> in a dictionary (because
        ///     <see cref="GetHashCode" /> return value will be different if you modify the array). If
        ///     it's never inserted as a <c>key</c> in a dictionary, then the contents can be modified,
        ///     but shouldn't be resized after <see cref="SetOffset" /> has been called.
        /// </param>
        public ByteArrayChunk(byte[] array)
        {
            Data = array ?? new byte[0];
        }

        /// <summary>
        ///     Gets the data
        /// </summary>
        public byte[] Data
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
            return GetFileLength();
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Data);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Utils.GetHashCode(Data);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var other = obj as ByteArrayChunk;
            return other != null && Utils.Equals(Data, other.Data);
        }
    }
}