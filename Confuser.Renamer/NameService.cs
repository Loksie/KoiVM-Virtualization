#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Confuser.Core;
using Confuser.Core.Services;
using Confuser.Renamer.Analyzers;
using dnlib.DotNet;
using dnlib.DotNet.MD;

#endregion

namespace Confuser.Renamer
{
    public interface INameService
    {
        VTableStorage GetVTables();

        void Analyze(IDnlibDef def);

        bool CanRename(object obj);
        void SetCanRename(object obj, bool val);

        void SetParam(IDnlibDef def, string name, string value);
        string GetParam(IDnlibDef def, string name);

        RenameMode GetRenameMode(object obj);
        void SetRenameMode(object obj, RenameMode val);
        void ReduceRenameMode(object obj, RenameMode val);

        string ObfuscateName(string name, RenameMode mode);
        string RandomName();
        string RandomName(RenameMode mode);

        void RegisterRenamer(IRenamer renamer);
        T FindRenamer<T>();
        void AddReference<T>(T obj, INameReference<T> reference);

        void SetOriginalName(object obj, string name);
        void SetOriginalNamespace(object obj, string ns);

        void MarkHelper(IDnlibDef def, IMarkerService marker, ConfuserComponent parentComp);
    }

    internal class NameService : INameService
    {
        private static readonly object CanRenameKey = new object();
        private static readonly object RenameModeKey = new object();
        private static readonly object ReferencesKey = new object();
        private static readonly object OriginalNameKey = new object();
        private static readonly object OriginalNamespaceKey = new object();

        private readonly ConfuserContext context;

        private readonly HashSet<string> identifiers = new HashSet<string>();
        private readonly byte[] nameId = new byte[8];
        private readonly Dictionary<string, string> nameMap1 = new Dictionary<string, string>();
        private readonly Dictionary<string, string> nameMap2 = new Dictionary<string, string>();
        private readonly byte[] nameSeed;
        private readonly RandomGenerator random;
        private readonly VTableStorage storage;
        private AnalyzePhase analyze;
        internal ReversibleRenamer reversibleRenamer;

        public NameService(ConfuserContext context)
        {
            this.context = context;
            storage = new VTableStorage(context.Logger);
            random = context.Registry.GetService<IRandomService>().GetRandomGenerator(NameProtection._FullId);
            nameSeed = random.NextBytes(20);

            Renamers = new List<IRenamer>
            {
                new InterReferenceAnalyzer(),
                new VTableAnalyzer(),
                new TypeBlobAnalyzer(),
                new ResourceAnalyzer(),
                new LdtokenEnumAnalyzer()
            };
        }

        public IList<IRenamer> Renamers
        {
            get;
        }

        public VTableStorage GetVTables()
        {
            return storage;
        }

        public bool CanRename(object obj)
        {
            if(obj is IDnlibDef)
            {
                if(analyze == null)
                    analyze = context.Pipeline.FindPhase<AnalyzePhase>();

                var prot = (NameProtection) analyze.Parent;
                var parameters = ProtectionParameters.GetParameters(context, (IDnlibDef) obj);
                if(parameters == null || !parameters.ContainsKey(prot))
                    return false;
                return context.Annotations.Get(obj, CanRenameKey, true);
            }
            return false;
        }

        public void SetCanRename(object obj, bool val)
        {
            context.Annotations.Set(obj, CanRenameKey, val);
        }

        public void SetParam(IDnlibDef def, string name, string value)
        {
            var param = ProtectionParameters.GetParameters(context, def);
            if(param == null)
                ProtectionParameters.SetParameters(context, def, param = new ProtectionSettings());
            Dictionary<string, string> nameParam;
            if(!param.TryGetValue(analyze.Parent, out nameParam))
                param[analyze.Parent] = nameParam = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            nameParam[name] = value;
        }

        public string GetParam(IDnlibDef def, string name)
        {
            var param = ProtectionParameters.GetParameters(context, def);
            if(param == null)
                return null;
            Dictionary<string, string> nameParam;
            if(!param.TryGetValue(analyze.Parent, out nameParam))
                return null;
            return nameParam.GetValueOrDefault(name);
        }

