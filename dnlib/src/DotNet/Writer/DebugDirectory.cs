#region

using System;
using System.IO;
using dnlib.DotNet.Pdb;
using dnlib.IO;
using dnlib.PE;

#endregion

namespace dnlib.DotNet.Writer
{
    /// <summary>
    ///     Debug directory chunk
    /// </summary>
    public sealed class DebugDirectory : IChunk
    {
        /// <summary>
        ///     Size of <see cref="IMAGE_DEBUG_DIRECTORY" />
        /// </summary>
        public const int HEADER_SIZE = 28;

        internal IMAGE_DEBUG_DIRECTORY debugDirData;
        private bool dontWriteAnything;
        private uint length;

        /// <summary>
        ///     Gets/sets the time date stamp that should be written. This should be the same time date
        ///     stamp that is written to the PE header.
        /// </summary>
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the raw debug data
        /// </summary>
        public byte[] Data
        {
            get;
            set;
        }

        /// <summary>
        ///     Set it to <c>true</c> if eg. the PDB file couldn't be created. If <c>true</c>, the size
        ///     of this chunk will be 0.
        /// </summary>
        public bool DontWriteAnything
        {
            get { return dontWriteAnything; }
            set
            {
                if(length != 0)
                    throw new InvalidOperationException("SetOffset() has already been called");
                dontWriteAnything = value;
            }
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

            length = HEADER_SIZE;
            if(Data != null) // Could be null if dontWriteAnything is true
                length += (uint) Data.Length;
        }

        /// <inheritdoc />
        public uint GetFileLength()
        {
            if(dontWriteAnything)
                return 0;
            return length;
        }

        /// <inheritdoc />
        public uint GetVirtualSize()
        {
            return GetFileLength();
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            if(dontWriteAnything)
                return;

            writer.Write(debugDirData.Characteristics);
            writer.Write(TimeDateStamp);
            writer.Write(debugDirData.MajorVersion);
            writer.Write(debugDirData.MinorVersion);
            writer.Write(debugDirData.Type);
            writer.Write(debugDirData.SizeOfData);
            writer.Write((uint) RVA + HEADER_SIZE);
            writer.Write((uint) FileOffset + HEADER_SIZE);
            writer.Write(Data);
        }
    }
}