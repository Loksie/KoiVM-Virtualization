#region

using System;
using System.Diagnostics;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.Threading;
#if THREAD_SAFE
using ThreadSafe = dnlib.Threading.Collections;
#else
using ThreadSafe = System.Collections.Generic;

#endif

#endregion

namespace dnlib.DotNet
{
    /// <summary>
    ///     A high-level representation of a row in the DeclSecurity table
    /// </summary>
    [DebuggerDisplay("{Action} Count={SecurityAttributes.Count}")]
    public abstract class DeclSecurity : IHasCustomAttribute
    {
        /// <summary />
        protected SecurityAction action;

        /// <summary />
        protected CustomAttributeCollection customAttributes;

        /// <summary>
        ///     The row id in its table
        /// </summary>
        protected uint rid;

        /// <summary />
        protected ThreadSafe.IList<SecurityAttribute> securityAttributes;

        /// <summary>
        ///     From column DeclSecurity.Action
        /// </summary>
        public SecurityAction Action
        {
            get { return action; }
            set { action = value; }
        }

        /// <summary>
        ///     From column DeclSecurity.PermissionSet
        /// </summary>
        public ThreadSafe.IList<SecurityAttribute> SecurityAttributes
        {
            get
            {
                if(securityAttributes == null)
                    InitializeSecurityAttributes();
                return securityAttributes;
            }
        }

        /// <summary>
        ///     <c>true</c> if <see cref="SecurityAttributes" /> is not empty
        /// </summary>
        public bool HasSecurityAttributes => SecurityAttributes.Count > 0;

        /// <inheritdoc />
        public MDToken MDToken => new MDToken(Table.DeclSecurity, rid);

        /// <inheritdoc />
        public uint Rid
        {
            get { return rid; }
            set { rid = value; }
        }

        /// <inheritdoc />
        public int HasCustomAttributeTag => 8;

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

        /// <summary>Initializes <see cref="securityAttributes" /></summary>
        protected virtual void InitializeSecurityAttributes()
        {
            Interlocked.CompareExchange(ref securityAttributes, ThreadSafeListCreator.Create<SecurityAttribute>(), null);
        }

        /// <summary>Initializes <see cref="customAttributes" /></summary>
        protected virtual void InitializeCustomAttributes()
        {
            Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
        }

        /// <summary>
        ///     Gets the blob data or <c>null</c> if there's none
        /// </summary>
        /// <returns>Blob data or <c>null</c></returns>
        public abstract byte[] GetBlob();

        /// <summary>
        ///     Returns the .NET 1.x XML string or null if it's not a .NET 1.x format
        /// </summary>
        /// <returns></returns>
        public string GetNet1xXmlString()
        {
            return GetNet1xXmlStringInternal(SecurityAttributes);
        }

        internal static string GetNet1xXmlStringInternal(ThreadSafe.IList<SecurityAttribute> secAttrs)
        {
            if(secAttrs == null || secAttrs.Count != 1)
                return null;
            var sa = secAttrs[0];
            if(sa == null || sa.TypeFullName != "System.Security.Permissions.PermissionSetAttribute")
                return null;
            if(sa.NamedArguments.Count != 1)
                return null;
            var na = sa.NamedArguments[0];
            if(na == null || !na.IsProperty || na.Name != "XML")
                return null;
            if(na.ArgumentType.GetElementType() != ElementType.String)
                return null;
            var arg = na.Argument;
            if(arg.Type.GetElementType() != ElementType.String)
                return null;
            var utf8 = arg.Value as UTF8String;
            if((object) utf8 != null)
                return utf8;
            var s = arg.Value as string;
            if(s != null)
                return s;
            return null;
        }
    }

    /// <summary>
    ///     A DeclSecurity row created by the user and not present in the original .NET file
    /// </summary>
    public class DeclSecurityUser : DeclSecurity
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        public DeclSecurityUser()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="action">The security action</param>
        /// <param name="securityAttrs">The security attributes (now owned by this)</param>
        public DeclSecurityUser(SecurityAction action, ThreadSafe.IList<SecurityAttribute> securityAttrs)
        {
            this.action = action;
            securityAttributes = ThreadSafeListCreator.MakeThreadSafe(securityAttrs);
        }

        /// <inheritdoc />
        public override byte[] GetBlob()
        {
            return null;
        }
    }

    /// <summary>
    ///     Created from a row in the DeclSecurity table
    /// </summary>
    internal sealed class DeclSecurityMD : DeclSecurity, IMDTokenProviderMD
    {
        private readonly uint permissionSet;

        /// <summary>The module where this instance is located</summary>
        private readonly ModuleDefMD readerModule;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="readerModule">The module which contains this <c>DeclSecurity</c> row</param>
        /// <param name="rid">Row ID</param>
        /// <exception cref="ArgumentNullException">If <paramref name="readerModule" /> is <c>null</c></exception>
        /// <exception cref="ArgumentException">If <paramref name="rid" /> is invalid</exception>
        public DeclSecurityMD(ModuleDefMD readerModule, uint rid)
        {
#if DEBUG
            if(readerModule == null)
                throw new ArgumentNullException("readerModule");
            if(readerModule.TablesStream.DeclSecurityTable.IsInvalidRID(rid))
                throw new BadImageFormatException(string.Format("DeclSecurity rid {0} does not exist", rid));
#endif
            OrigRid = rid;
            this.rid = rid;
            this.readerModule = readerModule;
            permissionSet = readerModule.TablesStream.ReadDeclSecurityRow(OrigRid, out action);
        }

        /// <inheritdoc />
        public uint OrigRid
        {
            get;
        }

        /// <inheritdoc />
        protected override void InitializeSecurityAttributes()
        {
            var gpContext = new GenericParamContext();
            var tmp = DeclSecurityReader.Read(readerModule, permissionSet, gpContext);
            Interlocked.CompareExchange(ref securityAttributes, tmp, null);
        }

        /// <inheritdoc />
        protected override void InitializeCustomAttributes()
        {
            var list = readerModule.MetaData.GetCustomAttributeRidList(Table.DeclSecurity, OrigRid);
            var tmp = new CustomAttributeCollection((int) list.Length, list, (list2, index) => readerModule.ReadCustomAttribute(((RidList) list2)[index]));
            Interlocked.CompareExchange(ref customAttributes, tmp, null);
        }

        /// <inheritdoc />
        public override byte[] GetBlob()
        {
            return readerModule.BlobStream.Read(permissionSet);
        }
    }
}