#region

using System;
using dnlib.IO;
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
    ///     A custom attribute
    /// </summary>
    public sealed class CustomAttribute : ICustomAttribute
    {
        private byte[] blob;
        private IBinaryReader blobReader;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="ctor">Custom attribute constructor</param>
        /// <param name="rawData">Raw custom attribute blob</param>
        public CustomAttribute(ICustomAttributeType ctor, byte[] rawData)
            : this(ctor, null, null, null)
        {
            RawData = rawData;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="ctor">Custom attribute constructor</param>
        public CustomAttribute(ICustomAttributeType ctor)
            : this(ctor, null, null, null)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="ctor">Custom attribute constructor</param>
        /// <param name="arguments">Constructor arguments or <c>null</c> if none</param>
        public CustomAttribute(ICustomAttributeType ctor, ThreadSafe.IEnumerable<CAArgument> arguments)
            : this(ctor, arguments, null)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="ctor">Custom attribute constructor</param>
        /// <param name="namedArguments">Named arguments or <c>null</c> if none</param>
        public CustomAttribute(ICustomAttributeType ctor, ThreadSafe.IEnumerable<CANamedArgument> namedArguments)
            : this(ctor, null, namedArguments)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="ctor">Custom attribute constructor</param>
        /// <param name="arguments">Constructor arguments or <c>null</c> if none</param>
        /// <param name="namedArguments">Named arguments or <c>null</c> if none</param>
        public CustomAttribute(ICustomAttributeType ctor, ThreadSafe.IEnumerable<CAArgument> arguments, ThreadSafe.IEnumerable<CANamedArgument> namedArguments)
            : this(ctor, arguments, namedArguments, null)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="ctor">Custom attribute constructor</param>
        /// <param name="arguments">Constructor arguments or <c>null</c> if none</param>
        /// <param name="namedArguments">Named arguments or <c>null</c> if none</param>
        /// <param name="blobReader">A reader that returns the original custom attribute blob data</param>
        public CustomAttribute(ICustomAttributeType ctor, ThreadSafe.IEnumerable<CAArgument> arguments, ThreadSafe.IEnumerable<CANamedArgument> namedArguments, IBinaryReader blobReader)
        {
            Constructor = ctor;
            ConstructorArguments = arguments == null ? ThreadSafeListCreator.Create<CAArgument>() : ThreadSafeListCreator.Create(arguments);
            NamedArguments = namedArguments == null ? ThreadSafeListCreator.Create<CANamedArgument>() : ThreadSafeListCreator.Create(namedArguments);
            this.blobReader = blobReader;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="ctor">Custom attribute constructor</param>
        /// <param name="arguments">Constructor arguments. The list is now owned by this instance.</param>
        /// <param name="namedArguments">Named arguments. The list is now owned by this instance.</param>
        /// <param name="blobReader">A reader that returns the original custom attribute blob data</param>
        internal CustomAttribute(ICustomAttributeType ctor, ThreadSafe.List<CAArgument> arguments, ThreadSafe.List<CANamedArgument> namedArguments, IBinaryReader blobReader)
        {
            Constructor = ctor;
            ConstructorArguments = arguments == null ? ThreadSafeListCreator.Create<CAArgument>() : ThreadSafeListCreator.MakeThreadSafe(arguments);
            NamedArguments = namedArguments == null ? ThreadSafeListCreator.Create<CANamedArgument>() : ThreadSafeListCreator.MakeThreadSafe(namedArguments);
            this.blobReader = blobReader;
        }

        /// <summary>
        ///     Gets/sets the custom attribute constructor
        /// </summary>
        public ICustomAttributeType Constructor
        {
            get;
            set;
        }

        /// <summary>
        ///     <c>true</c> if the raw custom attribute blob hasn't been parsed
        /// </summary>
        public bool IsRawBlob => RawData != null;

        /// <summary>
        ///     Gets the raw custom attribute blob or <c>null</c> if the CA was successfully parsed.
        /// </summary>
        public byte[] RawData
        {
            get;
        }

        /// <summary>
        ///     Gets all constructor arguments
        /// </summary>
        public ThreadSafe.IList<CAArgument> ConstructorArguments
        {
            get;
        }

        /// <summary>
        ///     <c>true</c> if <see cref="ConstructorArguments" /> is not empty
        /// </summary>
        public bool HasConstructorArguments => ConstructorArguments.Count > 0;

        /// <summary>
        ///     Gets the attribute type
        /// </summary>
        public ITypeDefOrRef AttributeType
        {
            get
            {
                var cat = Constructor;
                return cat == null ? null : cat.DeclaringType;
            }
        }

        /// <summary>
        ///     Gets the full name of the attribute type
        /// </summary>
        public string TypeFullName
        {
            get
            {
                var mrCtor = Constructor as MemberRef;
                if(mrCtor != null)
                    return mrCtor.GetDeclaringTypeFullName() ?? string.Empty;

                var mdCtor = Constructor as MethodDef;
                if(mdCtor != null)
                {
                    var declType = mdCtor.DeclaringType;
                    if(declType != null)
                        return declType.FullName;
                }

                return string.Empty;
            }
        }

        /// <summary>
        ///     Gets all named arguments (field and property values)
        /// </summary>
        public ThreadSafe.IList<CANamedArgument> NamedArguments
        {
            get;
        }

        /// <summary>
        ///     <c>true</c> if <see cref="NamedArguments" /> is not empty
        /// </summary>
        public bool HasNamedArguments => NamedArguments.Count > 0;

        /// <summary>
        ///     Gets all <see cref="CANamedArgument" />s that are field arguments
        /// </summary>
        public ThreadSafe.IEnumerable<CANamedArgument> Fields
        {
            get
            {
                foreach(var namedArg in NamedArguments.GetSafeEnumerable())
                    if(namedArg.IsField)
                        yield return namedArg;
            }
        }

        /// <summary>
        ///     Gets all <see cref="CANamedArgument" />s that are property arguments
        /// </summary>
        public ThreadSafe.IEnumerable<CANamedArgument> Properties
        {
            get
            {
                foreach(var namedArg in NamedArguments.GetSafeEnumerable())
                    if(namedArg.IsProperty)
                        yield return namedArg;
            }
        }

        /// <summary>
        ///     Gets the field named <paramref name="name" />
        /// </summary>
        /// <param name="name">Name of field</param>
        /// <returns>A <see cref="CANamedArgument" /> instance or <c>null</c> if not found</returns>
        public CANamedArgument GetField(string name)
        {
            return GetNamedArgument(name, true);
        }

        /// <summary>
        ///     Gets the field named <paramref name="name" />
        /// </summary>
        /// <param name="name">Name of field</param>
        /// <returns>A <see cref="CANamedArgument" /> instance or <c>null</c> if not found</returns>
        public CANamedArgument GetField(UTF8String name)
        {
            return GetNamedArgument(name, true);
        }

        /// <summary>
        ///     Gets the property named <paramref name="name" />
        /// </summary>
        /// <param name="name">Name of property</param>
        /// <returns>A <see cref="CANamedArgument" /> instance or <c>null</c> if not found</returns>
        public CANamedArgument GetProperty(string name)
        {
            return GetNamedArgument(name, false);
        }

        /// <summary>
        ///     Gets the property named <paramref name="name" />
        /// </summary>
        /// <param name="name">Name of property</param>
        /// <returns>A <see cref="CANamedArgument" /> instance or <c>null</c> if not found</returns>
        public CANamedArgument GetProperty(UTF8String name)
        {
            return GetNamedArgument(name, false);
        }

        /// <summary>
        ///     Gets the property/field named <paramref name="name" />
        /// </summary>
        /// <param name="name">Name of property/field</param>
        /// <param name="isField"><c>true</c> if it's a field, <c>false</c> if it's a property</param>
        /// <returns>A <see cref="CANamedArgument" /> instance or <c>null</c> if not found</returns>
        public CANamedArgument GetNamedArgument(string name, bool isField)
        {
            foreach(var namedArg in NamedArguments.GetSafeEnumerable())
                if(namedArg.IsField == isField && UTF8String.ToSystemStringOrEmpty(namedArg.Name) == name)
                    return namedArg;
            return null;
        }

        /// <summary>
        ///     Gets the property/field named <paramref name="name" />
        /// </summary>
        /// <param name="name">Name of property/field</param>
        /// <param name="isField"><c>true</c> if it's a field, <c>false</c> if it's a property</param>
        /// <returns>A <see cref="CANamedArgument" /> instance or <c>null</c> if not found</returns>
        public CANamedArgument GetNamedArgument(UTF8String name, bool isField)
        {
            foreach(var namedArg in NamedArguments.GetSafeEnumerable())
                if(namedArg.IsField == isField && UTF8String.Equals(namedArg.Name, name))
                    return namedArg;
            return null;
        }

        /// <summary>
        ///     Gets the binary custom attribute data that was used to create this instance.
        /// </summary>
        /// <returns>Blob of this custom attribute</returns>
        public byte[] GetBlob()
        {
            if(RawData != null)
                return RawData;
            if(blob != null)
                return blob;
#if THREAD_SAFE
			if (blobReader != null) {
				lock (this) {
#endif
            if(blobReader != null)
            {
                blob = blobReader.ReadAllBytes();
                blobReader.Dispose();
                blobReader = null;
                return blob;
            }
#if THREAD_SAFE
				}
			}
#endif
            if(blob != null)
                return blob;
            return blob = new byte[0];
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return TypeFullName;
        }
    }

    /// <summary>
    ///     A custom attribute constructor argument
    /// </summary>
    public struct CAArgument : ICloneable
    {
        /// <summary>
        ///     Gets/sets the argument type
        /// </summary>
        public TypeSig Type
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the argument value
        /// </summary>
        public object Value
        {
            get;
            set;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="type">Argument type</param>
        public CAArgument(TypeSig type)
        {
            Type = type;
            Value = null;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="type">Argument type</param>
        /// <param name="value">Argument value</param>
        public CAArgument(TypeSig type, object value)
        {
            Type = type;
            Value = value;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        ///     Clones this instance and any <see cref="CAArgument" />s and <see cref="CANamedArgument" />s
        ///     referenced from this instance.
        /// </summary>
        /// <returns></returns>
        public CAArgument Clone()
        {
            var value = Value;
            if(value is CAArgument)
            {
                value = ((CAArgument) value).Clone();
            }
            else if(value is ThreadSafe.IList<CAArgument>)
            {
                var args = (ThreadSafe.IList<CAArgument>) value;
                var newArgs = ThreadSafeListCreator.Create<CAArgument>(args.Count);
                foreach(var arg in args.GetSafeEnumerable())
                    newArgs.Add(arg.Clone());
                value = newArgs;
            }
            return new CAArgument(Type, value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var v = Value;
            return string.Format("{0} ({1})", v == null ? "null" : v, Type);
        }
    }

    /// <summary>
    ///     A custom attribute field/property argument
    /// </summary>
    public sealed class CANamedArgument : ICloneable
    {
        private CAArgument argument;

        /// <summary>
        ///     Default constructor
        /// </summary>
        public CANamedArgument()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="isField"><c>true</c> if field, <c>false</c> if property</param>
        public CANamedArgument(bool isField)
        {
            IsField = isField;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="isField"><c>true</c> if field, <c>false</c> if property</param>
        /// <param name="type">Field/property type</param>
        public CANamedArgument(bool isField, TypeSig type)
        {
            IsField = isField;
            Type = type;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="isField"><c>true</c> if field, <c>false</c> if property</param>
        /// <param name="type">Field/property type</param>
        /// <param name="name">Name of field/property</param>
        public CANamedArgument(bool isField, TypeSig type, UTF8String name)
        {
            IsField = isField;
            Type = type;
            Name = name;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="isField"><c>true</c> if field, <c>false</c> if property</param>
        /// <param name="type">Field/property type</param>
        /// <param name="name">Name of field/property</param>
        /// <param name="argument">Field/property argument</param>
        public CANamedArgument(bool isField, TypeSig type, UTF8String name, CAArgument argument)
        {
            IsField = isField;
            Type = type;
            Name = name;
            this.argument = argument;
        }

        /// <summary>
        ///     <c>true</c> if it's a field
        /// </summary>
        public bool IsField
        {
            get;
            set;
        }

        /// <summary>
        ///     <c>true</c> if it's a property
        /// </summary>
        public bool IsProperty
        {
            get { return !IsField; }
            set { IsField = !value; }
        }

        /// <summary>
        ///     Gets/sets the field/property type
        /// </summary>
        public TypeSig Type
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the property/field name
        /// </summary>
        public UTF8String Name
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the argument
        /// </summary>
        public CAArgument Argument
        {
            get { return argument; }
            set { argument = value; }
        }

        /// <summary>
        ///     Gets/sets the argument type
        /// </summary>
        public TypeSig ArgumentType
        {
            get { return argument.Type; }
            set { argument.Type = value; }
        }

        /// <summary>
        ///     Gets/sets the argument value
        /// </summary>
        public object Value
        {
            get { return argument.Value; }
            set { argument.Value = value; }
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        ///     Clones this instance and any <see cref="CAArgument" />s referenced from this instance.
        /// </summary>
        /// <returns></returns>
        public CANamedArgument Clone()
        {
            return new CANamedArgument(IsField, Type, Name, argument.Clone());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var v = Value;
            return string.Format("({0}) {1} {2} = {3} ({4})", IsField ? "field" : "property", Type, Name, v == null ? "null" : v, ArgumentType);
        }
    }
}