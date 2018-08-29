#region

using System;
using System.Threading;
using dnlib.DotNet.MD;

#endregion

namespace dnlib.DotNet
{
    /// <summary>
    ///     A high-level representation of a row in the ModuleRef table
    /// </summary>
    public abstract class ModuleRef : IHasCustomAttribute, IMemberRefParent, IResolutionScope, IModule, IOwnerModule
    {
        /// <summary />
        protected CustomAttributeCollection customAttributes;

        /// <summary>
        ///     The owner module
        /// </summary>
        protected ModuleDef module;

        /// <summary>Name</summary>
        protected UTF8String name;

        /// <summary>
        ///     The row id in its table
        /// </summary>
        protected uint rid;

        /// <summary>
        ///     Gets the definition module, i.e., the module which it references, or <c>null</c>
        ///     if the module can't be found.
        /// </summary>
        public ModuleDef DefinitionModule
        {
            get
            {
                if(module == null)
                    return null;
                var n = name;
                if(UTF8String.CaseInsensitiveEquals(n, module.Name))
                    return module;
                var asm = DefinitionAssembly;
                return asm == null ? null : asm.FindModule(n);
            }
        }

        /// <summary>
        ///     Gets the definition assembly, i.e., the assembly of the module it references, or
        ///     <c>null</c> if the assembly can't be found.
        /// </summary>
        public AssemblyDef DefinitionAssembly => module == null ? null : module.Assembly;

        /// <inheritdoc />
        public MDToken MDToken => new MDToken(Table.ModuleRef, rid);

        /// <inheritdoc />
        public uint Rid
        {
            get { return rid; }
            set { rid = value; }
        }

        /// <inheritdoc />
        public int HasCustomAttributeTag => 12;

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
        public int MemberRefParentTag => 2;

        /// <summary>
        ///     From column ModuleRef.Name
        /// </summary>
        public UTF8String Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <inheritdoc />
        public string FullName => UTF8String.ToSystemStringOrEmpty(name);

        /// <inheritdoc />
        public ScopeType ScopeType => ScopeType.ModuleRef;

        /// <inheritdoc />
        public string ScopeName => FullName;

        /// <inheritdoc />
        public ModuleDef Module => module;

        /// <inheritdoc />
        public int ResolutionScopeTag => 1;

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
    ///     A ModuleRef row created by the user and not present in the original .NET file
    /// </summary>
    public class ModuleRefUser : ModuleRef
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="module">Owner module</param>
        public ModuleRefUser(ModuleDef module)
            : this(module, UTF8String.Empty)
        {
        }

        /// <summary>
        ///     Constructor
        ///     <param name="module">Owner module</param>
        /// </summary>
        /// <param name="name">Module name</param>
        public ModuleRefUser(ModuleDef module, UTF8String name)
        {
            this.module = module;
            this.name = name;
        }
    }

    /// <summary>
    ///     Created from a row in the ModuleRef table
    /// </summary>
    internal sealed class ModuleRefMD : ModuleRef, IMDTokenProviderMD
    {
        /// <summary>The module where this instance is located</summary>
        private readonly ModuleDefMD readerModule;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="readerModule">The module which contains this <c>ModuleRef</c> row</param>
        /// <param name="rid">Row ID</param>
        /// <exception cref="ArgumentNullException">If <paramref name="readerModule" /> is <c>null</c></exception>
        /// <exception cref="ArgumentException">If <paramref name="rid" /> is invalid</exception>
        public ModuleRefMD(ModuleDefMD readerModule, uint rid)
        {
#if DEBUG
            if(readerModule == null)
                throw new ArgumentNullException("readerModule");
            if(readerModule.TablesStream.ModuleRefTable.IsInvalidRID(rid))
                throw new BadImageFormatException(string.Format("ModuleRef rid {0} does not exist", rid));
#endif
            OrigRid = rid;
            this.rid = rid;
            this.readerModule = readerModule;
            module = readerModule;
            var name = readerModule.TablesStream.ReadModuleRefRow2(OrigRid);
            this.name = readerModule.StringsStream.ReadNoNull(name);
        }

        /// <inheritdoc />
        public uint OrigRid
        {
            get;
        }

        /// <inheritdoc />
        protected override void InitializeCustomAttributes()
        {
            var list = readerModule.MetaData.GetCustomAttributeRidList(Table.ModuleRef, OrigRid);
            var tmp = new CustomAttributeCollection((int) list.Length, list, (list2, index) => readerModule.ReadCustomAttribute(((RidList) list2)[index]));
            Interlocked.CompareExchange(ref customAttributes, tmp, null);
        }
    }
}