#region

using System;
using System.Threading;
using dnlib.DotNet.MD;

#endregion

namespace dnlib.DotNet
{
    /// <summary>
    ///     A high-level representation of a row in the GenericParamConstraint table
    /// </summary>
    public abstract class GenericParamConstraint : IHasCustomAttribute, IContainsGenericParameter
    {
        /// <summary />
        protected ITypeDefOrRef constraint;

        /// <summary />
        protected CustomAttributeCollection customAttributes;

        /// <summary />
        protected GenericParam owner;

        /// <summary>
        ///     The row id in its table
        /// </summary>
        protected uint rid;

        /// <summary>
        ///     Gets the owner generic param
        /// </summary>
        public GenericParam Owner
        {
            get { return owner; }
            internal set { owner = value; }
        }

        /// <summary>
        ///     From column GenericParamConstraint.Constraint
        /// </summary>
        public ITypeDefOrRef Constraint
        {
            get { return constraint; }
            set { constraint = value; }
        }

        bool IContainsGenericParameter.ContainsGenericParameter => TypeHelper.ContainsGenericParameter(this);

        /// <inheritdoc />
        public MDToken MDToken => new MDToken(Table.GenericParamConstraint, rid);

        /// <inheritdoc />
        public uint Rid
        {
            get { return rid; }
            set { rid = value; }
        }

        /// <inheritdoc />
        public int HasCustomAttributeTag => 20;

        /// <summary>
        ///     Gets all custom attributes
        /// </summary>
        public CustomAttributeCollection CustomAttributes
        {
            get
            {
                if(customAttributes == null)
                    InitializeCustomAttributes();
                return customAttributes;
            }
        }

        /// <inheritdoc />
        public bool HasCustomAttributes => CustomAttributes.Count > 0;

        /// <summary>Initializes <see cref="customAttributes" /></summary>
        protected virtual void InitializeCustomAttributes()
        {
            Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
        }
    }

    /// <summary>
    ///     A GenericParamConstraintAssembly row created by the user and not present in the original .NET file
    /// </summary>
    public class GenericParamConstraintUser : GenericParamConstraint
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        public GenericParamConstraintUser()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="constraint">The constraint</param>
        public GenericParamConstraintUser(ITypeDefOrRef constraint)
        {
            this.constraint = constraint;
        }
    }

    /// <summary>
    ///     Created from a row in the GenericParamConstraint table
    /// </summary>
    internal sealed class GenericParamConstraintMD : GenericParamConstraint, IMDTokenProviderMD
    {
        /// <summary>The module where this instance is located</summary>
        private readonly ModuleDefMD readerModule;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="readerModule">The module which contains this <c>GenericParamConstraint</c> row</param>
        /// <param name="rid">Row ID</param>
        /// <param name="gpContext">Generic parameter context</param>
        /// <exception cref="ArgumentNullException">If <paramref name="readerModule" /> is <c>null</c></exception>
        /// <exception cref="ArgumentException">If <paramref name="rid" /> is invalid</exception>
        public GenericParamConstraintMD(ModuleDefMD readerModule, uint rid, GenericParamContext gpContext)
        {
#if DEBUG
            if(readerModule == null)
                throw new ArgumentNullException("readerModule");
            if(readerModule.TablesStream.GenericParamConstraintTable.IsInvalidRID(rid))
                throw new BadImageFormatException(string.Format("GenericParamConstraint rid {0} does not exist", rid));
#endif
            OrigRid = rid;
            this.rid = rid;
            this.readerModule = readerModule;
            var constraint = readerModule.TablesStream.ReadGenericParamConstraintRow2(OrigRid);
            this.constraint = readerModule.ResolveTypeDefOrRef(constraint, gpContext);
            owner = readerModule.GetOwner(this);
        }

        /// <inheritdoc />
        public uint OrigRid
        {
            get;
        }

        /// <inheritdoc />
        protected override void InitializeCustomAttributes()
        {
            var list = readerModule.MetaData.GetCustomAttributeRidList(Table.GenericParamConstraint, OrigRid);
            var tmp = new CustomAttributeCollection((int) list.Length, list, (list2, index) => readerModule.ReadCustomAttribute(((RidList) list2)[index]));
            Interlocked.CompareExchange(ref customAttributes, tmp, null);
        }

        internal GenericParamConstraintMD InitializeAll()
        {
            MemberMDInitializer.Initialize(Owner);
            MemberMDInitializer.Initialize(Constraint);
            MemberMDInitializer.Initialize(CustomAttributes);
            return this;
        }
    }
}