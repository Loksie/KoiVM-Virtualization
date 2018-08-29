#region

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using dnlib.IO;
using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

#endregion

namespace dnlib.DotNet.Pdb.Dss
{
    /// <summary>
    ///     Implements <see cref="IStream" /> and uses an <see cref="IImageStream" /> as the underlying
    ///     stream.
    /// </summary>
    internal sealed class ImageStreamIStream : IStream, IDisposable
    {
        private const int STG_E_INVALIDFUNCTION = unchecked((int) 0x80030001);
        private const int STG_E_CANTSAVE = unchecked((int) 0x80030103);
        private readonly string name;
        private readonly IImageStream stream;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="stream">Source stream</param>
        public ImageStreamIStream(IImageStream stream)
            : this(stream, string.Empty)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <param name="name">Name of original file or <c>null</c> if unknown.</param>
        public ImageStreamIStream(IImageStream stream, string name)
        {
            if(stream == null)
                throw new ArgumentNullException("stream");
            this.stream = stream;
            this.name = name ?? string.Empty;
        }

        /// <summary>
        ///     User can set this to anything he/she wants. If it implements <see cref="IDisposable" />,
        ///     its <see cref="IDisposable.Dispose()" /> method will get called when this instance
        ///     is <see cref="IDisposable.Dispose()" />'d.
        /// </summary>
        public object UserData
        {
            get;
            set;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            stream.Dispose();
            var id = UserData as IDisposable;
            if(id != null)
                id.Dispose();
        }

        /// <inheritdoc />
        public void Clone(out IStream ppstm)
        {
            var newStream = stream.Clone();
            newStream.Position = stream.Position;
            ppstm = new ImageStreamIStream(newStream, name);
        }

        /// <inheritdoc />
        public void Commit(int grfCommitFlags)
        {
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
            Marshal.ThrowExceptionForHR(STG_E_INVALIDFUNCTION);
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
            s.grfMode = 0;
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
            Marshal.ThrowExceptionForHR(STG_E_CANTSAVE);
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