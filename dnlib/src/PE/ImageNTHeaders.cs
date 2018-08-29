#region

using System;
using dnlib.IO;

#endregion

namespace dnlib.PE
{
    /// <summary>
    ///     Represents the IMAGE_NT_HEADERS PE section
    /// </summary>
    public sealed class ImageNTHeaders : FileSection
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="reader">PE file reader pointing to the start of this section</param>
        /// <param name="verify">Verify section</param>
        /// <exception cref="BadImageFormatException">Thrown if verification fails</exception>
        public ImageNTHeaders(IImageStream reader, bool verify)
        {
            SetStartOffset(reader);
            Signature = reader.ReadUInt32();
            if(verify && Signature != 0x4550)
                throw new BadImageFormatException("Invalid NT headers signature");
            FileHeader = new ImageFileHeader(reader, verify);
            OptionalHeader = CreateImageOptionalHeader(reader, verify);
            SetEndoffset(reader);
        }

        /// <summary>
        ///     Returns the IMAGE_NT_HEADERS.Signature field
        /// </summary>
        public uint Signature
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_NT_HEADERS.FileHeader field
        /// </summary>
        public ImageFileHeader FileHeader
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_NT_HEADERS.OptionalHeader field
        /// </summary>
        public IImageOptionalHeader OptionalHeader
        {
            get;
        }

        /// <summary>
        ///     Creates an IImageOptionalHeader
        /// </summary>
        /// <param name="reader">PE file reader pointing to the start of the optional header</param>
        /// <param name="verify">Verify section</param>
        /// <returns>The created IImageOptionalHeader</returns>
        /// <exception cref="BadImageFormatException">Thrown if verification fails</exception>
        private IImageOptionalHeader CreateImageOptionalHeader(IImageStream reader, bool verify)
        {
            var magic = reader.ReadUInt16();
            reader.Position -= 2;
            switch(magic)
            {
                case 0x010B: return new ImageOptionalHeader32(reader, FileHeader.SizeOfOptionalHeader, verify);
                case 0x020B: return new ImageOptionalHeader64(reader, FileHeader.SizeOfOptionalHeader, verify);
                default: throw new BadImageFormatException("Invalid optional header magic");
            }
        }
    }
}