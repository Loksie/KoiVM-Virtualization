#region

using System;
using dnlib.IO;

#endregion

namespace dnlib.PE
{
    /// <summary>
    ///     Represents the IMAGE_FILE_HEADER PE section
    /// </summary>
    public sealed class ImageFileHeader : FileSection
    {
        private readonly ushort numberOfSections;
        private readonly ushort sizeOfOptionalHeader;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="reader">PE file reader pointing to the start of this section</param>
        /// <param name="verify">Verify section</param>
        /// <exception cref="BadImageFormatException">Thrown if verification fails</exception>
        public ImageFileHeader(IImageStream reader, bool verify)
        {
            SetStartOffset(reader);
            Machine = (Machine) reader.ReadUInt16();
            numberOfSections = reader.ReadUInt16();
            TimeDateStamp = reader.ReadUInt32();
            PointerToSymbolTable = reader.ReadUInt32();
            NumberOfSymbols = reader.ReadUInt32();
            sizeOfOptionalHeader = reader.ReadUInt16();
            Characteristics = (Characteristics) reader.ReadUInt16();
            SetEndoffset(reader);
            if(verify && sizeOfOptionalHeader == 0)
                throw new BadImageFormatException("Invalid SizeOfOptionalHeader");
        }

        /// <summary>
        ///     Returns the IMAGE_FILE_HEADER.Machine field
        /// </summary>
        public Machine Machine
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_FILE_HEADER.NumberOfSections field
        /// </summary>
        public int NumberOfSections => numberOfSections;

        /// <summary>
        ///     Returns the IMAGE_FILE_HEADER.TimeDateStamp field
        /// </summary>
        public uint TimeDateStamp
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_FILE_HEADER.PointerToSymbolTable field
        /// </summary>
        public uint PointerToSymbolTable
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_FILE_HEADER.NumberOfSymbols field
        /// </summary>
        public uint NumberOfSymbols
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_FILE_HEADER.SizeOfOptionalHeader field
        /// </summary>
        public uint SizeOfOptionalHeader => sizeOfOptionalHeader;

        /// <summary>
        ///     Returns the IMAGE_FILE_HEADER.Characteristics field
        /// </summary>
        public Characteristics Characteristics
        {
            get;
        }
    }
}