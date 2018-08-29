#region

using System;
using System.Diagnostics;
using System.IO;

#endregion

namespace dnlib.IO
{
    /// <summary>
    ///     Creates <see cref="MemoryStream" />s to partially access a byte[]
    /// </summary>
    /// <seealso cref="UnmanagedMemoryStreamCreator" />
    [DebuggerDisplay("byte[]: O:{dataOffset} L:{dataLength} {FileName}")]
    internal sealed class MemoryStreamCreator : IImageStreamCreator
    {
        private byte[] data;
        private int dataLength;
        private int dataOffset;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="data">The data</param>
        public MemoryStreamCreator(byte[] data)
            : this(data, 0, data.Length)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="data">The data</param>
        /// <param name="offset">Start offset in <paramref name="data" /></param>
        /// <param name="length">Length of data starting from <paramref name="offset" /></param>
        /// <exception cref="ArgumentOutOfRangeException">If one of the args is invalid</exception>
        public MemoryStreamCreator(byte[] data, int offset, int length)
        {
            if(offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if(length < 0 || offset + length < offset)
                throw new ArgumentOutOfRangeException("length");
            if(offset + length > data.Length)
                throw new ArgumentOutOfRangeException("length");
            this.data = data;
            dataOffset = offset;
            dataLength = length;
        }

        /// <summary>
        ///     The file name
        /// </summary>
        public string FileName
        {
            get;
            set;
        }

        /// <inheritdoc />
        public long Length => dataLength;

        /// <inheritdoc />
        public IImageStream Create(FileOffset offset, long length)
        {
            if(offset < 0 || length < 0)
                return MemoryImageStream.CreateEmpty();

            var offs = (int) Math.Min(dataLength, (long) offset);
            var len = (int) Math.Min((long) dataLength - offs, length);
            return new MemoryImageStream(offset, data, dataOffset + offs, len);
        }

        /// <inheritdoc />
        public IImageStream CreateFull()
        {
            return new MemoryImageStream(0, data, dataOffset, dataLength);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            data = null;
            dataOffset = 0;
            dataLength = 0;
            FileName = null;
        }
    }
}