#region

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
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
    ///     Resolves assemblies
    /// </summary>
    public class AssemblyResolver : IAssemblyResolver
    {
        private static readonly ModuleDef nullModule = new ModuleDefUser();

        // DLL files are searched before EXE files
        private static readonly string[] assemblyExtensions = {".dll", ".exe"};

        private static readonly string[] winMDAssemblyExtensions = {".winmd"};

        private static readonly ThreadSafe.List<GacInfo> gacInfos;
        private static readonly ThreadSafe.List<string> extraMonoPaths;

        private static readonly string[] monoVerDirs =
        {
            "4.5", "4.0",
            "3.5", "3.0", "2.0",
            "1.1", "1.0"
        };

        private readonly ThreadSafe.Dictionary<ModuleDef, ThreadSafe.IList<string>> moduleSearchPaths = new ThreadSafe.Dictionary<ModuleDef, ThreadSafe.IList<string>>();
        private readonly ThreadSafe.Dictionary<string, AssemblyDef> cachedAssemblies = new ThreadSafe.Dictionary<string, AssemblyDef>(StringComparer.Ordinal);
#if THREAD_SAFE
		readonly Lock theLock = Lock.Create();
#endif

        private sealed class GacInfo
        {
            public readonly string path;
            public readonly string prefix;
            public readonly ThreadSafe.IList<string> subDirs;
            public readonly int version;

            public GacInfo(int version, string prefix, string path, ThreadSafe.IList<string> subDirs)
            {
                this.version = version;
                this.prefix = prefix;
                this.path = path;
                this.subDirs = subDirs;
            }
        }

        static AssemblyResolver()
        {
            gacInfos = new ThreadSafe.List<GacInfo>();

            if(Type.GetType("Mono.Runtime") != null)
            {
                var dirs = new ThreadSafe.Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                extraMonoPaths = new ThreadSafe.List<string>();
                foreach(var prefix in FindMonoPrefixes())
                {
                    var dir = Path.Combine(Path.Combine(Path.Combine(prefix, "lib"), "mono"), "gac");
                    if(dirs.ContainsKey(dir))
                        continue;
                    dirs[dir] = true;

                    if(Directory.Exists(dir))
                        gacInfos.Add(new GacInfo(-1, "", Path.GetDirectoryName(dir), new[]
                        {
                            Path.GetFileName(dir)
                        }));

                    dir = Path.GetDirectoryName(dir);
                    foreach(var verDir in monoVerDirs)
                    {
                        var dir2 = Path.Combine(dir, verDir);
                        if(Directory.Exists(dir2))
                            extraMonoPaths.Add(dir2);
                    }
                }

                var paths = Environment.GetEnvironmentVariable("MONO_PATH");
                if(paths != null)
                    foreach(var path in paths.Split(Path.PathSeparator))
                        if(path != string.Empty && Directory.Exists(path))
                            extraMonoPaths.Add(path);
            }
            else
            {
                var windir = Environment.GetEnvironmentVariable("WINDIR");
                if(!string.IsNullOrEmpty(windir))
                {
                    string path;

                    // .NET 1.x and 2.x
                    path = Path.Combine(windir, "assembly");
                    if(Directory.Exists(path))
                        gacInfos.Add(new GacInfo(2, "", path, new[]
                        {
                            "GAC_32", "GAC_64", "GAC_MSIL", "GAC"
                        }));

                    // .NET 4.x
                    path = Path.Combine(Path.Combine(windir, "Microsoft.NET"), "assembly");
                    if(Directory.Exists(path))
                        gacInfos.Add(new GacInfo(4, "v4.0_", path, new[]
                        {
                            "GAC_32", "GAC_64", "GAC_MSIL"
                        }));
                }
            }
        }

        private static string GetCurrentMonoPrefix()
        {
            var path = typeof(object).Module.FullyQualifiedName;
            for(var i = 0; i < 4; i++)
                path = Path.GetDirectoryName(path);
            return path;
        }

        private static ThreadSafe.IEnumerable<string> FindMonoPrefixes()
        {
            yield return GetCurrentMonoPrefix();

            var prefixes = Environment.GetEnvironmentVariable("MONO_GAC_PREFIX");
            if(!string.IsNullOrEmpty(prefixes))
                foreach(var prefix in prefixes.Split(Path.PathSeparator))
                    if(prefix != string.Empty)
                        yield return prefix;
        }

        /// <summary>
        ///     Gets/sets the default <see cref="ModuleContext" />
        /// </summary>
        public ModuleContext DefaultModuleContext
        {
            get;
            set;
        }

        /// <summary>
        ///     <c>true</c> if <see cref="Resolve" /> should find an assembly that matches exactly.
        ///     <c>false</c> if it first tries to match exactly, and if that fails, it picks an
        ///     assembly that is closest to the requested assembly.
        /// </summary>
        public bool FindExactMatch
        {
            get;
            set;
        }

        /// <summary>
        ///     <c>true</c> if resolved .NET framework assemblies can be redirected to the source
        ///     module's framework assembly version. Eg. if a resolved .NET 3.5 assembly can be
        ///     redirected to a .NET 4.0 assembly if the source module is a .NET 4.0 assembly. This is
        ///     ignored if <see cref="FindExactMatch" /> is <c>true</c>.
        /// </summary>
        public bool EnableFrameworkRedirect
        {
            get;
            set;
        }

        /// <summary>
        ///     If <c>true</c>, all modules in newly resolved assemblies will have their
        ///     <see cref="ModuleDef.EnableTypeDefFindCache" /> property set to <c>true</c>.
        /// </summary>
        public bool EnableTypeDefCache
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets paths searched before trying the standard locations
        /// </summary>
        public ThreadSafe.IList<string> PreSearchPaths
        {
            get;
        } = ThreadSafeListCreator.Create<string>();

        /// <summary>
        ///     Gets paths searched after trying the standard locations
        /// </summary>
        public ThreadSafe.IList<string> PostSearchPaths
        {
            get;
        } = ThreadSafeListCreator.Create<string>();

        /// <summary>
        ///     Default constructor
        /// </summary>
        public AssemblyResolver()
            : this(null, true)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="defaultModuleContext">Module context for all resolved assemblies</param>
        public AssemblyResolver(ModuleContext defaultModuleContext)
            : this(defaultModuleContext, true)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="defaultModuleContext">Module context for all resolved assemblies</param>
        /// <param name="addOtherSearchPaths">
        ///     If <c>true</c>, add other common assembly search
        ///     paths, not just the module search paths and the GAC.
        /// </param>
        public AssemblyResolver(ModuleContext defaultModuleContext, bool addOtherSearchPaths)
        {
            DefaultModuleContext = defaultModuleContext;
            EnableFrameworkRedirect = true;
            if(addOtherSearchPaths)
                AddOtherSearchPaths(PostSearchPaths);
        }

        /// <inheritdoc />
        public AssemblyDef Resolve(IAssembly assembly, ModuleDef sourceModule)
        {
            if(assembly == null)
                return null;

            if(EnableFrameworkRedirect && !FindExactMatch)
                FrameworkRedirect.ApplyFrameworkRedirect(ref assembly, sourceModule);

#if THREAD_SAFE
			theLock.EnterWriteLock(); try {
#endif
            var resolvedAssembly = Resolve2(assembly, sourceModule);
            if(resolvedAssembly == null)
            {
                var asmName = UTF8String.ToSystemStringOrEmpty(assembly.Name);
                var asmNameTrimmed = asmName.Trim();
                if(asmName != asmNameTrimmed)
                {
                    assembly = new AssemblyNameInfo
                    {
                        Name = asmNameTrimmed,
                        Version = assembly.Version,
                        PublicKeyOrToken = assembly.PublicKeyOrToken,
                        Culture = assembly.Culture
                    };
                    resolvedAssembly = Resolve2(assembly, sourceModule);
                }
            }

            if(resolvedAssembly == null)
            {
                // Make sure we don't search for this assembly again. This speeds up callers who
                // keep asking for this assembly when trying to resolve many different TypeRefs
                cachedAssemblies[GetAssemblyNameKey(assembly)] = null;
                return null;
            }

            var key1 = GetAssemblyNameKey(resolvedAssembly);
            var key2 = GetAssemblyNameKey(assembly);
            AssemblyDef asm1, asm2;
            cachedAssemblies.TryGetValue(key1, out asm1);
            cachedAssemblies.TryGetValue(key2, out asm2);

            if(asm1 != resolvedAssembly && asm2 != resolvedAssembly)
                if(EnableTypeDefCache)
                    foreach(var module in resolvedAssembly.Modules.GetSafeEnumerable())
                        if(module != null)
                            module.EnableTypeDefFindCache = true;

            var inserted = false;
            if(!cachedAssemblies.ContainsKey(key1))
            {
                cachedAssemblies.Add(key1, resolvedAssembly);
                inserted = true;
            }
            if(!cachedAssemblies.ContainsKey(key2))
            {
                cachedAssemblies.Add(key2, resolvedAssembly);
                inserted = true;
            }
            if(inserted || asm1 == resolvedAssembly || asm2 == resolvedAssembly)
                return resolvedAssembly;

            // Dupe assembly. Don't insert it.
            var dupeModule = resolvedAssembly.ManifestModule;
            if(dupeModule != null)
                dupeModule.Dispose();
            return asm1 ?? asm2;
#if THREAD_SAFE
			} finally { theLock.ExitWriteLock(); }
#endif
        }

        /// <inheritdoc />
        public bool AddToCache(AssemblyDef asm)
        {
            if(asm == null)
                return false;
            var asmKey = GetAssemblyNameKey(asm);
            AssemblyDef cachedAsm;
#if THREAD_SAFE
			theLock.EnterWriteLock(); try {
#endif
            if(cachedAssemblies.TryGetValue(asmKey, out cachedAsm) && cachedAsm != null)
                return asm == cachedAsm;
            cachedAssemblies[asmKey] = asm;
            return true;
#if THREAD_SAFE
			} finally { theLock.ExitWriteLock(); }
#endif
        }

        /// <inheritdoc />
        public bool Remove(AssemblyDef asm)
        {
            if(asm == null)
                return false;
            var asmKey = GetAssemblyNameKey(asm);
#if THREAD_SAFE
			theLock.EnterWriteLock(); try {
#endif
            return cachedAssemblies.Remove(asmKey);
#if THREAD_SAFE
			} finally { theLock.ExitWriteLock(); }
#endif
        }

        /// <inheritdoc />
        public void Clear()
        {
            ThreadSafe.List<AssemblyDef> asms;
#if THREAD_SAFE
			theLock.EnterWriteLock(); try {
#endif
            asms = new ThreadSafe.List<AssemblyDef>(cachedAssemblies.Values);
            cachedAssemblies.Clear();
#if THREAD_SAFE
			} finally { theLock.ExitWriteLock(); }
#endif
            foreach(var asm in asms)
            {
                if(asm == null)
                    continue;
                foreach(var mod in asm.Modules.GetSafeEnumerable())
                    mod.Dispose();
            }
        }

        /// <summary>
        ///     Gets the cached assemblies in this resolver.
        /// </summary>
        /// <returns>The cached assemblies.</returns>
        public ThreadSafe.IEnumerable<AssemblyDef> GetCachedAssemblies()
        {
            return cachedAssemblies.Values;
        }

        private static string GetAssemblyNameKey(IAssembly asmName)
        {
            // Make sure the name contains PublicKeyToken= and not PublicKey=
            return asmName.FullNameToken.ToUpperInvariant();
        }

        private AssemblyDef Resolve2(IAssembly assembly, ModuleDef sourceModule)
        {
            AssemblyDef resolvedAssembly;

            if(cachedAssemblies.TryGetValue(GetAssemblyNameKey(assembly), out resolvedAssembly))
                return resolvedAssembly;

            var moduleContext = DefaultModuleContext;
            if(moduleContext == null && sourceModule != null)
                moduleContext = sourceModule.Context;

            resolvedAssembly = FindExactAssembly(assembly, PreFindAssemblies(assembly, sourceModule, true), moduleContext) ??
                               FindExactAssembly(assembly, FindAssemblies(assembly, sourceModule, true), moduleContext) ??
                               FindExactAssembly(assembly, PostFindAssemblies(assembly, sourceModule, true), moduleContext);
            if(resolvedAssembly != null)
                return resolvedAssembly;

            if(!FindExactMatch)
            {
                resolvedAssembly = FindClosestAssembly(assembly);
                resolvedAssembly = FindClosestAssembly(assembly, resolvedAssembly, PreFindAssemblies(assembly, sourceModule, false), moduleContext);
                resolvedAssembly = FindClosestAssembly(assembly, resolvedAssembly, FindAssemblies(assembly, sourceModule, false), moduleContext);
                resolvedAssembly = FindClosestAssembly(assembly, resolvedAssembly, PostFindAssemblies(assembly, sourceModule, false), moduleContext);
            }

            return resolvedAssembly;
        }

        /// <summary>
        ///     Finds an assembly that exactly matches the requested assembly
        /// </summary>
        /// <param name="assembly">Assembly to find</param>
        /// <param name="paths">Search paths or <c>null</c> if none</param>
        /// <param name="moduleContext">Module context</param>
        /// <returns>
        ///     An <see cref="AssemblyDef" /> instance or <c>null</c> if an exact match
        ///     couldn't be found.
        /// </returns>
        private AssemblyDef FindExactAssembly(IAssembly assembly, ThreadSafe.IEnumerable<string> paths, ModuleContext moduleContext)
        {
            if(paths == null)
                return null;
            var asmComparer = AssemblyNameComparer.CompareAll;
            foreach(var path in paths.GetSafeEnumerable())
            {
                ModuleDefMD mod = null;
                try
                {
                    mod = ModuleDefMD.Load(path, moduleContext);
                    var asm = mod.Assembly;
                    if(asm != null && asmComparer.Equals(assembly, asm))
                    {
                        mod = null;
                        return asm;
                    }
                }
                catch
                {
                }
                finally
                {
                    if(mod != null)
                        mod.Dispose();
                }
            }
            return null;
        }

        /// <summary>
        ///     Finds the closest assembly from the already cached assemblies
        /// </summary>
        /// <param name="assembly">Assembly to find</param>
        /// <returns>The closest <see cref="AssemblyDef" /> or <c>null</c> if none found</returns>
        private AssemblyDef FindClosestAssembly(IAssembly assembly)
        {
            AssemblyDef closest = null;
            var asmComparer = AssemblyNameComparer.CompareAll;
            foreach(var asm in cachedAssemblies.Values)
            {
                if(asm == null)
                    continue;
                if(asmComparer.CompareClosest(assembly, closest, asm) == 1)
                    closest = asm;
            }
            return closest;
        }

        private AssemblyDef FindClosestAssembly(IAssembly assembly, AssemblyDef closest, ThreadSafe.IEnumerable<string> paths, ModuleContext moduleContext)
        {
            if(paths == null)
                return closest;
            var asmComparer = AssemblyNameComparer.CompareAll;
            foreach(var path in paths.GetSafeEnumerable())
            {
                ModuleDefMD mod = null;
                try
                {
                    mod = ModuleDefMD.Load(path, moduleContext);
                    var asm = mod.Assembly;
                    if(asm != null && asmComparer.CompareClosest(assembly, closest, asm) == 1)
                    {
                        if(!IsCached(closest) && closest != null)
                        {
                            var closeMod = closest.ManifestModule;
                            if(closeMod != null)
                                closeMod.Dispose();
                        }
                        closest = asm;
                        mod = null;
                    }
                }
                catch
                {
                }
                finally
                {
                    if(mod != null)
                        mod.Dispose();
                }
            }

            return closest;
        }

        /// <summary>
        ///     Returns <c>true</c> if <paramref name="asm" /> is inserted in <see cref="cachedAssemblies" />
        /// </summary>
        /// <param name="asm">Assembly to check</param>
        private bool IsCached(AssemblyDef asm)
        {
            if(asm == null)
                return false;
            AssemblyDef cachedAsm;
            return cachedAssemblies.TryGetValue(GetAssemblyNameKey(asm), out cachedAsm) &&
                   cachedAsm == asm;
        }

        private ThreadSafe.IEnumerable<string> FindAssemblies2(IAssembly assembly, ThreadSafe.IEnumerable<string> paths)
        {
            if(paths != null)
            {
                var asmSimpleName = UTF8String.ToSystemStringOrEmpty(assembly.Name);
                var exts = assembly.IsContentTypeWindowsRuntime ? winMDAssemblyExtensions : assemblyExtensions;
                foreach(var ext in exts)
                foreach(var path in paths.GetSafeEnumerable())
                {
                    var fullPath = Path.Combine(path, asmSimpleName + ext);
                    if(File.Exists(fullPath))
                        yield return fullPath;
                }
            }
        }

        /// <summary>
        ///     Called before <see cref="FindAssemblies" />
        /// </summary>
        /// <param name="assembly">Assembly to find</param>
        /// <param name="sourceModule">The module that needs to resolve an assembly or <c>null</c></param>
        /// <param name="matchExactly">We're trying to find an exact match</param>
        /// <returns><c>null</c> or an enumerable of full paths to try</returns>
        protected virtual ThreadSafe.IEnumerable<string> PreFindAssemblies(IAssembly assembly, ModuleDef sourceModule, bool matchExactly)
        {
            foreach(var path in FindAssemblies2(assembly, PreSearchPaths))
                yield return path;
        }

        /// <summary>
        ///     Called after <see cref="FindAssemblies" /> (if it fails)
        /// </summary>
        /// <param name="assembly">Assembly to find</param>
        /// <param name="sourceModule">The module that needs to resolve an assembly or <c>null</c></param>
        /// <param name="matchExactly">We're trying to find an exact match</param>
        /// <returns><c>null</c> or an enumerable of full paths to try</returns>
        protected virtual ThreadSafe.IEnumerable<string> PostFindAssemblies(IAssembly assembly, ModuleDef sourceModule, bool matchExactly)
        {
            foreach(var path in FindAssemblies2(assembly, PostSearchPaths))
                yield return path;
        }

        /// <summary>
        ///     Called after <see cref="PreFindAssemblies" /> (if it fails)
        /// </summary>
        /// <param name="assembly">Assembly to find</param>
        /// <param name="sourceModule">The module that needs to resolve an assembly or <c>null</c></param>
        /// <param name="matchExactly">We're trying to find an exact match</param>
        /// <returns><c>null</c> or an enumerable of full paths to try</returns>
        protected virtual ThreadSafe.IEnumerable<string> FindAssemblies(IAssembly assembly, ModuleDef sourceModule, bool matchExactly)
        {
            if(assembly.IsContentTypeWindowsRuntime)
            {
                var path = Path.Combine(Path.Combine(Environment.SystemDirectory, "WinMetadata"), assembly.Name + ".winmd");
                if(File.Exists(path))
                    yield return path;
            }
            else
            {
                foreach(var path in FindAssembliesGac(assembly, sourceModule, matchExactly))
                    yield return path;
            }
            foreach(var path in FindAssembliesModuleSearchPaths(assembly, sourceModule, matchExactly))
                yield return path;
        }

        private ThreadSafe.IEnumerable<string> FindAssembliesGac(IAssembly assembly, ModuleDef sourceModule, bool matchExactly)
        {
            if(matchExactly)
                return FindAssembliesGacExactly(assembly, sourceModule);
            return FindAssembliesGacAny(assembly, sourceModule);
        }

        private ThreadSafe.IEnumerable<GacInfo> GetGacInfos(ModuleDef sourceModule)
        {
            var version = sourceModule == null ? int.MinValue : sourceModule.IsClr40 ? 4 : 2;
            // Try the correct GAC first (eg. GAC4 if it's a .NET 4 assembly)
            foreach(var gacInfo in gacInfos)
                if(gacInfo.version == version)
                    yield return gacInfo;
            foreach(var gacInfo in gacInfos)
                if(gacInfo.version != version)
                    yield return gacInfo;
        }

        private ThreadSafe.IEnumerable<string> FindAssembliesGacExactly(IAssembly assembly, ModuleDef sourceModule)
        {
            foreach(var gacInfo in GetGacInfos(sourceModule))
            foreach(var path in FindAssembliesGacExactly(gacInfo, assembly, sourceModule))
                yield return path;
            if(extraMonoPaths != null)
                foreach(var path in GetExtraMonoPaths(assembly, sourceModule))
                    yield return path;
        }

        private static ThreadSafe.IEnumerable<string> GetExtraMonoPaths(IAssembly assembly, ModuleDef sourceModule)
        {
            if(extraMonoPaths != null)
                foreach(var dir in extraMonoPaths)
                {
                    var file = Path.Combine(dir, assembly.Name + ".dll");
                    if(File.Exists(file))
                        yield return file;
                }
        }

        private ThreadSafe.IEnumerable<string> FindAssembliesGacExactly(GacInfo gacInfo, IAssembly assembly, ModuleDef sourceModule)
        {
            var pkt = PublicKeyBase.ToPublicKeyToken(assembly.PublicKeyOrToken);
            if(gacInfo != null && pkt != null)
            {
                var pktString = pkt.ToString();
                var verString = Utils.CreateVersionWithNoUndefinedValues(assembly.Version).ToString();
                var asmSimpleName = UTF8String.ToSystemStringOrEmpty(assembly.Name);
                foreach(var subDir in gacInfo.subDirs)
                {
                    var baseDir = Path.Combine(gacInfo.path, subDir);
                    baseDir = Path.Combine(baseDir, asmSimpleName);
                    baseDir = Path.Combine(baseDir, string.Format("{0}{1}__{2}", gacInfo.prefix, verString, pktString));
                    var pathName = Path.Combine(baseDir, asmSimpleName + ".dll");
                    if(File.Exists(pathName))
                        yield return pathName;
                }
            }
        }

        private ThreadSafe.IEnumerable<string> FindAssembliesGacAny(IAssembly assembly, ModuleDef sourceModule)
        {
            foreach(var gacInfo in GetGacInfos(sourceModule))
            foreach(var path in FindAssembliesGacAny(gacInfo, assembly, sourceModule))
                yield return path;
            if(extraMonoPaths != null)
                foreach(var path in GetExtraMonoPaths(assembly, sourceModule))
                    yield return path;
        }

        private ThreadSafe.IEnumerable<string> FindAssembliesGacAny(GacInfo gacInfo, IAssembly assembly, ModuleDef sourceModule)
        {
            if(gacInfo != null)
            {
                var asmSimpleName = UTF8String.ToSystemStringOrEmpty(assembly.Name);
                foreach(var subDir in gacInfo.subDirs)
                {
                    var baseDir = Path.Combine(gacInfo.path, subDir);
                    baseDir = Path.Combine(baseDir, asmSimpleName);
                    foreach(var dir in GetDirs(baseDir))
                    {
                        var pathName = Path.Combine(dir, asmSimpleName + ".dll");
                        if(File.Exists(pathName))
                            yield return pathName;
                    }
                }
            }
        }

        private ThreadSafe.IEnumerable<string> GetDirs(string baseDir)
        {
            var dirs = new ThreadSafe.List<string>();
            try
            {
                foreach(var di in new DirectoryInfo(baseDir).GetDirectories())
                    dirs.Add(di.FullName);
            }
            catch
            {
            }
            return dirs;
        }

        private ThreadSafe.IEnumerable<string> FindAssembliesModuleSearchPaths(IAssembly assembly, ModuleDef sourceModule, bool matchExactly)
        {
            var asmSimpleName = UTF8String.ToSystemStringOrEmpty(assembly.Name);
            var searchPaths = GetSearchPaths(sourceModule);
            var exts = assembly.IsContentTypeWindowsRuntime ? winMDAssemblyExtensions : assemblyExtensions;
            foreach(var ext in exts)
            foreach(var path in searchPaths.GetSafeEnumerable())
                for(var i = 0; i < 2; i++)
                {
                    string path2;
                    if(i == 0)
                        path2 = Path.Combine(path, asmSimpleName + ext);
                    else
                        path2 = Path.Combine(Path.Combine(path, asmSimpleName), asmSimpleName + ext);
                    if(File.Exists(path2))
                        yield return path2;
                }
        }

        /// <summary>
        ///     Gets all search paths to use for this module
        /// </summary>
        /// <param name="module">The module or <c>null</c> if unknown</param>
        /// <returns>A list of all search paths to use for this module</returns>
        private ThreadSafe.IEnumerable<string> GetSearchPaths(ModuleDef module)
        {
            var keyModule = module;
            if(keyModule == null)
                keyModule = nullModule;
            ThreadSafe.IList<string> searchPaths;
            if(moduleSearchPaths.TryGetValue(keyModule, out searchPaths))
                return searchPaths;
            moduleSearchPaths[keyModule] = searchPaths = new ThreadSafe.List<string>(GetModuleSearchPaths(module));
            return searchPaths;
        }

        /// <summary>
        ///     Gets all module search paths. This is usually empty unless its assembly has
        ///     a <c>.config</c> file specifying any additional private search paths in a
        ///     &lt;probing/&gt; element.
        /// </summary>
        /// <param name="module">The module or <c>null</c> if unknown</param>
        /// <returns>A list of search paths</returns>
        protected virtual ThreadSafe.IEnumerable<string> GetModuleSearchPaths(ModuleDef module)
        {
            return GetModulePrivateSearchPaths(module);
        }

        /// <summary>
        ///     Gets all private assembly search paths as found in the module's <c>.config</c> file.
        /// </summary>
        /// <param name="module">The module or <c>null</c> if unknown</param>
        /// <returns>A list of search paths</returns>
        protected ThreadSafe.IEnumerable<string> GetModulePrivateSearchPaths(ModuleDef module)
        {
            if(module == null)
                return new string[0];
            var asm = module.Assembly;
            if(asm == null)
                return new string[0];
            module = asm.ManifestModule;
            if(module == null)
                return new string[0]; // Should never happen

            string baseDir = null;
            try
            {
                var imageName = module.Location;
                if(imageName != string.Empty)
                {
                    baseDir = Directory.GetParent(imageName).FullName;
                    var configName = imageName + ".config";
                    if(File.Exists(configName))
                        return GetPrivatePaths(baseDir, configName);
                }
            }
            catch
            {
            }
            if(baseDir != null)
                return new ThreadSafe.List<string> {baseDir};
            return new string[0];
        }

        private ThreadSafe.IEnumerable<string> GetPrivatePaths(string baseDir, string configFileName)
        {
            var searchPaths = new ThreadSafe.List<string>();

            try
            {
                var dirName = Path.GetDirectoryName(Path.GetFullPath(configFileName));
                searchPaths.Add(dirName);

                using(var xmlStream = new FileStream(configFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var doc = new XmlDocument();
                    doc.Load(XmlReader.Create(xmlStream));
                    foreach(var tmp in doc.GetElementsByTagName("probing"))
                    {
                        var probingElem = tmp as XmlElement;
                        if(probingElem == null)
                            continue;
                        var privatePath = probingElem.GetAttribute("privatePath");
                        if(string.IsNullOrEmpty(privatePath))
                            continue;
                        foreach(var tmp2 in privatePath.Split(';'))
                        {
                            var path = tmp2.Trim();
                            if(path == "")
                                continue;
                            var newPath = Path.GetFullPath(Path.Combine(dirName, path.Replace('\\', Path.DirectorySeparatorChar)));
                            if(Directory.Exists(newPath) && newPath.StartsWith(baseDir + Path.DirectorySeparatorChar))
                                searchPaths.Add(newPath);
                        }
                    }
                }
            }
            catch(ArgumentException)
            {
            }
            catch(IOException)
            {
            }
            catch(XmlException)
            {
            }

            return searchPaths;
        }

        /// <summary>
        ///     Add other common search paths
        /// </summary>
        /// <param name="paths">A list that gets updated with the new paths</param>
        protected static void AddOtherSearchPaths(ThreadSafe.IList<string> paths)
        {
            var dirPF = Environment.GetEnvironmentVariable("ProgramFiles");
            AddOtherAssemblySearchPaths(paths, dirPF);
            var dirPFx86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            if(!StringComparer.OrdinalIgnoreCase.Equals(dirPF, dirPFx86))
                AddOtherAssemblySearchPaths(paths, dirPFx86);

            var windir = Environment.GetEnvironmentVariable("WINDIR");
            if(!string.IsNullOrEmpty(windir))
            {
                AddIfExists(paths, windir, @"Microsoft.NET\Framework\v1.1.4322");
                AddIfExists(paths, windir, @"Microsoft.NET\Framework\v1.0.3705");
            }
        }

        private static void AddOtherAssemblySearchPaths(ThreadSafe.IList<string> paths, string path)
        {
            if(string.IsNullOrEmpty(path))
                return;
            AddSilverlightDirs(paths, Path.Combine(path, @"Microsoft Silverlight"));
            AddIfExists(paths, path, @"Microsoft SDKs\Silverlight\v2.0\Libraries\Client");
            AddIfExists(paths, path, @"Microsoft SDKs\Silverlight\v2.0\Libraries\Server");
            AddIfExists(paths, path, @"Microsoft SDKs\Silverlight\v2.0\Reference Assemblies");
            AddIfExists(paths, path, @"Microsoft SDKs\Silverlight\v3.0\Libraries\Client");
            AddIfExists(paths, path, @"Microsoft SDKs\Silverlight\v3.0\Libraries\Server");
            AddIfExists(paths, path, @"Microsoft SDKs\Silverlight\v4.0\Libraries\Client");
            AddIfExists(paths, path, @"Microsoft SDKs\Silverlight\v4.0\Libraries\Server");
            AddIfExists(paths, path, @"Microsoft SDKs\Silverlight\v5.0\Libraries\Client");
            AddIfExists(paths, path, @"Microsoft SDKs\Silverlight\v5.0\Libraries\Server");
            AddIfExists(paths, path, @"Microsoft.NET\SDK\CompactFramework\v2.0\WindowsCE");
            AddIfExists(paths, path, @"Microsoft.NET\SDK\CompactFramework\v3.5\WindowsCE");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\Profile\Client");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Client");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETCore\v4.5.1");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETCore\v4.5");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETMicroFramework\v3.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETMicroFramework\v4.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETMicroFramework\v4.1");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETMicroFramework\v4.2");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETMicroFramework\v4.3");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETPortable\v4.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\.NETPortable\v4.6");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\v3.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\v3.5");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\Silverlight\v3.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\Silverlight\v4.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\Silverlight\v5.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\WindowsPhone\v8.1");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\Framework\WindowsPhoneApp\v8.1");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\.NETCore\3.259.4.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\.NETCore\3.259.3.1");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\.NETCore\3.78.4.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\.NETCore\3.78.3.1");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\.NETCore\3.7.4.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\.NETCore\3.3.1.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\.NETFramework\v2.0\2.3.0.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\.NETFramework\v4.0\4.3.0.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\.NETFramework\v4.0\4.3.1.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\.NETFramework\v4.0\4.4.0.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\.NETPortable\2.3.5.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\.NETPortable\2.3.5.1");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\.NETPortable\3.47.4.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\2.0\Runtime\v2.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\2.0\Runtime\v4.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\3.0\Runtime\.NETPortable");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\3.0\Runtime\v2.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\FSharp\3.0\Runtime\v4.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\WindowsPowerShell\v1.0");
            AddIfExists(paths, path, @"Reference Assemblies\Microsoft\WindowsPowerShell\3.0");
            AddIfExists(paths, path, @"Microsoft Visual Studio .NET\Common7\IDE\PublicAssemblies");
            AddIfExists(paths, path, @"Microsoft Visual Studio .NET\Common7\IDE\PrivateAssemblies");
            AddIfExists(paths, path, @"Microsoft Visual Studio .NET 2003\Common7\IDE\PublicAssemblies");
            AddIfExists(paths, path, @"Microsoft Visual Studio .NET 2003\Common7\IDE\PrivateAssemblies");
            AddIfExists(paths, path, @"Microsoft Visual Studio 8\Common7\IDE\PublicAssemblies");
            AddIfExists(paths, path, @"Microsoft Visual Studio 8\Common7\IDE\PrivateAssemblies");
            AddIfExists(paths, path, @"Microsoft Visual Studio 9.0\Common7\IDE\PublicAssemblies");
            AddIfExists(paths, path, @"Microsoft Visual Studio 9.0\Common7\IDE\PrivateAssemblies");
            AddIfExists(paths, path, @"Microsoft Visual Studio 10.0\Common7\IDE\PublicAssemblies");
            AddIfExists(paths, path, @"Microsoft Visual Studio 10.0\Common7\IDE\PrivateAssemblies");
            AddIfExists(paths, path, @"Microsoft Visual Studio 11.0\Common7\IDE\PublicAssemblies");
            AddIfExists(paths, path, @"Microsoft Visual Studio 11.0\Common7\IDE\PrivateAssemblies");
            AddIfExists(paths, path, @"Microsoft Visual Studio 12.0\Common7\IDE\PublicAssemblies");
            AddIfExists(paths, path, @"Microsoft Visual Studio 12.0\Common7\IDE\PrivateAssemblies");
            AddIfExists(paths, path, @"Microsoft Visual Studio 14.0\Common7\IDE\PublicAssemblies");
            AddIfExists(paths, path, @"Microsoft Visual Studio 14.0\Common7\IDE\PrivateAssemblies");
            AddIfExists(paths, path, @"Microsoft XNA\XNA Game Studio\v2.0\References\Windows\x86");
            AddIfExists(paths, path, @"Microsoft XNA\XNA Game Studio\v2.0\References\Xbox360");
            AddIfExists(paths, path, @"Microsoft XNA\XNA Game Studio\v3.0\References\Windows\x86");
            AddIfExists(paths, path, @"Microsoft XNA\XNA Game Studio\v3.0\References\Xbox360");
            AddIfExists(paths, path, @"Microsoft XNA\XNA Game Studio\v3.0\References\Zune");
            AddIfExists(paths, path, @"Microsoft XNA\XNA Game Studio\v3.1\References\Windows\x86");
            AddIfExists(paths, path, @"Microsoft XNA\XNA Game Studio\v3.1\References\Xbox360");
            AddIfExists(paths, path, @"Microsoft XNA\XNA Game Studio\v3.1\References\Zune");
            AddIfExists(paths, path, @"Microsoft XNA\XNA Game Studio\v4.0\References\Windows\x86");
            AddIfExists(paths, path, @"Microsoft XNA\XNA Game Studio\v4.0\References\Xbox360");
            AddIfExists(paths, path, @"Windows CE Tools\wce500\Windows Mobile 5.0 Pocket PC SDK\Designtimereferences");
            AddIfExists(paths, path, @"Windows CE Tools\wce500\Windows Mobile 5.0 Smartphone SDK\Designtimereferences");
            AddIfExists(paths, path, @"Windows Mobile 5.0 SDK R2\Managed Libraries");
            AddIfExists(paths, path, @"Windows Mobile 6 SDK\Managed Libraries");
            AddIfExists(paths, path, @"Windows Mobile 6.5.3 DTK\Managed Libraries");
            AddIfExists(paths, path, @"Microsoft SQL Server\90\SDK\Assemblies");
            AddIfExists(paths, path, @"Microsoft SQL Server\100\SDK\Assemblies");
            AddIfExists(paths, path, @"Microsoft SQL Server\110\SDK\Assemblies");
            AddIfExists(paths, path, @"Microsoft SQL Server\120\SDK\Assemblies");
            AddIfExists(paths, path, @"Microsoft ASP.NET\ASP.NET MVC 2\Assemblies");
            AddIfExists(paths, path, @"Microsoft ASP.NET\ASP.NET MVC 3\Assemblies");
            AddIfExists(paths, path, @"Microsoft ASP.NET\ASP.NET MVC 4\Assemblies");
            AddIfExists(paths, path, @"Microsoft ASP.NET\ASP.NET Web Pages\v1.0\Assemblies");
            AddIfExists(paths, path, @"Microsoft ASP.NET\ASP.NET Web Pages\v2.0\Assemblies");
            AddIfExists(paths, path, @"Microsoft SDKs\F#\3.0\Framework\v4.0");
        }

        private static void AddSilverlightDirs(ThreadSafe.IList<string> paths, string basePath)
        {
            try
            {
                var di = new DirectoryInfo(basePath);
                foreach(var dir in di.GetDirectories())
                    if(Regex.IsMatch(dir.Name, @"^\d+(?:\.\d+){3}$"))
                        AddIfExists(paths, basePath, dir.Name);
            }
            catch
            {
            }
        }

        private static void AddIfExists(ThreadSafe.IList<string> paths, string basePath, string extraPath)
        {
            var path = Path.Combine(basePath, extraPath);
            if(Directory.Exists(path))
                paths.Add(path);
        }
    }
}