        public RenameMode GetRenameMode(object obj)
        {
            return context.Annotations.Get(obj, RenameModeKey, RenameMode.Unicode);
        }

        public void SetRenameMode(object obj, RenameMode val)
        {
            context.Annotations.Set(obj, RenameModeKey, val);
        }

        public void ReduceRenameMode(object obj, RenameMode val)
        {
            var original = GetRenameMode(obj);
            if(original < val)
                context.Annotations.Set(obj, RenameModeKey, val);
        }

        public void AddReference<T>(T obj, INameReference<T> reference)
        {
            context.Annotations.GetOrCreate(obj, ReferencesKey, key => new List<INameReference>()).Add(reference);
        }

        public void Analyze(IDnlibDef def)
        {
            if(analyze == null)
                analyze = context.Pipeline.FindPhase<AnalyzePhase>();

            SetOriginalName(def, def.Name);
            if(def is TypeDef)
            {
                GetVTables().GetVTable((TypeDef) def);
                SetOriginalNamespace(def, ((TypeDef) def).Namespace);
            }
            analyze.Analyze(this, context, ProtectionParameters.Empty, def, true);
        }
        private Random r = new Random();
        private string Name
        {
            get
            {
                int x = r.Next(1, 20);
                string s = "";
                for (int y = 0; y <= x; y++)
                {
                    s += "_";
                }

                return s;
            }
        }
        public string ObfuscateName(string name, RenameMode mode)
        {
            string newName = null;
            int? count;
            name = ParseGenericName(name, out count);
            return Name;

            //if(string.IsNullOrEmpty(name))
            //    return string.Empty;

            //if(mode == RenameMode.Empty)
            //    return "";
            //if(mode == RenameMode.Debug)
            //    return "_" + name;
            //if(mode == RenameMode.Reversible)
            //{
            //    if(reversibleRenamer == null)
            //        throw new ArgumentException("Password not provided for reversible renaming.");
            //    newName = reversibleRenamer.Encrypt(name);
            //    return MakeGenericName(newName, count);
            //}

            //if(nameMap1.ContainsKey(name))
            //    return nameMap1[name];

            //var hash = Utils.Xor(Utils.SHA1(Encoding.UTF8.GetBytes(name)), nameSeed);
            //for(var i = 0; i < 100; i++)
            //{
            //    newName = ObfuscateNameInternal(hash, mode);
            //    if(!identifiers.Contains(MakeGenericName(newName, count)))
            //        break;
            //    hash = Utils.SHA1(hash);
            //}

            //if((mode & RenameMode.Decodable) != 0)
            //{
            //    nameMap2[newName] = name;
            //    nameMap1[name] = newName;
            //}

            //return MakeGenericName(newName, count);
        }

        public string RandomName()
        {
            return RandomName(RenameMode.Unicode);
        }

        public string RandomName(RenameMode mode)
        {
            return ObfuscateName(Utils.ToHexString(random.NextBytes(16)), mode);
        }

        public void SetOriginalName(object obj, string name)
        {
            identifiers.Add(name);
            context.Annotations.Set(obj, OriginalNameKey, name);
        }

        public void SetOriginalNamespace(object obj, string ns)
        {
            identifiers.Add(ns);
            context.Annotations.Set(obj, OriginalNamespaceKey, ns);
        }

        public void RegisterRenamer(IRenamer renamer)
        {
            Renamers.Add(renamer);
        }

        public T FindRenamer<T>()
        {
            return Renamers.OfType<T>().Single();
        }

        public void MarkHelper(IDnlibDef def, IMarkerService marker, ConfuserComponent parentComp)
        {
            if(marker.IsMarked(def))
                return;
            if(def is MethodDef)
            {
                var method = (MethodDef) def;
                method.Access = MethodAttributes.Assembly;
                if(!method.IsSpecialName && !method.IsRuntimeSpecialName && !method.DeclaringType.IsDelegate())
                    method.Name = RandomName();
            }
            else if(def is FieldDef)
            {
                var field = (FieldDef) def;
                field.Access = FieldAttributes.Assembly;
                if(!field.IsSpecialName && !field.IsRuntimeSpecialName)
                    field.Name = RandomName();
            }
            else if(def is TypeDef)
            {
                var type = (TypeDef) def;
                type.Visibility = type.DeclaringType == null ? TypeAttributes.NotPublic : TypeAttributes.NestedAssembly;
                type.Namespace = "";
                if(!type.IsSpecialName && !type.IsRuntimeSpecialName)
                    type.Name = RandomName();
            }
            SetCanRename(def, false);
            Analyze(def);
            marker.Mark(def, parentComp);
        }

