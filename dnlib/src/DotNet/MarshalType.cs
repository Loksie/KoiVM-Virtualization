#region

using System;

#endregion

namespace dnlib.DotNet
{
    /// <summary>
    ///     Base class of all marshal types
    /// </summary>
    public class MarshalType
    {
        /// <summary>
        ///     The native type
        /// </summary>
        protected readonly NativeType nativeType;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="nativeType">Native type</param>
        public MarshalType(NativeType nativeType)
        {
            this.nativeType = nativeType;
        }

        /// <summary>
        ///     Gets the <see cref="dnlib.DotNet.NativeType" />
        /// </summary>
        public NativeType NativeType => nativeType;

        /// <inheritdoc />
        public override string ToString()
        {
            return nativeType.ToString();
        }
    }

    /// <summary>
    ///     Contains the raw marshal blob data
    /// </summary>
    public sealed class RawMarshalType : MarshalType
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="data">Raw data</param>
        public RawMarshalType(byte[] data)
            : base(NativeType.RawBlob)
        {
            Data = data;
        }

        /// <summary>
        ///     Gets/sets the raw data
        /// </summary>
        public byte[] Data
        {
            get;
            set;
        }
    }

    /// <summary>
    ///     A <see cref="NativeType.FixedSysString" /> marshal type
    /// </summary>
    public sealed class FixedSysStringMarshalType : MarshalType
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        public FixedSysStringMarshalType()
            : this(-1)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="size">Size</param>
        public FixedSysStringMarshalType(int size)
            : base(NativeType.FixedSysString)
        {
            Size = size;
        }

        /// <summary>
        ///     Gets/sets the size
        /// </summary>
        public int Size
        {
            get;
            set;
        }

        /// <summary>
        ///     <c>true</c> if <see cref="Size" /> is valid
        /// </summary>
        public bool IsSizeValid => Size >= 0;

        /// <inheritdoc />
        public override string ToString()
        {
            if(IsSizeValid)
                return string.Format("{0} ({1})", nativeType, Size);
            return string.Format("{0} (<no size>)", nativeType);
        }
    }

    /// <summary>
    ///     A <see cref="NativeType.SafeArray" /> marshal type
    /// </summary>
    public sealed class SafeArrayMarshalType : MarshalType
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        public SafeArrayMarshalType()
            : this(VariantType.NotInitialized, null)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="vt">Variant type</param>
        public SafeArrayMarshalType(VariantType vt)
            : this(vt, null)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="userDefinedSubType">User-defined sub type</param>
        public SafeArrayMarshalType(ITypeDefOrRef userDefinedSubType)
            : this(VariantType.NotInitialized, userDefinedSubType)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="vt">Variant type</param>
        /// <param name="userDefinedSubType">User-defined sub type</param>
        public SafeArrayMarshalType(VariantType vt, ITypeDefOrRef userDefinedSubType)
            : base(NativeType.SafeArray)
        {
            VariantType = vt;
            UserDefinedSubType = userDefinedSubType;
        }

        /// <summary>
        ///     Gets/sets the variant type
        /// </summary>
        public VariantType VariantType
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the user-defined sub type (it's usually <c>null</c>)
        /// </summary>
        public ITypeDefOrRef UserDefinedSubType
        {
            get;
            set;
        }

        /// <summary>
        ///     <c>true</c> if <see cref="VariantType" /> is valid
        /// </summary>
        public bool IsVariantTypeValid => VariantType != VariantType.NotInitialized;

        /// <summary>
        ///     <c>true</c> if <see cref="UserDefinedSubType" /> is valid
        /// </summary>
        public bool IsUserDefinedSubTypeValid => UserDefinedSubType != null;

        /// <inheritdoc />
        public override string ToString()
        {
            var udt = UserDefinedSubType;
            if(udt != null)
                return string.Format("{0} ({1}, {2})", nativeType, VariantType, udt);
            return string.Format("{0} ({1})", nativeType, VariantType);
        }
    }

    /// <summary>
    ///     A <see cref="NativeType.FixedArray" /> marshal type
    /// </summary>
    public sealed class FixedArrayMarshalType : MarshalType
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        public FixedArrayMarshalType()
            : this(0)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="size">Size</param>
        public FixedArrayMarshalType(int size)
            : this(size, NativeType.NotInitialized)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="elementType">Element type</param>
        public FixedArrayMarshalType(int size, NativeType elementType)
            : base(NativeType.FixedArray)
        {
            Size = size;
            ElementType = elementType;
        }

        /// <summary>
        ///     Gets/sets the element type
        /// </summary>
        public NativeType ElementType
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the size
        /// </summary>
        public int Size
        {
            get;
            set;
        }

        /// <summary>
        ///     <c>true</c> if <see cref="ElementType" /> is valid
        /// </summary>
        public bool IsElementTypeValid => ElementType != NativeType.NotInitialized;

        /// <summary>
        ///     <c>true</c> if <see cref="Size" /> is valid
        /// </summary>
        public bool IsSizeValid => Size >= 0;

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0} ({1}, {2})", nativeType, Size, ElementType);
        }
    }

    /// <summary>
    ///     A <see cref="NativeType.Array" /> marshal type
    /// </summary>
    public sealed class ArrayMarshalType : MarshalType
    {
        private const int ntaSizeParamIndexSpecified = 1;

        /// <summary>
        ///     Default constructor
        /// </summary>
        public ArrayMarshalType()
            : this(NativeType.NotInitialized, -1, -1, -1)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="elementType">Element type</param>
        public ArrayMarshalType(NativeType elementType)
            : this(elementType, -1, -1, -1)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="elementType">Element type</param>
        /// <param name="paramNum">Parameter number</param>
        public ArrayMarshalType(NativeType elementType, int paramNum)
            : this(elementType, paramNum, -1, -1)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="elementType">Element type</param>
        /// <param name="paramNum">Parameter number</param>
        /// <param name="numElems">Number of elements</param>
        public ArrayMarshalType(NativeType elementType, int paramNum, int numElems)
            : this(elementType, paramNum, numElems, -1)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="elementType">Element type</param>
        /// <param name="paramNum">Parameter number</param>
        /// <param name="numElems">Number of elements</param>
        /// <param name="flags">Flags</param>
        public ArrayMarshalType(NativeType elementType, int paramNum, int numElems, int flags)
            : base(NativeType.Array)
        {
            ElementType = elementType;
            ParamNumber = paramNum;
            Size = numElems;
            Flags = flags;
        }

        /// <summary>
        ///     Gets/sets the element type
        /// </summary>
        public NativeType ElementType
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the parameter number
        /// </summary>
        public int ParamNumber
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the size of the array
        /// </summary>
        public int Size
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the flags
        /// </summary>
        public int Flags
        {
            get;
            set;
        }

        /// <summary>
        ///     <c>true</c> if <see cref="ElementType" /> is valid
        /// </summary>
        public bool IsElementTypeValid => ElementType != NativeType.NotInitialized;

        /// <summary>
        ///     <c>true</c> if <see cref="ParamNumber" /> is valid
        /// </summary>
        public bool IsParamNumberValid => ParamNumber >= 0;

        /// <summary>
        ///     <c>true</c> if <see cref="Size" /> is valid
        /// </summary>
        public bool IsSizeValid => Size >= 0;

        /// <summary>
        ///     <c>true</c> if <see cref="Flags" /> is valid
        /// </summary>
        public bool IsFlagsValid => Flags >= 0;

        /// <summary>
        ///     <c>true</c> if <c>ntaSizeParamIndexSpecified</c> bit is set, <c>false</c> if it's not
        ///     set or if <see cref="Flags" /> is invalid.
        /// </summary>
        public bool IsSizeParamIndexSpecified => IsFlagsValid && (Flags & ntaSizeParamIndexSpecified) != 0;

        /// <summary>
        ///     <c>true</c> if <c>ntaSizeParamIndexSpecified</c> bit is not set, <c>false</c> if it's
        ///     set or if <see cref="Flags" /> is invalid.
        /// </summary>
        public bool IsSizeParamIndexNotSpecified => IsFlagsValid && (Flags & ntaSizeParamIndexSpecified) == 0;

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0} ({1}, {2}, {3}, {4})", nativeType, ElementType, ParamNumber, Size, Flags);
        }
    }

    /// <summary>
    ///     A <see cref="NativeType.CustomMarshaler" /> marshal type
    /// </summary>
    public sealed class CustomMarshalType : MarshalType
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        public CustomMarshalType()
            : this(null, null, null, null)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="guid">GUID string</param>
        public CustomMarshalType(UTF8String guid)
            : this(guid, null, null, null)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="guid">GUID string</param>
        /// <param name="nativeTypeName">Native type name string</param>
        public CustomMarshalType(UTF8String guid, UTF8String nativeTypeName)
            : this(guid, nativeTypeName, null, null)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="guid">GUID string</param>
        /// <param name="nativeTypeName">Native type name string</param>
        /// <param name="custMarshaler">Custom marshaler name string</param>
        public CustomMarshalType(UTF8String guid, UTF8String nativeTypeName, ITypeDefOrRef custMarshaler)
            : this(guid, nativeTypeName, custMarshaler, null)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="guid">GUID string</param>
        /// <param name="nativeTypeName">Native type name string</param>
        /// <param name="custMarshaler">Custom marshaler name string</param>
        /// <param name="cookie">Cookie string</param>
        public CustomMarshalType(UTF8String guid, UTF8String nativeTypeName, ITypeDefOrRef custMarshaler, UTF8String cookie)
            : base(NativeType.CustomMarshaler)
        {
            Guid = guid;
            NativeTypeName = nativeTypeName;
            CustomMarshaler = custMarshaler;
            Cookie = cookie;
        }

        /// <summary>
        ///     Gets/sets the <c>GUID</c> string
        /// </summary>
        public UTF8String Guid
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the native type name string
        /// </summary>
        public UTF8String NativeTypeName
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the custom marshaler
        /// </summary>
        public ITypeDefOrRef CustomMarshaler
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the cookie string
        /// </summary>
        public UTF8String Cookie
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0} ({1}, {2}, {3}, {4})", nativeType, Guid, NativeTypeName, CustomMarshaler, Cookie);
        }
    }

    /// <summary>
    ///     A <see cref="NativeType.IUnknown" />, <see cref="NativeType.IDispatch" /> or a
    ///     <see cref="NativeType.IntF" /> marshal type
    /// </summary>
    public sealed class InterfaceMarshalType : MarshalType
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="nativeType">Native type</param>
        public InterfaceMarshalType(NativeType nativeType)
            : this(nativeType, -1)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="nativeType">Native type</param>
        /// <param name="iidParamIndex">IID parameter index</param>
        public InterfaceMarshalType(NativeType nativeType, int iidParamIndex)
            : base(nativeType)
        {
            if(nativeType != NativeType.IUnknown &&
               nativeType != NativeType.IDispatch &&
               nativeType != NativeType.IntF)
                throw new ArgumentException("Invalid nativeType");
            IidParamIndex = iidParamIndex;
        }

        /// <summary>
        ///     Gets/sets the IID parameter index
        /// </summary>
        public int IidParamIndex
        {
            get;
            set;
        }

        /// <summary>
        ///     <c>true</c> if <see cref="IidParamIndex" /> is valid
        /// </summary>
        public bool IsIidParamIndexValid => IidParamIndex >= 0;

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0} ({1})", nativeType, IidParamIndex);
        }
    }
}