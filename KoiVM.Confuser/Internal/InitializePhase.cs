#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Confuser.Core;
using Confuser.Core.Services;
using Confuser.Protections;
using Confuser.Renamer;
using dnlib.DotNet;

#endregion

namespace KoiVM.Confuser.Internal
{
    [Obfuscation(Exclude = false, Feature = "+koi;")]
    public class InitializePhase : ProtectionPhase
    {
        private static readonly string Version;
        private static readonly string Copyright;
        private readonly string koiDir;

        static InitializePhase()
        {
            var assembly = typeof(Fish).Assembly;
            var nameAttr = (AssemblyProductAttribute) assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];
            var verAttr =
                (AssemblyInformationalVersionAttribute)
                assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)[0];
            var cpAttr = (AssemblyCopyrightAttribute) assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0];
            Version = string.Format("{0} {1}", nameAttr.Product, verAttr.InformationalVersion);
            Copyright = cpAttr.Copyright;
        }

        public InitializePhase(Protection parent, string koiDir)
            : base(parent)
        {
            this.koiDir = koiDir;
            // ExpirationChecker.Init(koiDir);
        }

        public override ProtectionTargets Targets => ProtectionTargets.Modules;

        public override string Name => "Virtualization initialization";

        protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
        {
            context.Logger.InfoFormat("{0} {1}", Version, Copyright);

            if(Fish.Id == "00000000")
                context.Logger.InfoFormat("For Internal Use Only, Do Not Distribute.");
            else
                context.Logger.InfoFormat("Licensed to {0}, subscription ends at {1}.", Fish.UserName, Fish.SubscriptionEnd);

            var random = context.Registry.GetService<IRandomService>();
            var refProxy = context.Registry.GetService<IReferenceProxyService>();
            var nameSrv = context.Registry.GetService<INameService>();
            var seed = random.GetRandomGenerator(Parent.FullId).NextInt32();

            string rtName = null;
            bool dbg = false, stackwalk = false;
            ModuleDef merge = null;
            foreach(var module in context.Modules)
            {
                if(rtName == null)
                    rtName = parameters.GetParameter<string>(context, module, "rtName");
                if(dbg == false)
                    dbg = parameters.GetParameter<bool>(context, module, "dbgInfo");
                if(stackwalk == false)
                    stackwalk = parameters.GetParameter<bool>(context, module, "stackwalk");
                if(merge == null && parameters.GetParameter(context, module, "merge", false))
                {
                    Console.WriteLine("MerggggggggggEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
                    merge = module;
                }
                else
                {
                    Console.WriteLine("Should we merge the assemblies -> y\n");

                    var k = Console.ReadLine();
                    if(k.ToLower() == "y")
                        merge = module;
                    rtName = "Virtualization";
                }
            }
            rtName = rtName ?? "KoiVM.Runtime--test";

            ModuleDefMD rtModule;
            var resStream = typeof(Virtualizer).Assembly.GetManifestResourceStream("KoiVM.Runtime.dll");
            if(resStream != null)
            {
                rtModule = ModuleDefMD.Load(resStream, context.Resolver.DefaultModuleContext);
            }
            else
            {
                var rtPath = Path.Combine(koiDir, "KoiVM.Runtime.dll");
                rtModule = ModuleDefMD.Load(rtPath, context.Resolver.DefaultModuleContext);
            }
            rtModule.Assembly.Name = rtName;
            rtModule.Name = rtName + ".dll";
            var vr = new Virtualizer(seed, context.Project.Debug);
            vr.ExportDbgInfo = dbg;
            vr.DoStackWalk = stackwalk;
            vr.Initialize(rtModule);

            context.Annotations.Set(context, Fish.VirtualizerKey, vr);
            context.Annotations.Set(context, Fish.MergeKey, merge);

            if(merge != null)
            {
                var types = new List<TypeDef>(vr.RuntimeModule.GetTypes());
                types.Remove(vr.RuntimeModule.GlobalType);
                vr.CommitRuntime(merge);
                foreach(var type in types)
                foreach(var def in type.FindDefinitions())
                {
                    if(def is TypeDef && def != type) // nested type
                        continue;
                    nameSrv.SetCanRename(def, false);
                    ProtectionParameters.SetParameters(context, def, new ProtectionSettings());
                }
            }
            else
            {
                vr.CommitRuntime(merge);
            }

            var ctor = typeof(InternalsVisibleToAttribute).GetConstructor(new[] {typeof(string)});
            foreach(ModuleDef module in context.Modules)
            {
                var methods = new HashSet<MethodDef>();
                foreach(var type in module.GetTypes())
                foreach(var method in type.Methods)
                    if(ProtectionParameters.GetParameters(context, method).ContainsKey(Parent))
                        methods.Add(method);

                if(methods.Count > 0)
                {
                    var ca = new CustomAttribute((ICustomAttributeType) module.Import(ctor));
                    ca.ConstructorArguments.Add(new CAArgument(module.CorLibTypes.String, vr.RuntimeModule.Assembly.Name.String));
                    module.Assembly.CustomAttributes.Add(ca);
                }

                foreach(var entry in new Scanner(module, methods).Scan().WithProgress(context.Logger))
                {
                    if(entry.Item2)
                        context.Annotations.Set(entry.Item1, Fish.ExportKey, Fish.ExportKey);
                    else
                        refProxy.ExcludeTarget(context, entry.Item1);
                    context.CheckCancellation();
                }
            }
        }
    }
}