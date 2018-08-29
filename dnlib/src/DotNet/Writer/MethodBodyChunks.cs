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
    ///     Stores all method body chunks
    /// </summary>
    public sealed class MethodBodyChunks : IChunk
    {
        private const uint FAT_BODY_ALIGNMENT = 4;
        private readonly bool alignFatBodies;
        private readonly List<MethodBody> fatMethods;
        private readonly bool shareBodies;
        private readonly List<MethodBody> tinyMethods;
        private Dictionary<MethodBody, MethodBody> fatMethodsDict;
        private uint length;
        private bool setOffsetCalled;
        private Dictionary<MethodBody, MethodBody> tinyMethodsDict;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="shareBodies"><c>true</c> if bodies can be shared</param>
        public MethodBodyChunks(bool shareBodies)
        {
            this.shareBodies = shareBodies;
            alignFatBodies = true;
            if(shareBodies)
            {
                tinyMethodsDict = new Dictionary<MethodBody, MethodBody>();
                fatMethodsDict = new Dictionary<MethodBody, MethodBody>();
            }
            tinyMethods = new List<MethodBody>();
            fatMethods = new List<MethodBody>();
        }

        /// <summary>
        ///     Gets the number of bytes saved by re-using method bodies
        /// </summary>
        public uint SavedBytes
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

            tinyMethodsDict = null;
            fatMethodsDict = null;

            var rva2 = rva;
            foreach(var mb in tinyMethods)
            {
                mb.SetOffset(offset, rva2);
                var len = mb.GetFileLength();
                rva2 += len;
                offset += len;
            }

            foreach(var mb in fatMethods)
            {
                if(alignFatBodies)
                {
                    var padding = (uint) rva2.AlignUp(FAT_BODY_ALIGNMENT) - (uint) rva2;
                    rva2 += padding;
                    offset += padding;
                }
                mb.SetOffset(offset, rva2);
                var len = mb.GetFileLength();
                rva2 += len;
                offset += len;
            }

            length = (uint) rva2 - (uint) rva;
        }

        /// <inheritdoc />
        public uint GetFileLength()
        {
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
            var rva2 = RVA;
            foreach(var mb in tinyMethods)
            {
                mb.VerifyWriteTo(writer);
                rva2 += mb.GetFileLength();
            }

            foreach(var mb in fatMethods)
            {
                if(alignFatBodies)
                {
                    var padding = (int) rva2.AlignUp(FAT_BODY_ALIGNMENT) - (int) rva2;
                    writer.WriteZeros(padding);
                    rva2 += (uint) padding;
                }
                mb.VerifyWriteTo(writer);
                rva2 += mb.GetFileLength();
            }
        }

        /// <summary>
        ///     Adds a <see cref="MethodBody" /> and returns the one that has been cached
        /// </summary>
        /// <param name="methodBody">The method body</param>
        /// <returns>The cached method body</returns>
        public MethodBody Add(MethodBody methodBody)
        {
            if(setOffsetCalled)
                throw new InvalidOperationException("SetOffset() has already been called");
            if(shareBodies)
            {
                var dict = methodBody.IsFat ? fatMethodsDict : tinyMethodsDict;
                MethodBody cached;
                if(dict.TryGetValue(methodBody, out cached))
                {
                    SavedBytes += (uint) methodBody.GetSizeOfMethodBody();
                    return cached;
                }
                dict[methodBody] = methodBody;
            }
            var list = methodBody.IsFat ? fatMethods : tinyMethods;
            list.Add(methodBody);
            return methodBody;
        }

        /// <summary>
        ///     Removes the specified method body from this chunk
        /// </summary>
        /// <param name="methodBody">The method body</param>
        /// <returns><c>true</c> if the method body is removed</returns>
        public bool Remove(MethodBody methodBody)
        {
            if(setOffsetCalled)
                throw new InvalidOperationException("SetOffset() has already been called");
            var list = methodBody.IsFat ? fatMethods : tinyMethods;
            return list.Remove(methodBody);
        }
    }
}