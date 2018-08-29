#region

using System;
using System.IO;

#endregion

namespace dnlib.DotNet.Writer
{
    /// <summary>
    ///     Writes field marshal blobs
    /// </summary>
    public struct MarshalBlobWriter : IDisposable, IFullNameCreatorHelper
    {
        private readonly ModuleDef module;
        private readonly MemoryStream outStream;
        private readonly BinaryWriter writer;
        private readonly IWriterError helper;

        /// <summary>
        ///     Creates a field marshal blob from <paramref name="marshalType" />
        /// </summary>
        /// <param name="module">Owner module</param>
        /// <param name="marshalType">Marshal type</param>
        /// <param name="helper">Helps this class</param>
        /// <returns>
        ///     A field marshal blob or <c>null</c> if <paramref name="marshalType" /> is
        ///     <c>null</c>
        /// </returns>
        public static byte[] Write(ModuleDef module, MarshalType marshalType, IWriterError helper)
        {
            using(var writer = new MarshalBlobWriter(module, helper))
            {
                return writer.Write(marshalType);
            }
        }

        private MarshalBlobWriter(ModuleDef module, IWriterError helper)
        {
            this.module = module;
            outStream = new MemoryStream();
            writer = new BinaryWriter(outStream);
            this.helper = helper;
        }

        private byte[] Write(MarshalType marshalType)
        {
            if(marshalType == null)
                return null;

            var type = marshalType.NativeType;
            if(type != NativeType.RawBlob)
            {
                if((uint) type > byte.MaxValue)
                    helper.Error("Invalid MarshalType.NativeType");
                writer.Write((byte) type);
            }
            var canWrite = true;
            switch(type)
            {
                case NativeType.FixedSysString:
                    var fixedSysString = (FixedSysStringMarshalType) marshalType;
                    if(fixedSysString.IsSizeValid)
                        WriteCompressedUInt32((uint) fixedSysString.Size);
                    break;

                case NativeType.SafeArray:
                    var safeArray = (SafeArrayMarshalType) marshalType;
                    if(UpdateCanWrite(safeArray.IsVariantTypeValid, "VariantType", ref canWrite))
                        WriteCompressedUInt32((uint) safeArray.VariantType);
                    if(UpdateCanWrite(safeArray.IsUserDefinedSubTypeValid, "UserDefinedSubType", ref canWrite))
                        Write(safeArray.UserDefinedSubType.AssemblyQualifiedName);
                    break;

                case NativeType.FixedArray:
                    var fixedArray = (FixedArrayMarshalType) marshalType;
                    if(UpdateCanWrite(fixedArray.IsSizeValid, "Size", ref canWrite))
                        WriteCompressedUInt32((uint) fixedArray.Size);
                    if(UpdateCanWrite(fixedArray.IsElementTypeValid, "ElementType", ref canWrite))
                        WriteCompressedUInt32((uint) fixedArray.ElementType);
                    break;

                case NativeType.Array:
                    var array = (ArrayMarshalType) marshalType;
                    if(UpdateCanWrite(array.IsElementTypeValid, "ElementType", ref canWrite))
                        WriteCompressedUInt32((uint) array.ElementType);
                    if(UpdateCanWrite(array.IsParamNumberValid, "ParamNumber", ref canWrite))
                        WriteCompressedUInt32((uint) array.ParamNumber);
                    if(UpdateCanWrite(array.IsSizeValid, "Size", ref canWrite))
                        WriteCompressedUInt32((uint) array.Size);
                    if(UpdateCanWrite(array.IsFlagsValid, "Flags", ref canWrite))
                        WriteCompressedUInt32((uint) array.Flags);
                    break;

                case NativeType.CustomMarshaler:
                    var custMarshaler = (CustomMarshalType) marshalType;
                    Write(custMarshaler.Guid);
                    Write(custMarshaler.NativeTypeName);
                    var cm = custMarshaler.CustomMarshaler;
                    var cmName = cm == null ? string.Empty : FullNameCreator.AssemblyQualifiedName(cm, this);
                    Write(cmName);
                    Write(custMarshaler.Cookie);
                    break;

                case NativeType.IUnknown:
                case NativeType.IDispatch:
                case NativeType.IntF:
                    var iface = (InterfaceMarshalType) marshalType;
                    if(iface.IsIidParamIndexValid)
                        WriteCompressedUInt32((uint) iface.IidParamIndex);
                    break;

                case NativeType.RawBlob:
                    var data = ((RawMarshalType) marshalType).Data;
                    if(data != null)
                        writer.Write(data);
                    break;

                default:
                    break;
            }

            writer.Flush();
            return outStream.ToArray();
        }

        private bool UpdateCanWrite(bool isValid, string field, ref bool canWriteMore)
        {
            if(!canWriteMore)
            {
                if(isValid)
                    helper.Error(string.Format("MarshalType field {0} is valid even though a previous field was invalid", field));
                return canWriteMore;
            }

            if(!isValid)
                canWriteMore = false;

            return canWriteMore;
        }

        private uint WriteCompressedUInt32(uint value)
        {
            return writer.WriteCompressedUInt32(helper, value);
        }

        private void Write(UTF8String s)
        {
            writer.Write(helper, s);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if(outStream != null)
                outStream.Dispose();
        }

        bool IFullNameCreatorHelper.MustUseAssemblyName(IType type)
        {
            return FullNameCreator.MustUseAssemblyName(module, type);
        }
    }
}