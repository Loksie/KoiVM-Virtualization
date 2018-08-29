#region

using System;
using System.Diagnostics;
using dnlib.IO;

#endregion

namespace dnlib.PE
{
    /// <summary>
    ///     Represents the IMAGE_DATA_DIRECTORY PE section
    /// </summary>
    [DebuggerDisplay("{VirtualAddress} {Size}")]
    public sealed class ImageDataDirectory : FileSection
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        public ImageDataDirectory()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="reader">PE file reader pointing to the start of this section</param>
        /// <param name="verify">Verify section</param>
        /// <exception cref="BadImageFormatException">Thrown if verification fails</exception>
        public ImageDataDirectory(IImageStream reader, bool verify)
        {
            SetStartOffset(reader);
            VirtualAddress = (RVA) reader.ReadUInt32();
            Size = reader.ReadUInt32();
            SetEndoffset(reader);
        }

        /// <summary>
        ///     Returns the IMAGE_DATA_DIRECTORY.VirtualAddress field
        /// </summary>
        public RVA VirtualAddress
        {
            get;
        }

        /// <summary>
        ///     Returns the IMAGE_DATA_DIRECTORY.Size field
        /// </summary>
        public uint Size
        {
            get;
        }
    }
}