#region

using System;
using dnlib.IO;

#endregion

namespace dnlib.DotNet
{
    /// <summary>
    ///     Reads <see cref="MarshalType" />s
    /// </summary>
    public struct MarshalBlobReader : IDisposable
    {
        private readonly ModuleDef module;
        private readonly IBinaryReader reader;
        private readonly GenericParamContext gpContext;

        /// <summary>
        ///     Reads a <see cref="MarshalType" /> from the <c>#Blob</c> heap
        /// </summary>
        /// <param name="module">Module</param>
        /// <param name="sig">Blob offset</param>
        /// <returns>A new <see cref="MarshalType" /> instance</returns>
        public static MarshalType Read(ModuleDefMD module, uint sig)
        {
            return Read(module, module.BlobStream.CreateStream(sig), new GenericParamContext());
        }

        /// <summary>
        ///     Reads a <see cref="MarshalType" /> from the <c>#Blob</c> heap
        /// </summary>
        /// <param name="module">Module</param>
        /// <param name="sig">Blob offset</param>
        /// <param name="gpContext">Generic parameter context</param>
        /// <returns>A new <see cref="MarshalType" /> instance</returns>
        public static MarshalType Read(ModuleDefMD module, uint sig, GenericParamContext gpContext)
        {
            return Read(module, module.BlobStream.CreateStream(sig), gpContext);
        }

        /// <summary>
        ///     Reads a <see cref="MarshalType" /> from <paramref name="data" />
        /// </summary>
        /// <param name="module">Owner module</param>
        /// <param name="data">Marshal data</param>
        /// <returns>A new <see cref="MarshalType" /> instance</returns>
        public static MarshalType Read(ModuleDef module, byte[] data)
        {
            return Read(module, MemoryImageStream.Create(data), new GenericParamContext());
        }

        /// <summary>
        ///     Reads a <see cref="MarshalType" /> from <paramref name="data" />
        /// </summary>
        /// <param name="module">Owner module</param>
        /// <param name="data">Marshal data</param>
        /// <param name="gpContext">Generic parameter context</param>
        /// <returns>A new <see cref="MarshalType" /> instance</returns>
        public static MarshalType Read(ModuleDef module, byte[] data, GenericParamContext gpContext)
        {
            return Read(module, MemoryImageStream.Create(data), gpContext);
        }

        /// <summary>
        ///     Reads a <see cref="MarshalType" /> from <see cref="reader" />
        /// </summary>
        /// <param name="module">Owner module</param>
        /// <param name="reader">A reader that will be owned by us</param>
        /// <returns>A new <see cref="MarshalType" /> instance</returns>
        public static MarshalType Read(ModuleDef module, IBinaryReader reader)
        {
            return Read(module, reader, new GenericParamContext());
        }

        /// <summary>
        ///     Reads a <see cref="MarshalType" /> from <see cref="reader" />
        /// </summary>
        /// <param name="module">Owner module</param>
        /// <param name="reader">A reader that will be owned by us</param>
        /// <param name="gpContext">Generic parameter context</param>
        /// <returns>A new <see cref="MarshalType" /> instance</returns>
        public static MarshalType Read(ModuleDef module, IBinaryReader reader, GenericParamContext gpContext)
        {
            using(var marshalReader = new MarshalBlobReader(module, reader, gpContext))
            {
                return marshalReader.Read();
            }
        }

        private MarshalBlobReader(ModuleDef module, IBinaryReader reader, GenericParamContext gpContext)
        {
            this.module = module;
            this.reader = reader;
            this.gpContext = gpContext;
        }

        private MarshalType Read()
        {
            MarshalType returnValue;
            try
            {
                var nativeType = (NativeType) reader.ReadByte();
                NativeType nt;
                int size;
                switch(nativeType)
                {
                    case NativeType.FixedSysString:
                        size = CanRead() ? (int) reader.ReadCompressedUInt32() : -1;
                        returnValue = new FixedSysStringMarshalType(size);
                        break;

                    case NativeType.SafeArray:
                        var vt = CanRead() ? (VariantType) reader.ReadCompressedUInt32() : VariantType.NotInitialized;
                        var udtName = CanRead() ? ReadUTF8String() : null;
                        var udtRef = (object) udtName == null ? null : TypeNameParser.ParseReflection(module, UTF8String.ToSystemStringOrEmpty(udtName), null, gpContext);
                        returnValue = new SafeArrayMarshalType(vt, udtRef);
                        break;

                    case NativeType.FixedArray:
                        size = CanRead() ? (int) reader.ReadCompressedUInt32() : -1;
                        nt = CanRead() ? (NativeType) reader.ReadCompressedUInt32() : NativeType.NotInitialized;
                        returnValue = new FixedArrayMarshalType(size, nt);
                        break;

                    case NativeType.Array:
                        nt = CanRead() ? (NativeType) reader.ReadCompressedUInt32() : NativeType.NotInitialized;
                        var paramNum = CanRead() ? (int) reader.ReadCompressedUInt32() : -1;
                        size = CanRead() ? (int) reader.ReadCompressedUInt32() : -1;
                        var flags = CanRead() ? (int) reader.ReadCompressedUInt32() : -1;
                        returnValue = new ArrayMarshalType(nt, paramNum, size, flags);
                        break;

                    case NativeType.CustomMarshaler:
                        var guid = ReadUTF8String();
                        var nativeTypeName = ReadUTF8String();
                        var custMarshalerName = ReadUTF8String();
                        var cmRef = TypeNameParser.ParseReflection(module, UTF8String.ToSystemStringOrEmpty(custMarshalerName), new CAAssemblyRefFinder(module), gpContext);
                        var cookie = ReadUTF8String();
                        returnValue = new CustomMarshalType(guid, nativeTypeName, cmRef, cookie);
                        break;

                    case NativeType.IUnknown:
                    case NativeType.IDispatch:
                    case NativeType.IntF:
                        var iidParamIndex = CanRead() ? (int) reader.ReadCompressedUInt32() : -1;
                        return new InterfaceMarshalType(nativeType, iidParamIndex);

                    default:
                        returnValue = new MarshalType(nativeType);
                        break;
                }
            }
            catch
            {
                returnValue = new RawMarshalType(reader.ReadAllBytes());
            }

            return returnValue;
        }

        private bool CanRead()
        {
            return reader.Position < reader.Length;
        }

        private UTF8String ReadUTF8String()
        {
            var len = reader.ReadCompressedUInt32();
            return len == 0 ? UTF8String.Empty : new UTF8String(reader.ReadBytes((int) len));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if(reader != null)
                reader.Dispose();
        }
    }
}