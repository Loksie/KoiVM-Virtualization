#region

using System;
using System.Collections.Generic;
using System.IO;
using dnlib.IO;
using dnlib.PE;

#endregion

namespace dnlib.DotNet.Writer
{
    /// <summary>
    ///     .NET resources
    /// </summary>
    public sealed class NetResources : IChunk
    {
        private readonly uint alignment;
        private readonly List<ByteArrayChunk> resources = new List<ByteArrayChunk>();
        private bool setOffsetCalled;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="alignment">Alignment of all resources</param>
        public NetResources(uint alignment)
        {
            this.alignment = alignment;
        }

        /// <summary>
        ///     Gets offset of next resource. This offset is relative to the start of
        ///     the .NET resources and is always aligned.
        /// </summary>
        public uint NextOffset
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
        public RVA RVA
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public void SetOffset(FileOffset offset, RVA rva)
        {
            setOffsetCalled = true;
            FileOffset = offset;
            RVA = rva;
            foreach(var resource in resources)
            {
                resource.SetOffset(offset + 4, rva + 4);
                var len = 4 + resource.GetFileLength();
                offset = (offset + len).AlignUp(alignment);
                rva = (rva + len).AlignUp(alignment);
            }
        }

        /// <inheritdoc />
        public uint GetFileLength()
        {
            return NextOffset;
        }

        /// <inheritdoc />
        public uint GetVirtualSize()
        {
            return GetFileLength();
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            var rva2 = RVA;
            foreach(var resourceData in resources)
            {
                writer.Write(resourceData.GetFileLength());
                resourceData.VerifyWriteTo(writer);
                rva2 += 4 + resourceData.GetFileLength();
                var padding = (int) rva2.AlignUp(alignment) - (int) rva2;
                writer.WriteZeros(padding);
                rva2 += (uint) padding;
            }
        }

        /// <summary>
        ///     Adds a resource
        /// </summary>
        /// <param name="stream">The resource data</param>
        /// <returns>The resource data</returns>
        public ByteArrayChunk Add(IImageStream stream)
        {
            if(setOffsetCalled)
                throw new InvalidOperationException("SetOffset() has already been called");
            var rawData = stream.ReadAllBytes();
            NextOffset = Utils.AlignUp(NextOffset + 4 + (uint) rawData.Length, alignment);
            var data = new ByteArrayChunk(rawData);
            resources.Add(data);
            return data;
        }
    }
}