#region

using System.IO;
using System.Runtime.Serialization;
using dnlib.IO;

#endregion

namespace dnlib.DotNet.Resources
{
    /// <summary>
    ///     Base class of all user data
    /// </summary>
    public abstract class UserResourceData : IResourceData
    {
        private readonly UserResourceType type;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="type">User resource type</param>
        public UserResourceData(UserResourceType type)
        {
            this.type = type;
        }

        /// <summary>
        ///     Full name including assembly of type
        /// </summary>
        public string TypeName => type.Name;

        /// <summary>
        ///     User type code
        /// </summary>
        public ResourceTypeCode Code => type.Code;

        /// <inheritdoc />
        public FileOffset StartOffset
        {
            get;
            set;
        }

        /// <inheritdoc />
        public FileOffset EndOffset
        {
            get;
            set;
        }

        /// <inheritdoc />
        public abstract void WriteData(BinaryWriter writer, IFormatter formatter);
    }

    /// <summary>
    ///     Binary data
    /// </summary>
    public sealed class BinaryResourceData : UserResourceData
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="type">User resource type</param>
        /// <param name="data">Raw serialized data</param>
        public BinaryResourceData(UserResourceType type, byte[] data)
            : base(type)
        {
            Data = data;
        }

        /// <summary>
        ///     Gets the raw data
        /// </summary>
        public byte[] Data
        {
            get;
        }

        /// <inheritdoc />
        public override void WriteData(BinaryWriter writer, IFormatter formatter)
        {
            writer.Write(Data);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("Binary: Length: {0}", Data.Length);
        }
    }
}