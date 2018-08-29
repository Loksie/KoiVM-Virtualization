#region

using System;
using dnlib.IO;

#endregion

namespace dnlib.PE
{
    /// <summary>
    ///     Represents the IMAGE_OPTIONAL_HEADER64 PE section
    /// </summary>
    public sealed class ImageOptionalHeader64 : FileSection, IImageOptionalHeader
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="reader">PE file reader pointing to the start of this section</param>
        /// <param name="totalSize">Total size of this optional header (from the file header)</param>
        /// <param name="verify">Verify section</param>
        /// <exception cref="BadImageFormatException">Thrown if verification fails</exception>
        public ImageOptionalHeader64(IImageStream reader, uint totalSize, bool verify)
        {
            if(totalSize < 0x70)
                throw new BadImageFormatException("Invalid optional header size");
            if(verify && reader.Position + totalSize > reader.Length)
                throw new BadImageFormatException("Invalid optional header size");
            SetStartOffset(reader);
            Magic = reader.ReadUInt16();
            MajorLinkerVersion = reader.ReadByte();
            MinorLinkerVersion = reader.ReadByte();
            SizeOfCode = reader.ReadUInt32();
            SizeOfInitializedData = reader.ReadUInt32();
            SizeOfUninitializedData = reader.ReadUInt32();
            AddressOfEntryPoint = (RVA) reader.ReadUInt32();
            BaseOfCode = (RVA) reader.ReadUInt32();
            ImageBase = reader.ReadUInt64();
            SectionAlignment = reader.ReadUInt32();
            FileAlignment = reader.ReadUInt32();
            MajorOperatingSystemVersion = reader.ReadUInt16();
            MinorOperatingSystemVersion = reader.ReadUInt16();
            MajorImageVersion = reader.ReadUInt16();
            MinorImageVersion = reader.ReadUInt16();
            MajorSubsystemVersion = reader.ReadUInt16();
            MinorSubsystemVersion = reader.ReadUInt16();
            Win32VersionValue = reader.ReadUInt32();
            SizeOfImage = reader.ReadUInt32();
            SizeOfHeaders = reader.ReadUInt32();
            CheckSum = reader.ReadUInt32();
            Subsystem = (Subsystem) reader.ReadUInt16();
            DllCharacteristics = (DllCharacteristics) reader.ReadUInt16();
            SizeOfStackReserve = reader.ReadUInt64();
            SizeOfStackCommit = reader.ReadUInt64();
            SizeOfHeapReserve = reader.ReadUInt64();
            SizeOfHeapCommit = reader.ReadUInt64();
            LoaderFlags = reader.ReadUInt32();
            NumberOfRvaAndSizes = reader.ReadUInt32();
            for(var i = 0; i < DataDirectories.Length; i++)
            {
                var len = (uint) (reader.Position - startOffset);
                if(len + 8 <= totalSize)
                    DataDirectories[i] = new ImageDataDirectory(reader, verify);
                else
                    DataDirectories[i] = new ImageDataDirectory();
            }
            reader.Position = (long) startOffset + totalSize;
            SetEndoffset(reader);
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.Magic field
        /// </summary>
        public ushort Magic
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.MajorLinkerVersion field
        /// </summary>
        public byte MajorLinkerVersion
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.MinorLinkerVersion field
        /// </summary>
        public byte MinorLinkerVersion
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.SizeOfCode field
        /// </summary>
        public uint SizeOfCode
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.SizeOfInitializedData field
        /// </summary>
        public uint SizeOfInitializedData
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.SizeOfUninitializedData field
        /// </summary>
        public uint SizeOfUninitializedData
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.AddressOfEntryPoint field
        /// </summary>
        public RVA AddressOfEntryPoint
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.BaseOfCode field
        /// </summary>
        public RVA BaseOfCode
        {
            get;
        }

        /// <summary>
        ///     Returns 0 since BaseOfData is not present in IMAGE_OPTIONAL_HEADER64
        /// </summary>
        public RVA BaseOfData => 0;

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.ImageBase field
        /// </summary>
        public ulong ImageBase
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.SectionAlignment field
        /// </summary>
        public uint SectionAlignment
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.FileAlignment field
        /// </summary>
        public uint FileAlignment
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.MajorOperatingSystemVersion field
        /// </summary>
        public ushort MajorOperatingSystemVersion
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.MinorOperatingSystemVersion field
        /// </summary>
        public ushort MinorOperatingSystemVersion
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.MajorImageVersion field
        /// </summary>
        public ushort MajorImageVersion
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.MinorImageVersion field
        /// </summary>
        public ushort MinorImageVersion
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.MajorSubsystemVersion field
        /// </summary>
        public ushort MajorSubsystemVersion
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.MinorSubsystemVersion field
        /// </summary>
        public ushort MinorSubsystemVersion
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.Win32VersionValue field
        /// </summary>
        public uint Win32VersionValue
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.SizeOfImage field
        /// </summary>
        public uint SizeOfImage
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.SizeOfHeaders field
        /// </summary>
        public uint SizeOfHeaders
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.CheckSum field
        /// </summary>
        public uint CheckSum
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.Subsystem field
        /// </summary>
        public Subsystem Subsystem
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.DllCharacteristics field
        /// </summary>
        public DllCharacteristics DllCharacteristics
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.SizeOfStackReserve field
        /// </summary>
        public ulong SizeOfStackReserve
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.SizeOfStackCommit field
        /// </summary>
        public ulong SizeOfStackCommit
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.SizeOfHeapReserve field
        /// </summary>
        public ulong SizeOfHeapReserve
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.SizeOfHeapCommit field
        /// </summary>
        public ulong SizeOfHeapCommit
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.LoaderFlags field
        /// </summary>
        public uint LoaderFlags
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.NumberOfRvaAndSizes field
        /// </summary>
        public uint NumberOfRvaAndSizes
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_OPTIONAL_HEADER64.DataDirectories field
        /// </summary>
        public ImageDataDirectory[] DataDirectories
        {
            get;
        } = new ImageDataDirectory[16];
    }
}