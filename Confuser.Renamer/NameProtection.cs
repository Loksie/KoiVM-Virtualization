#region

using System.IO;
using Confuser.Core;

#endregion

namespace Confuser.Renamer
{
    internal class NameProtection : Protection
    {
        public const string _Id = "rename";
        public const string _FullId = "Ki.Rename";
        public const string _ServiceId = "Ki.Rename";

        public override string Name => "Name Protection";

        public override string Description => "This protection obfuscate the symbols' name so the decompiled source code can neither be compiled nor read.";

        public override string Id => _Id;

        public override string FullId => _FullId;

        public override ProtectionPreset Preset => ProtectionPreset.Minimum;

        protected override void Initialize(ConfuserContext context)
        {
            context.Registry.RegisterService(_ServiceId, typeof(INameService), new NameService(context));
        }

        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPostStage(PipelineStage.Inspection, new AnalyzePhase(this));
            pipeline.InsertPostStage(PipelineStage.BeginModule, new RenamePhase(this));
            pipeline.InsertPreStage(PipelineStage.EndModule, new PostRenamePhase(this));
            pipeline.InsertPostStage(PipelineStage.SaveModules, new ExportMapPhase(this));
        }

        private class ExportMapPhase : ProtectionPhase
        {
            public ExportMapPhase(NameProtection parent)
                : base(parent)
            {
            }

            public override ProtectionTargets Targets => ProtectionTargets.Modules;

            public override string Name => "Export symbol map";

            public override bool ProcessAll => true;

            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                var srv = (NameService) context.Registry.GetService<INameService>();
                var map = srv.GetNameMap();
                if(map.Count == 0)
                    return;

                var path = Path.GetFullPath(Path.Combine(context.OutputDirectory, "symbols.map"));
                var dir = Path.GetDirectoryName(path);
                if(!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using(var writer = new StreamWriter(File.OpenWrite(path)))
                {
                    foreach(var entry in map)
                        writer.WriteLine("{0}\t{1}", entry.Key, entry.Value);
                }
            }
        }
    }
}