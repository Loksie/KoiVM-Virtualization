#region

using System.Diagnostics;
using System.Security.Cryptography;
using Confuser.Core;
using Confuser.Renamer;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;

#endregion

namespace Confuser.Protections.Compress
{
    internal class StubProtection : Protection
    {
        private readonly CompressorContext ctx;
        private readonly ModuleDef originModule;

        internal StubProtection(CompressorContext ctx, ModuleDef originModule)
        {
            this.ctx = ctx;
            this.originModule = originModule;
        }

        public override string Name => "Compressor Stub Protection";

        public override string Description => "Do some extra works on the protected stub.";

        public override string Id => "Ki.Compressor.Protection";

        public override string FullId => "Ki.Compressor.Protection";

        public override ProtectionPreset Preset => ProtectionPreset.None;

        protected override void Initialize(ConfuserContext context)
        {
            //
        }

        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            if(!ctx.CompatMode)
                pipeline.InsertPreStage(PipelineStage.Inspection, new InjPhase(this));
            pipeline.InsertPostStage(PipelineStage.BeginModule, new SigPhase(this));
        }

        private class InjPhase : ProtectionPhase
        {
            public InjPhase(StubProtection parent)
                : base(parent)
            {
            }

            public override ProtectionTargets Targets => ProtectionTargets.Modules;

            public override bool ProcessAll => true;

            public override string Name => "Module injection";

            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                // Hack the origin module into the assembly to make sure correct type resolution
                var originModule = ((StubProtection) Parent).originModule;
                originModule.Assembly.Modules.Remove(originModule);
                context.Modules[0].Assembly.Modules.Add(((StubProtection) Parent).originModule);
            }
        }

        private class SigPhase : ProtectionPhase
        {
            public SigPhase(StubProtection parent)
                : base(parent)
            {
            }

            public override ProtectionTargets Targets => ProtectionTargets.Modules;

            public override string Name => "Packer info encoding";

            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                var field = context.CurrentModule.Types[0].FindField("DataField");
                Debug.Assert(field != null);
                context.Registry.GetService<INameService>().SetCanRename(field, true);

                context.CurrentModuleWriterListener.OnWriterEvent += (sender, e) =>
                {
                    if(e.WriterEvent == ModuleWriterEvent.MDBeginCreateTables)
                    {
                        // Add key signature
                        var writer = (ModuleWriterBase) sender;
                        var prot = (StubProtection) Parent;
                        var blob = writer.MetaData.BlobHeap.Add(prot.ctx.KeySig);
                        var rid = writer.MetaData.TablesHeap.StandAloneSigTable.Add(new RawStandAloneSigRow(blob));
                        Debug.Assert((0x11000000 | rid) == prot.ctx.KeyToken);

                        if(prot.ctx.CompatMode)
                            return;

                        // Add File reference
                        var hash = SHA1.Create().ComputeHash(prot.ctx.OriginModule);
                        var hashBlob = writer.MetaData.BlobHeap.Add(hash);

                        var fileTbl = writer.MetaData.TablesHeap.FileTable;
                        var fileRid = fileTbl.Add(new RawFileRow(
                            (uint) FileAttributes.ContainsMetaData,
                            writer.MetaData.StringsHeap.Add("koi"),
                            hashBlob));
                    }
                };
            }
        }
    }
}