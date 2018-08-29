namespace dnlib.W32Resources
{
    /// <summary>
    ///     Base class of <see cref="ResourceDirectory" /> and <see cref="ResourceData" />
    /// </summary>
    public abstract class ResourceDirectoryEntry
    {
        private ResourceName name;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="name">Name</param>
        protected ResourceDirectoryEntry(ResourceName name)
        {
            this.name = name;
        }

        /// <summary>
        ///     Gets/sets the name
        /// </summary>
        public ResourceName Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return name.ToString();
        }
    }
}