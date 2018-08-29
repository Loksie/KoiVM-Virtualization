namespace dnlib.DotNet
{
    /// <summary>
    ///     Represents a public key token
    /// </summary>
    public sealed class PublicKeyToken : PublicKeyBase
    {
        /// <inheritdoc />
        public PublicKeyToken()
        {
        }

        /// <inheritdoc />
        public PublicKeyToken(byte[] data)
            : base(data)
        {
        }

        /// <inheritdoc />
        public PublicKeyToken(string hexString)
            : base(hexString)
        {
        }

        /// <summary>
        ///     Gets the <see cref="PublicKeyToken" />
        /// </summary>
        public override PublicKeyToken Token => this;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if(this == obj)
                return true;
            var other = obj as PublicKeyToken;
            if(other == null)
                return false;
            return Utils.Equals(Data, other.Data);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Utils.GetHashCode(Data);
        }
    }
}