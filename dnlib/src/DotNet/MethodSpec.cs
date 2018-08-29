#region

using System;
using System.Threading;
using dnlib.DotNet.MD;

#endregion

namespace dnlib.DotNet
{
    /// <summary>
    ///     A high-level representation of a row in the MethodSpec table
    /// </summary>
    public abstract class MethodSpec : IHasCustomAttribute, IMethod, IContainsGenericParameter
    {
        /// <summary />
        protected CustomAttributeCollection customAttributes;

        /// <summary />
        protected CallingConventionSig instantiation;

        /// <summary />
        protected IMethodDefOrRef method;

        /// <summary>
        ///     The row id in its table
        /// </summary>
        protected uint rid;

        /// <summary>
        ///     From column MethodSpec.Method
        /// </summary>
        public IMethodDefOrRef Method
        {
            get { return method; }
            set { method = value; }
        }

        /// <summary>
        ///     From column MethodSpec.Instantiation
        /// </summary>
        public CallingConventionSig Instantiation
        {
            get { return instantiation; }
            set { instantiation = value; }
        }

        /// <summary>
        ///     Gets/sets the generic instance method sig
        /// </summary>
        public GenericInstMethodSig GenericInstMethodSig
        {
            get { return instantiation as GenericInstMethodSig; }
            set { instantiation = value; }
        }

        bool IContainsGenericParameter.ContainsGenericParameter => TypeHelper.ContainsGenericParameter(this);

        /// <inheritdoc />
        public MDToken MDToken => new MDToken(Table.MethodSpec, rid);

        /// <inheritdoc />
        public uint Rid
        {
            get { return rid; }
            set { rid = value; }
        }

        /// <inheritdoc />
        public int HasCustomAttributeTag => 21;

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

        /// <inheritdoc />
        MethodSig IMethod.MethodSig
        {
            get
            {
                var m = method;
                return m == null ? null : m.MethodSig;
            }
            set
            {
                var m = method;
                if(m != null)
                    m.MethodSig = value;
            }
        }

        /// <inheritdoc />
        public UTF8String Name
        {
            get
            {
                var m = method;
                return m == null ? UTF8String.Empty : m.Name;
            }
            set
            {
                var m = method;
                if(m != null)
                    m.Name = value;
            }
        }

        /// <inheritdoc />
        public ITypeDefOrRef DeclaringType
        {
            get
            {
                var m = method;
                return m == null ? null : m.DeclaringType;
            }
        }

        /// <inheritdoc />
        int IGenericParameterProvider.NumberOfGenericParameters
        {
            get
            {
                var sig = GenericInstMethodSig;
                return sig == null ? 0 : sig.GenericArguments.Count;
            }
        }

        /// <inheritdoc />
        public ModuleDef Module
        {
            get
            {
                var m = method;
                return m == null ? null : m.Module;
            }
        }

        /// <summary>
        ///     Gets the full name
        /// </summary>
        public string FullName
        {
            get
            {
                var gims = GenericInstMethodSig;
                var methodGenArgs = gims == null ? null : gims.GenericArguments;
                var m = method;
                var methodDef = m as MethodDef;
                if(methodDef != null)
                {
                    var declaringType = methodDef.DeclaringType;
                    return FullNameCreator.MethodFullName(declaringType == null ? null : declaringType.FullName, methodDef.Name, methodDef.MethodSig, null, methodGenArgs);
                }

                var memberRef = m as MemberRef;
                if(memberRef != null)
                {
                    var methodSig = memberRef.MethodSig;
                    if(methodSig != null)
                    {
                        var tsOwner = memberRef.Class as TypeSpec;
                        var gis = tsOwner == null ? null : tsOwner.TypeSig as GenericInstSig;
                        var typeGenArgs = gis == null ? null : gis.GenericArguments;
                        return FullNameCreator.MethodFullName(memberRef.GetDeclaringTypeFullName(), memberRef.Name, methodSig, typeGenArgs, methodGenArgs);
                    }
                }

                return string.Empty;
            }
        }

        bool IIsTypeOrMethod.IsType => false;

        bool IIsTypeOrMethod.IsMethod => true;

        bool IMemberRef.IsField => false;

        bool IMemberRef.IsTypeSpec => false;

        bool IMemberRef.IsTypeRef => false;

        bool IMemberRef.IsTypeDef => false;

        bool IMemberRef.IsMethodSpec => true;

        bool IMemberRef.IsMethodDef => false;

        bool IMemberRef.IsMemberRef => false;

        bool IMemberRef.IsFieldDef => false;

        bool IMemberRef.IsPropertyDef => false;

        bool IMemberRef.IsEventDef => false;

        bool IMemberRef.IsGenericParam => false;

        /// <summary>Initializes <see cref="customAttributes" /></summary>
        protected virtual void InitializeCustomAttributes()
        {
            Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }
    }

    /// <summary>
    ///     A MethodSpec row created by the user and not present in the original .NET file
    /// </summary>
    public class MethodSpecUser : MethodSpec
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        public MethodSpecUser()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="method">The generic method</param>
        public MethodSpecUser(IMethodDefOrRef method)
            : this(method, null)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="method">The generic method</param>
        /// <param name="sig">The instantiated method sig</param>
        public MethodSpecUser(IMethodDefOrRef method, GenericInstMethodSig sig)
        {
            this.method = method;
            instantiation = sig;
        }
    }

    /// <summary>
    ///     Created from a row in the MethodSpec table
    /// </summary>
    internal sealed class MethodSpecMD : MethodSpec, IMDTokenProviderMD
    {
        /// <summary>The module where this instance is located</summary>
        private readonly ModuleDefMD readerModule;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="readerModule">The module which contains this <c>MethodSpec</c> row</param>
        /// <param name="rid">Row ID</param>
        /// <param name="gpContext">Generic parameter context</param>
        /// <exception cref="ArgumentNullException">If <paramref name="readerModule" /> is <c>null</c></exception>
        /// <exception cref="ArgumentException">If <paramref name="rid" /> is invalid</exception>
        public MethodSpecMD(ModuleDefMD readerModule, uint rid, GenericParamContext gpContext)
        {
#if DEBUG
            if(readerModule == null)
                throw new ArgumentNullException("readerModule");
            if(readerModule.TablesStream.MethodSpecTable.IsInvalidRID(rid))
                throw new BadImageFormatException(string.Format("MethodSpec rid {0} does not exist", rid));
#endif
            OrigRid = rid;
            this.rid = rid;
            this.readerModule = readerModule;
            uint method;
            var instantiation = readerModule.TablesStream.ReadMethodSpecRow(OrigRid, out method);
            this.method = readerModule.ResolveMethodDefOrRef(method, gpContext);
            this.instantiation = readerModule.ReadSignature(instantiation, gpContext);
        }

        /// <inheritdoc />
        public uint OrigRid
        {
            get;
        }

        /// <inheritdoc />
        protected override void InitializeCustomAttributes()
        {
            var list = readerModule.MetaData.GetCustomAttributeRidList(Table.MethodSpec, OrigRid);
            var tmp = new CustomAttributeCollection((int) list.Length, list, (list2, index) => readerModule.ReadCustomAttribute(((RidList) list2)[index]));
            Interlocked.CompareExchange(ref customAttributes, tmp, null);
        }
    }
}