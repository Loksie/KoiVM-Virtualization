#region

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

#endregion

namespace dnlib.DotNet.Pdb.Dss
{
    /// <summary>
    ///     Implements <see cref="IStream" /> and uses a <see cref="Stream" /> as the underlying
    ///     stream.
    /// </summary>
    internal sealed class StreamIStream : IStream
    {
        private const int STG_E_INVALIDFUNCTION = unchecked((int) 0x80030001);
        private readonly string name;
        private readonly Stream stream;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="stream">Source stream</param>
        public StreamIStream(Stream stream)
            : this(stream, string.Empty)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <param name="name">Name of original file or <c>null</c> if unknown.</param>
        public StreamIStream(Stream stream, string name)
        {
            if(stream == null)
                throw new ArgumentNullException("stream");
            this.stream = stream;
            this.name = name ?? string.Empty;
        }

        /// <inheritdoc />
        public void Clone(out IStream ppstm)
        {
            Marshal.ThrowExceptionForHR(STG_E_INVALIDFUNCTION);
            throw new Exception();
        }

        /// <inheritdoc />
        public void Commit(int grfCommitFlags)
        {
            stream.Flush();
        }

        /// <inheritdoc />
        public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
            if(cb > int.MaxValue)
                cb = int.MaxValue;
            else if(cb < 0)
                cb = 0;
            var sizeToRead = (int) cb;

            if(stream.Position + sizeToRead < sizeToRead || stream.Position + sizeToRead > stream.Length)
                sizeToRead = (int) (stream.Length - Math.Min(stream.Position, stream.Length));

            var buffer = new byte[sizeToRead];
            Read(buffer, sizeToRead, pcbRead);
            if(pcbRead != null)
                Marshal.WriteInt64(pcbRead, Marshal.ReadInt32(pcbRead));
            pstm.Write(buffer, buffer.Length, pcbWritten);
            if(pcbWritten != null)
                Marshal.WriteInt64(pcbWritten, Marshal.ReadInt32(pcbWritten));
        }

        /// <inheritdoc />
        public void LockRegion(long libOffset, long cb, int dwLockType)
        {
            Marshal.ThrowExceptionForHR(STG_E_INVALIDFUNCTION);
        }

        /// <inheritdoc />
        public void Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            if(cb < 0)
                cb = 0;

            cb = stream.Read(pv, 0, cb);

            if(pcbRead != IntPtr.Zero)
                Marshal.WriteInt32(pcbRead, cb);
        }

        /// <inheritdoc />
        public void Revert()
        {
        }

        /// <inheritdoc />
        public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            switch((STREAM_SEEK) dwOrigin)
            {
                case STREAM_SEEK.SET:
                    stream.Position = dlibMove;
                    break;

                case STREAM_SEEK.CUR:
                    stream.Position += dlibMove;
                    break;

                case STREAM_SEEK.END:
                    stream.Position = stream.Length + dlibMove;
                    break;
            }

            if(plibNewPosition != IntPtr.Zero)
                Marshal.WriteInt64(plibNewPosition, stream.Position);
        }

        /// <inheritdoc />
        public void SetSize(long libNewSize)
        {
            stream.SetLength(libNewSize);
        }

        /// <inheritdoc />
        public void Stat(out STATSTG pstatstg, int grfStatFlag)
        {
            var s = new STATSTG();

            // s.atime = ???;
            s.cbSize = stream.Length;
            s.clsid = Guid.Empty;
            // s.ctime = ???;
            s.grfLocksSupported = 0;
            s.grfMode = 2;
            s.grfStateBits = 0;
            // s.mtime = ???;
            if((grfStatFlag & (int) STATFLAG.NONAME) == 0)
                s.pwcsName = name;
            s.reserved = 0;
            s.type = (int) STGTY.STREAM;

            pstatstg = s;
        }

        /// <inheritdoc />
        public void UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            Marshal.ThrowExceptionForHR(STG_E_INVALIDFUNCTION);
        }

        /// <inheritdoc />
        public void Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            stream.Write(pv, 0, cb);
            if(pcbWritten != null)
                Marshal.WriteInt32(pcbWritten, cb);
        }

        private enum STREAM_SEEK
        {
            SET = 0,
            CUR = 1,
            END = 2
        }

        private enum STATFLAG
        {
            DEFAULT = 0,
            NONAME = 1,
            NOOPEN = 2
        }

        private enum STGTY
        {
            STORAGE = 1,
            STREAM = 2,
            LOCKBYTES = 3,
            PROPERTY = 4
        }
    }
}