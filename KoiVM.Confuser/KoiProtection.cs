#region

using System.Reflection;
using Confuser.Core;
using KoiVM.Confuser.Internal;

#endregion

namespace KoiVM.Confuser
{
    [Obfuscation(Exclude = false, Feature = "-rename", ApplyToMembers = false)]
    [BeforeProtection("Ki.ControlFlow", "Ki.AntiTamper")]
    [AfterProtection("Ki.Constants")]
    public class KoiProtection : Protection
    {
        public const string _Id = "koi";
        public const string _FullId = "Ki.Koi";

        public override string Name => "Koi Virtualizer";

        public override string Description => "A majestic Koi fish (or Magikarp, if you prefer) will virtualize your code!";

        public override string Id => _Id;

        public override string FullId => _FullId;

        public override ProtectionPreset Preset => ProtectionPreset.Maximum;

        protected override void Initialize(ConfuserContext context)
        {
            KoiInfo.Init(context);
        }

        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPostStage(PipelineStage.Inspection,
                new InitializePhase(this, KoiInfo.KoiDirectory));
            pipeline.InsertPreStage(PipelineStage.EndModule, new MarkPhase(this));
            pipeline.InsertPreStage(PipelineStage.Debug, new FinalizePhase(this));
            pipeline.InsertPreStage(PipelineStage.Pack, new SavePhase(this));
        }
    }
}