#region

using System;
using dnlib.IO;
using dnlib.PE;

#endregion

namespace dnlib.DotNet.MD
{
    /// <summary>
    ///     Represents the IMAGE_COR20_HEADER structure
    /// </summary>
    public sealed class ImageCor20Header : FileSection
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="reader">PE file reader pointing to the start of this section</param>
        /// <param name="verify">Verify section</param>
        /// <exception cref="BadImageFormatException">Thrown if verification fails</exception>
        public ImageCor20Header(IImageStream reader, bool verify)
        {
            SetStartOffset(reader);
            CB = reader.ReadUInt32();
            if(verify && CB < 0x48)
                throw new BadImageFormatException("Invalid IMAGE_COR20_HEADER.cb value");
            MajorRuntimeVersion = reader.ReadUInt16();
            MinorRuntimeVersion = reader.ReadUInt16();
            MetaData = new ImageDataDirectory(reader, verify);
            Flags = (ComImageFlags) reader.ReadUInt32();
            EntryPointToken_or_RVA = reader.ReadUInt32();
            Resources = new ImageDataDirectory(reader, verify);
            StrongNameSignature = new ImageDataDirectory(reader, verify);
            CodeManagerTable = new ImageDataDirectory(reader, verify);
            VTableFixups = new ImageDataDirectory(reader, verify);
            ExportAddressTableJumps = new ImageDataDirectory(reader, verify);
            ManagedNativeHeader = new ImageDataDirectory(reader, verify);
            SetEndoffset(reader);
        }

        /// <summary>
        ///     Returns <c>true</c> if it has a native header
        /// </summary>
        public bool HasNativeHeader => (Flags & ComImageFlags.ILLibrary) != 0;

        /// <summary>
        ///     Returns the IMAGE_COR20_HEADER.cb field
        /// </summary>
        public uint CB
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_COR20_HEADER.MajorRuntimeVersion field
        /// </summary>
        public ushort MajorRuntimeVersion
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_COR20_HEADER.MinorRuntimeVersion field
        /// </summary>
        public ushort MinorRuntimeVersion
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_COR20_HEADER.MetaData field
        /// </summary>
        public ImageDataDirectory MetaData
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_COR20_HEADER.Flags field
        /// </summary>
        public ComImageFlags Flags
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_COR20_HEADER.EntryPointToken/EntryPointTokenRVA field
        /// </summary>
        public uint EntryPointToken_or_RVA
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_COR20_HEADER.Resources field
        /// </summary>
        public ImageDataDirectory Resources
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_COR20_HEADER.StrongNameSignature field
        /// </summary>
        public ImageDataDirectory StrongNameSignature
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_COR20_HEADER.CodeManagerTable field
        /// </summary>
        public ImageDataDirectory CodeManagerTable
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_COR20_HEADER.VTableFixups field
        /// </summary>
        public ImageDataDirectory VTableFixups
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_COR20_HEADER.ExportAddressTableJumps field
        /// </summary>
        public ImageDataDirectory ExportAddressTableJumps
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_COR20_HEADER.ManagedNativeHeader field
        /// </summary>
        public ImageDataDirectory ManagedNativeHeader
        {
            get;
        }
    }
}