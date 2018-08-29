#region

using System;
using System.Threading;
using dnlib.DotNet.MD;

#endregion

namespace dnlib.DotNet
{
    /// <summary>
    ///     A high-level representation of a row in the StandAloneSig table
    /// </summary>
    public abstract class StandAloneSig : IHasCustomAttribute, IContainsGenericParameter
    {
        /// <summary />
        protected CustomAttributeCollection customAttributes;

        /// <summary>
        ///     The row id in its table
        /// </summary>
        protected uint rid;

        /// <summary />
        protected CallingConventionSig signature;

        /// <summary>
        ///     From column StandAloneSig.Signature
        /// </summary>
        public CallingConventionSig Signature
        {
            get { return signature; }
            set { signature = value; }
        }

        /// <summary>
        ///     Gets/sets the method sig
        /// </summary>
        public MethodSig MethodSig
        {
            get { return signature as MethodSig; }
            set { signature = value; }
        }

        /// <summary>
        ///     Gets/sets the locals sig
        /// </summary>
        public LocalSig LocalSig
        {
            get { return signature as LocalSig; }
            set { signature = value; }
        }

        /// <inheritdoc />
        public bool ContainsGenericParameter => TypeHelper.ContainsGenericParameter(this);

        /// <inheritdoc />
        public MDToken MDToken => new MDToken(Table.StandAloneSig, rid);

        /// <inheritdoc />
        public uint Rid
        {
            get { return rid; }
            set { rid = value; }
        }

        /// <inheritdoc />
        public int HasCustomAttributeTag => 11;

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
    ///     A StandAloneSig row created by the user and not present in the original .NET file
    /// </summary>
    public class StandAloneSigUser : StandAloneSig
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        public StandAloneSigUser()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="localSig">A locals sig</param>
        public StandAloneSigUser(LocalSig localSig)
        {
            signature = localSig;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="methodSig">A method sig</param>
        public StandAloneSigUser(MethodSig methodSig)
        {
            signature = methodSig;
        }
    }

    /// <summary>
    ///     Created from a row in the StandAloneSig table
    /// </summary>
    internal sealed class StandAloneSigMD : StandAloneSig, IMDTokenProviderMD
    {
        /// <summary>The module where this instance is located</summary>
        private readonly ModuleDefMD readerModule;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="readerModule">The module which contains this <c>StandAloneSig</c> row</param>
        /// <param name="rid">Row ID</param>
        /// <param name="gpContext">Generic parameter context</param>
        /// <exception cref="ArgumentNullException">If <paramref name="readerModule" /> is <c>null</c></exception>
        /// <exception cref="ArgumentException">If <paramref name="rid" /> is invalid</exception>
        public StandAloneSigMD(ModuleDefMD readerModule, uint rid, GenericParamContext gpContext)
        {
#if DEBUG
            if(readerModule == null)
                throw new ArgumentNullException("readerModule");
            if(readerModule.TablesStream.StandAloneSigTable.IsInvalidRID(rid))
                throw new BadImageFormatException(string.Format("StandAloneSig rid {0} does not exist", rid));
#endif
            OrigRid = rid;
            this.rid = rid;
            this.readerModule = readerModule;
            var signature = readerModule.TablesStream.ReadStandAloneSigRow2(OrigRid);
            this.signature = readerModule.ReadSignature(signature, gpContext);
        }

        /// <inheritdoc />
        public uint OrigRid
        {
            get;
        }

        /// <inheritdoc />
        protected override void InitializeCustomAttributes()
        {
            var list = readerModule.MetaData.GetCustomAttributeRidList(Table.StandAloneSig, OrigRid);
            var tmp = new CustomAttributeCollection((int) list.Length, list, (list2, index) => readerModule.ReadCustomAttribute(((RidList) list2)[index]));
            Interlocked.CompareExchange(ref customAttributes, tmp, null);
        }
    }
}