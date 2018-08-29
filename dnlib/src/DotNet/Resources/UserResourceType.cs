namespace dnlib.DotNet.Resources
{
    /// <summary>
    ///     User resource type
    /// </summary>
    public sealed class UserResourceType
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="name">Full name including assembly of type</param>
        /// <param name="code">User type code</param>
        public UserResourceType(string name, ResourceTypeCode code)
        {
            Name = name;
            Code = code;
        }

        /// <summary>
        ///     Full name including assembly of type
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        ///     User type code
        /// </summary>
        public ResourceTypeCode Code
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0:X2} {1}", (int) Code, Name);
        }
    }
}