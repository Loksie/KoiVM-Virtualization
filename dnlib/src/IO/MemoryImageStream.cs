#region

using System;
using System.Diagnostics;
using System.IO;

#endregion

namespace dnlib.IO
{
    /// <summary>
    ///     IImageStream for byte[]
    /// </summary>
    [DebuggerDisplay("FO:{FileOffset} S:{Length}")]
    public sealed class MemoryImageStream : IImageStream
    {
        private int dataEnd;
        private int position;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="fileOffset">File offset of data</param>
        /// <param name="data">The data</param>
        /// <param name="dataOffset">Start offset in <paramref name="data" /></param>
        /// <param name="dataLength">Length of data</param>
        public MemoryImageStream(FileOffset fileOffset, byte[] data, int dataOffset, int dataLength)
        {
            FileOffset = fileOffset;
            DataArray = data;
            DataOffset = dataOffset;
            dataEnd = dataOffset + dataLength;
            position = dataOffset;
        }

        /// <summary>
        ///     Gets the data
        /// </summary>
        internal byte[] DataArray
        {
            get;
            private set;
        }

        /// <summary>
        ///     Gets the start of the data in <see cref="DataArray" /> used by this stream
        /// </summary>
        internal int DataOffset
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public FileOffset FileOffset
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public long Length => dataEnd - DataOffset;

        /// <inheritdoc />
        public long Position
        {
            get { return position - DataOffset; }
            set
            {
                var newPos = DataOffset + value;
                if(newPos < DataOffset || newPos > int.MaxValue)
                    newPos = int.MaxValue;
                position = (int) newPos;
            }
        }

        /// <inheritdoc />
        public IImageStream Create(FileOffset offset, long length)
        {
            if((long) offset < 0 || length < 0)
                return CreateEmpty();

            var offs = (int) Math.Min(Length, (long) offset);
            var len = (int) Math.Min(Length - offs, length);
            return new MemoryImageStream((FileOffset) ((long) FileOffset + (long) offset), DataArray, DataOffset + offs, len);
        }

        /// <inheritdoc />
        public byte[] ReadBytes(int size)
        {
            if(size < 0)
                throw new IOException("Invalid size");
            size = Math.Min(size, (int) Length - Math.Min((int) Length, (int) Position));
            var newData = new byte[size];
            Array.Copy(DataArray, position, newData, 0, size);
            position += size;
            return newData;
        }

        /// <inheritdoc />
        public int Read(byte[] buffer, int offset, int length)
        {
            if(length < 0)
                throw new IOException("Invalid size");
            length = Math.Min(length, (int) Length - Math.Min((int) Length, (int) Position));
            Array.Copy(DataArray, position, buffer, offset, length);
            position += length;
            return length;
        }

        /// <inheritdoc />
        public byte[] ReadBytesUntilByte(byte b)
        {
            var pos = GetPositionOf(b);
            if(pos < 0)
                return null;
            return ReadBytes(pos - position);
        }

        /// <inheritdoc />
        public sbyte ReadSByte()
        {
            if(position >= dataEnd)
                throw new IOException("Can't read one SByte");
            return (sbyte) DataArray[position++];
        }

        /// <inheritdoc />
        public byte ReadByte()
        {
            if(position >= dataEnd)
                throw new IOException("Can't read one Byte");
            return DataArray[position++];
        }

        /// <inheritdoc />
        public short ReadInt16()
        {
            if(position + 1 >= dataEnd)
                throw new IOException("Can't read one Int16");
            return (short) (DataArray[position++] | (DataArray[position++] << 8));
        }

        /// <inheritdoc />
        public ushort ReadUInt16()
        {
            if(position + 1 >= dataEnd)
                throw new IOException("Can't read one UInt16");
            return (ushort) (DataArray[position++] | (DataArray[position++] << 8));
        }

