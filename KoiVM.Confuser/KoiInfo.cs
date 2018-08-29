#region

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Confuser.Core;

#endregion

namespace KoiVM.Confuser
{
    internal static class KoiInfo
    {
        internal static KoiSettings settings;
        private static bool inited;
        private static readonly List<Assembly> assemblies = new List<Assembly>();

        public static string KoiDirectory
        {
            get;
            private set;
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;
            foreach(var asm in assemblies)
                if(asm.GetName().Name == name)
                    return asm;

            var folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblyPath = Path.Combine(folderPath, name + ".dll");
            if(File.Exists(assemblyPath) == false)
                return null;

            var assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }

        internal static void Init()
        {
            lock(typeof(KoiInfo))
            {
                if(inited)
                    return;

                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                KoiDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                settings = new KoiSettings();

                inited = true;
            }
        }

        internal static void Init(ConfuserContext ctx)
        {
            lock(typeof(KoiInfo))
            {
                if(inited)
                    return;

                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                KoiDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                settings = new KoiSettings();

                // Check if not yet processed.
                if(Assembly.GetExecutingAssembly().ManifestModule.GetType("KoiVM.Confuser.Internal.Fish") != null)
                    return;

                if(ctx != null)
                {
                    var packPath = Path.Combine(KoiDirectory, "koi.pack");

                    if(!File.Exists(packPath))
                        RetrieveKoi(ctx);

                    else if(!settings.NoCheck)
                        CheckUpdate(ctx);

                    InitKoi();
                }
                inited = true;
            }
        }

        private static void CheckUpdate(ConfuserContext ctx)
        {
            var ver = new KoiSystem().GetVersion(settings.KoiID);
            if(ver == settings.Version)
                return;

            ctx.Logger.DebugFormat("New version of KoiVM: {0}", ver);
            ctx.Logger.DebugFormat("Current version of KoiVM: {0}", settings.Version);

            if(settings.NoUI)
            {
                ctx.Logger.DebugFormat("Updating...");
                var sys = new KoiSystem();
                var hnd = new ManualResetEvent(false);
                var okay = false;
                sys.Progress += _ => { };
                sys.Finish += f =>
                {
                    okay = f;
                    hnd.Set();
                };
                sys.Login(settings.KoiID);
                hnd.WaitOne();
                if(!okay)
                    throw new InvalidOperationException("Authentication failed.");
                settings.Version = ver;
                settings.Save();
            }
            else
            {
                Application.EnableVisualStyles();
                if(new UpdatePrompt(ver) {TopLevel = true}.ShowDialog() != DialogResult.OK)
                    throw new InvalidOperationException("Authentication failed.");
            }
        }

        private static void RetrieveKoi(ConfuserContext ctx)
        {
            if(settings.NoUI)
            {
                ctx.Logger.Debug("Retrieving Koi...");

                var sys = new KoiSystem();
                var hnd = new ManualResetEvent(false);
                var okay = false;
                sys.Progress += _ => { };
                sys.Finish += f =>
                {
                    okay = f;
                    hnd.Set();
                };
                sys.Login(settings.KoiID);
                hnd.WaitOne();
                if(!okay)
                    throw new InvalidOperationException("Authentication failed.");
            }
            else
            {
                Application.EnableVisualStyles();
                if(new LoginPrompt {TopLevel = true}.ShowDialog() != DialogResult.OK)
                    throw new InvalidOperationException("Authentication failed.");
            }
        }

        internal static void InitKoi(bool runCctor = true)
        {
            var rc4 = new RC4(Convert.FromBase64String("S29pVk0gaXMgY3V0ZSEhIQ=="));
            var buf = File.ReadAllBytes(Path.Combine(KoiDirectory, "koi.pack"));
            rc4.Crypt(buf, 0, buf.Length);
            using(var deflate = new DeflateStream(new MemoryStream(buf), CompressionMode.Decompress))
            using(var reader = new BinaryReader(deflate))
            {
                var count = reader.ReadInt32();
                assemblies.Clear();
                for(var i = 0; i < count; i++)
                {
                    var asm = Assembly.Load(reader.ReadBytes(reader.ReadInt32()));
                    assemblies.Add(asm);
                }
            }

            if(!runCctor)
                return;
            foreach(var asm in assemblies) // Initialize the modules, since internal calls might not trigger module cctor
                RuntimeHelpers.RunModuleConstructor(asm.ManifestModule.ModuleHandle);
        }
    }
}