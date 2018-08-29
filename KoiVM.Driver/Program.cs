#region

using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

#endregion

namespace KoiVM.Driver
{
    internal class Program
    {
#if DEBUG
        private const bool Debug = true;
#else
		const bool Debug = false;
#endif

        private static void Main(string[] args)
        {
            var resolver = new AssemblyResolver();
            resolver.EnableTypeDefCache = true;
            resolver.DefaultModuleContext = new ModuleContext(resolver);

            var module = ModuleDefMD.Load(args[0], resolver.DefaultModuleContext);
            if(Debug)
                module.LoadPdb();
            var vr = new Virtualizer(100, Debug);
            vr.Initialize(ModuleDefMD.Load(args[1], resolver.DefaultModuleContext));
            vr.AddModule(module);

            vr.ProcessMethods(module);
            var listener = vr.CommitModule(module);
            vr.CommitRuntime();

            var dir = Path.GetDirectoryName(args[0]);
            vr.SaveRuntime(dir);
            module.Write(Path.Combine(dir, "Test.virtualized.exe"), new ModuleWriterOptions(module, listener));
            if(Debug)
                File.WriteAllBytes(Path.Combine(dir, "Test.virtualized.map"), vr.Runtime.DebugInfo);
        }
    }
}