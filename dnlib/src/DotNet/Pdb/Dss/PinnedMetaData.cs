#region

using System;
using System.Runtime.InteropServices;
using dnlib.IO;

#endregion

namespace dnlib.DotNet.Pdb.Dss
{
    /// <summary>
    ///     Pins a metadata stream in memory
    /// </summary>
    internal sealed class PinnedMetaData : IDisposable
    {
        private readonly IImageStream stream;
        private readonly byte[] streamData;
        private GCHandle gcHandle;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="stream">Metadata stream</param>
        public PinnedMetaData(IImageStream stream)
        {
            this.stream = stream;

            var umStream = stream as UnmanagedMemoryImageStream;
            if(umStream != null)
            {
                Address = umStream.StartAddress;
                GC.SuppressFinalize(this); // no GCHandle so finalizer isn't needed
            }
            else
            {
                var memStream = stream as MemoryImageStream;
                if(memStream != null)
                {
                    streamData = memStream.DataArray;
                    gcHandle = GCHandle.Alloc(streamData, GCHandleType.Pinned);
                    Address = new IntPtr(gcHandle.AddrOfPinnedObject().ToInt64() + memStream.DataOffset);
                }
                else
                {
                    streamData = stream.ReadAllBytes();
                    gcHandle = GCHandle.Alloc(streamData, GCHandleType.Pinned);
                    Address = gcHandle.AddrOfPinnedObject();
                }
            }
        }

        /// <summary>
        ///     Gets the address
        /// </summary>
        public IntPtr Address
        {
            get;
        }

        /// <summary>
        ///     Gets the size
        /// </summary>
        public int Size => (int) stream.Length;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PinnedMetaData()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if(gcHandle.IsAllocated)
                try
                {
                    gcHandle.Free();
                }
                catch(InvalidOperationException)
                {
                }
            if(disposing)
                stream.Dispose();
        }
    }
}