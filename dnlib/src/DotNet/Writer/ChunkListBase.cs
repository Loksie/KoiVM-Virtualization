#region

using System.Collections.Generic;
using System.IO;
using dnlib.IO;
using dnlib.PE;

#endregion

namespace dnlib.DotNet.Writer
{
    /// <summary>
    ///     Base class of chunk list types
    /// </summary>
    /// <typeparam name="T">Chunk type</typeparam>
    public abstract class ChunkListBase<T> : IChunk where T : IChunk
    {
        /// <summary>All chunks</summary>
        protected List<Elem> chunks;

        private uint length;

        /// <summary><c>true</c> if <see cref="SetOffset" /> has been called</summary>
        protected bool setOffsetCalled;

        private uint virtualSize;

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
        public virtual void SetOffset(FileOffset offset, RVA rva)
        {
            setOffsetCalled = true;
            FileOffset = offset;
            RVA = rva;
            length = 0;
            virtualSize = 0;
            foreach(var elem in chunks)
            {
                var paddingF = (uint) offset.AlignUp(elem.alignment) - (uint) offset;
                var paddingV = (uint) rva.AlignUp(elem.alignment) - (uint) rva;
                offset += paddingF;
                rva += paddingV;
                elem.chunk.SetOffset(offset, rva);
                var chunkLenF = elem.chunk.GetFileLength();
                var chunkLenV = elem.chunk.GetVirtualSize();
                offset += chunkLenF;
                rva += chunkLenV;
                length += paddingF + chunkLenF;
                virtualSize += paddingV + chunkLenV;
            }
        }

        /// <inheritdoc />
        public uint GetFileLength()
        {
            return length;
        }

        /// <inheritdoc />
        public uint GetVirtualSize()
        {
            return virtualSize;
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            var offset2 = FileOffset;
            foreach(var elem in chunks)
            {
                var paddingF = (int) offset2.AlignUp(elem.alignment) - (int) offset2;
                writer.WriteZeros(paddingF);
                elem.chunk.VerifyWriteTo(writer);
                offset2 += (uint) paddingF + elem.chunk.GetFileLength();
            }
        }

        /// <summary>
        ///     Helper struct
        /// </summary>
        protected struct Elem
        {
            /// <summary>Data</summary>
            public readonly T chunk;

            /// <summary>Alignment</summary>
            public readonly uint alignment;

            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="chunk">Chunk</param>
            /// <param name="alignment">Alignment</param>
            public Elem(T chunk, uint alignment)
            {
                this.chunk = chunk;
                this.alignment = alignment;
            }
        }

        /// <summary>
        ///     Equality comparer for <see cref="Elem" />
        /// </summary>
        protected sealed class ElemEqualityComparer : IEqualityComparer<Elem>
        {
            private readonly IEqualityComparer<T> chunkComparer;

            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="chunkComparer">Compares the chunk type</param>
            public ElemEqualityComparer(IEqualityComparer<T> chunkComparer)
            {
                this.chunkComparer = chunkComparer;
            }

            /// <inheritdoc />
            public bool Equals(Elem x, Elem y)
            {
                return x.alignment == y.alignment &&
                       chunkComparer.Equals(x.chunk, y.chunk);
            }

            /// <inheritdoc />
            public int GetHashCode(Elem obj)
            {
                return (int) obj.alignment + chunkComparer.GetHashCode(obj.chunk);
            }
        }
    }
}