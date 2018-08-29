#region

using System;
using System.Collections.Generic;
using System.Text;
using dnlib.IO;

#endregion

namespace dnlib.DotNet.MD
{
    /// <summary>
    ///     Represents the .NET metadata header
    /// </summary>
    /// <remarks><c>IMAGE_COR20_HEADER.MetaData</c> points to this header</remarks>
    public sealed class MetaDataHeader : FileSection
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="reader">PE file reader pointing to the start of this section</param>
        /// <param name="verify">Verify section</param>
        /// <exception cref="BadImageFormatException">Thrown if verification fails</exception>
        public MetaDataHeader(IImageStream reader, bool verify)
        {
            SetStartOffset(reader);
            Signature = reader.ReadUInt32();
            if(verify && Signature != 0x424A5342)
                throw new BadImageFormatException("Invalid MetaData header signature");
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            if(verify && !(MajorVersion == 1 && MinorVersion == 1 || MajorVersion == 0 && MinorVersion >= 19))
                throw new BadImageFormatException(string.Format("Unknown MetaData header version: {0}.{1}", MajorVersion, MinorVersion));
            Reserved1 = reader.ReadUInt32();
            StringLength = reader.ReadUInt32();
            VersionString = ReadString(reader, StringLength);
            StorageHeaderOffset = reader.FileOffset + reader.Position;
            Flags = (StorageFlags) reader.ReadByte();
            Reserved2 = reader.ReadByte();
            Streams = reader.ReadUInt16();
            StreamHeaders = new StreamHeader[Streams];
            for(var i = 0; i < StreamHeaders.Count; i++)
                StreamHeaders[i] = new StreamHeader(reader, verify);
            SetEndoffset(reader);
        }

        /// <summary>
        ///     Returns the signature (should be 0x424A5342)
        /// </summary>
        public uint Signature
        {
            get;
        }

        /// <summary>
        ///     Returns the major version
        /// </summary>
        public ushort MajorVersion
        {
            get;
        }

        /// <summary>
        ///     Returns the minor version
        /// </summary>
        public ushort MinorVersion
        {
            get;
        }

        /// <summary>
        ///     Returns the reserved dword (pointer to extra header data)
        /// </summary>
        public uint Reserved1
        {
            get;
        }

        /// <summary>
        ///     Returns the version string length value
        /// </summary>
        public uint StringLength
        {
            get;
        }

        /// <summary>
        ///     Returns the version string
        /// </summary>
        public string VersionString
        {
            get;
        }

        /// <summary>
        ///     Returns the offset of <c>STORAGEHEADER</c>
        /// </summary>
        public FileOffset StorageHeaderOffset
        {
            get;
        }

        /// <summary>
        ///     Returns the flags (reserved)
        /// </summary>
        public StorageFlags Flags
        {
            get;
        }

        /// <summary>
        ///     Returns the reserved byte (padding)
        /// </summary>
        public byte Reserved2
        {
            get;
        }

        /// <summary>
        ///     Returns the number of streams
        /// </summary>
        public ushort Streams
        {
            get;
        }

        /// <summary>
        ///     Returns all stream headers
        /// </summary>
        public IList<StreamHeader> StreamHeaders
        {
            get;
        }

        private static string ReadString(IImageStream reader, uint maxLength)
        {
            var endPos = reader.Position + maxLength;
            if(endPos < reader.Position || endPos > reader.Length)
                throw new BadImageFormatException("Invalid MD version string");
            var utf8Bytes = new byte[maxLength];
            uint i;
            for(i = 0; i < maxLength; i++)
            {
                var b = reader.ReadByte();
                if(b == 0)
                    break;
                utf8Bytes[i] = b;
            }
            reader.Position = endPos;
            return Encoding.UTF8.GetString(utf8Bytes, 0, (int) i);
        }
    }
}