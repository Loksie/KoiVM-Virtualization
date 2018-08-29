#region

using System.Linq;
using System.Text.RegularExpressions;
using Confuser.Core;
using Confuser.Renamer.References;
using dnlib.DotNet;

#endregion

namespace Confuser.Renamer.Analyzers
{
    internal class ResourceAnalyzer : IRenamer
    {
        private static readonly Regex ResourceNamePattern = new Regex("^(.*)\\.resources$");

        public void Analyze(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            var module = def as ModuleDef;
            if(module == null) return;

            var asmName = module.Assembly.Name.String;
            if(!string.IsNullOrEmpty(module.Assembly.Culture) &&
               asmName.EndsWith(".resources"))
            {
                // Satellite assembly
                var satellitePattern = new Regex(string.Format("^(.*)\\.{0}\\.resources$", module.Assembly.Culture));
                var nameAsmName = asmName.Substring(0, asmName.Length - ".resources".Length);
                ModuleDef mainModule = context.Modules.SingleOrDefault(mod => mod.Assembly.Name == nameAsmName);
                if(mainModule == null)
                {
                    context.Logger.ErrorFormat("Could not find main assembly of satellite assembly '{0}'.", module.Assembly.FullName);
                    throw new ConfuserException(null);
                }

                var format = "{0}." + module.Assembly.Culture + ".resources";
                foreach(var res in module.Resources)
                {
                    var match = satellitePattern.Match(res.Name);
                    if(!match.Success)
                        continue;
                    var typeName = match.Groups[1].Value;
                    var type = mainModule.FindReflectionThrow(typeName);
                    if(type == null)
                    {
                        context.Logger.WarnFormat("Could not find resource type '{0}'.", typeName);
                        continue;
                    }
                    service.ReduceRenameMode(type, RenameMode.ASCII);
                    service.AddReference(type, new ResourceReference(res, type, format));
                }
            }
            else
            {
                var format = "{0}.resources";
                foreach(var res in module.Resources)
                {
                    var match = ResourceNamePattern.Match(res.Name);
                    if(!match.Success || res.ResourceType != ResourceType.Embedded)
                        continue;
                    var typeName = match.Groups[1].Value;

                    if(typeName.EndsWith(".g")) // WPF resources, ignore
                        continue;

                    var type = module.FindReflection(typeName);
                    if(type == null)
                    {
                        context.Logger.WarnFormat("Could not find resource type '{0}'.", typeName);
                        continue;
                    }
                    service.ReduceRenameMode(type, RenameMode.ASCII);
                    service.AddReference(type, new ResourceReference(res, type, format));
                }
            }
        }

        public void PreRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            //
        }

        public void PostRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            //
        }
    }
}