        /// <inheritdoc />
        public int ReadInt32()
        {
            if(position + 3 >= dataEnd)
                throw new IOException("Can't read one Int32");
            return DataArray[position++] |
                   (DataArray[position++] << 8) |
                   (DataArray[position++] << 16) |
                   (DataArray[position++] << 24);
        }

        /// <inheritdoc />
        public uint ReadUInt32()
        {
            if(position + 3 >= dataEnd)
                throw new IOException("Can't read one UInt32");
            return (uint) (DataArray[position++] |
                           (DataArray[position++] << 8) |
                           (DataArray[position++] << 16) |
                           (DataArray[position++] << 24));
        }

        /// <inheritdoc />
        public long ReadInt64()
        {
            if(position + 7 >= dataEnd)
                throw new IOException("Can't read one Int64");
            return DataArray[position++] |
                   ((long) DataArray[position++] << 8) |
                   ((long) DataArray[position++] << 16) |
                   ((long) DataArray[position++] << 24) |
                   ((long) DataArray[position++] << 32) |
                   ((long) DataArray[position++] << 40) |
                   ((long) DataArray[position++] << 48) |
                   ((long) DataArray[position++] << 56);
        }

        /// <inheritdoc />
        public ulong ReadUInt64()
        {
            if(position + 7 >= dataEnd)
                throw new IOException("Can't read one UInt64");
            return DataArray[position++] |
                   ((ulong) DataArray[position++] << 8) |
                   ((ulong) DataArray[position++] << 16) |
                   ((ulong) DataArray[position++] << 24) |
                   ((ulong) DataArray[position++] << 32) |
                   ((ulong) DataArray[position++] << 40) |
                   ((ulong) DataArray[position++] << 48) |
                   ((ulong) DataArray[position++] << 56);
        }

        /// <inheritdoc />
        public float ReadSingle()
        {
            if(position + 3 >= dataEnd)
                throw new IOException("Can't read one Single");
            var val = BitConverter.ToSingle(DataArray, position);
            position += 4;
            return val;
        }

        /// <inheritdoc />
        public double ReadDouble()
        {
            if(position + 7 >= dataEnd)
                throw new IOException("Can't read one Double");
            var val = BitConverter.ToDouble(DataArray, position);
            position += 8;
            return val;
        }

        /// <inheritdoc />
        public string ReadString(int chars)
        {
            if((uint) chars > int.MaxValue)
                throw new IOException("Not enough space to read the string");
            if(position + chars * 2 < position || chars != 0 && position + chars * 2 - 1 >= dataEnd)
                throw new IOException("Not enough space to read the string");
            var array = new char[chars];
            for(var i = 0; i < chars; i++)
                array[i] = (char) (DataArray[position++] | (DataArray[position++] << 8));
            return new string(array);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            FileOffset = 0;
            DataArray = null;
            DataOffset = 0;
            dataEnd = 0;
            position = 0;
        }

        /// <summary>
        ///     Creates a new <see cref="MemoryImageStream" /> instance
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>A new <see cref="MemoryImageStream" /> instance</returns>
        public static MemoryImageStream Create(byte[] data)
        {
            return new MemoryImageStream(0, data, 0, data.Length);
        }

        /// <summary>
        ///     Creates a new <see cref="MemoryImageStream" /> instance
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="offset">Start offset in <paramref name="data" /></param>
        /// <param name="len">Length of data</param>
        /// <returns>A new <see cref="MemoryImageStream" /> instance</returns>
        public static MemoryImageStream Create(byte[] data, int offset, int len)
        {
            return new MemoryImageStream(0, data, offset, len);
        }

        /// <summary>
        ///     Creates an empty <see cref="MemoryImageStream" /> instance
        /// </summary>
        public static MemoryImageStream CreateEmpty()
        {
            return new MemoryImageStream(0, new byte[0], 0, 0);
        }

        private int GetPositionOf(byte b)
        {
            var pos = position;
            while(pos < dataEnd)
            {
                if(DataArray[pos] == b)
                    return pos;
                pos++;
            }
            return -1;
        }
    }
}