        public void SetNameId(uint id)
        {
            for(var i = nameId.Length - 1; i >= 0; i--)
            {
                nameId[i] = (byte) (id & 0xff);
                id >>= 8;
            }
        }

        private void IncrementNameId()
        {
            for(var i = nameId.Length - 1; i >= 0; i--)
            {
                nameId[i]++;
                if(nameId[i] != 0)
                    break;
            }
        }

        private string ObfuscateNameInternal(byte[] hash, RenameMode mode)
        {
            switch(mode)
            {
                case RenameMode.Empty:
                    return "";
                case RenameMode.Unicode:
                    //return Utils.EncodeString(hash, unicodeCharset) + "\u202e\u202e\u202e\u202e\u202e\u202e";
                    return Utils.repeatingChar();
                case RenameMode.Letters:
                    return Utils.EncodeString(hash, letterCharset);
                case RenameMode.Repeating:
                    return Name;
                case RenameMode.ASCII:
                    return Utils.EncodeString(hash, asciiCharset);
                case RenameMode.Decodable:
                    IncrementNameId();
                    return "_" + Utils.EncodeString(hash, alphaNumCharset);
                case RenameMode.Sequential:
                    IncrementNameId();
                    return "_" + Utils.EncodeString(nameId, alphaNumCharset);
                default:

                    throw new NotSupportedException("Rename mode '" + mode + "' is not supported.");
            }
        }

        private string ParseGenericName(string name, out int? count)
        {
            if(name.LastIndexOf('`') != -1)
            {
                var index = name.LastIndexOf('`');
                int c;
                if(int.TryParse(name.Substring(index + 1), out c))
                {
                    count = c;
                    return name.Substring(0, index);
                }
            }
            count = null;
            return name;
        }

        private string MakeGenericName(string name, int? count)
        {
            if(count == null)
                return name;
            return string.Format("{0}`{1}", name, count.Value);
        }

        public RandomGenerator GetRandom()
        {
            return random;
        }

        public IList<INameReference> GetReferences(object obj)
        {
            return context.Annotations.GetLazy(obj, ReferencesKey, key => new List<INameReference>());
        }

        public string GetOriginalName(object obj)
        {
            return context.Annotations.Get(obj, OriginalNameKey, "");
        }

        public string GetOriginalNamespace(object obj)
        {
            return context.Annotations.Get(obj, OriginalNamespaceKey, "");
        }

        public ICollection<KeyValuePair<string, string>> GetNameMap()
        {
            return nameMap2;
        }

        #region Charsets

        private static readonly char[] asciiCharset = Enumerable.Range(32, 95)
            .Select(ord => (char) ord)
            .Except(new[] {'.'})
            .ToArray();

        private static readonly char[] letterCharset = Enumerable.Range(0, 26)
            .SelectMany(ord => new[] {(char) ('a' + ord), (char) ('A' + ord)})
            .ToArray();

        private static readonly char[] alphaNumCharset = Enumerable.Range(0, 26)
            .SelectMany(ord => new[] {(char) ('a' + ord), (char) ('A' + ord)})
            .Concat(Enumerable.Range(0, 10).Select(ord => (char) ('0' + ord)))
            .ToArray();

        // Especially chosen, just to mess with people.
        // Inspired by: http://xkcd.com/1137/ :D
        private static readonly char[] unicodeCharset = new char[] { }
            .Concat(Enumerable.Range(0x200b, 5).Select(ord => (char) ord))
            .Concat(Enumerable.Range(0x2029, 6).Select(ord => (char) ord))
            .Concat(Enumerable.Range(0x206a, 6).Select(ord => (char) ord))
            .Except(new[] {'\u2029'})
            .ToArray();

        #endregion
    }
}