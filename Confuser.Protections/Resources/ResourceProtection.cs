#region

using Confuser.Core;
using Confuser.Protections.Resources;

#endregion

namespace Confuser.Protections
{
    [BeforeProtection("Ki.ControlFlow")]
    [AfterProtection("Ki.Constants")]
    internal class ResourceProtection : Protection
    {
        public const string _Id = "resources";
        public const string _FullId = "Ki.Resources";
        public const string _ServiceId = "Ki.Resources";

        public override string Name => "Resources Protection";

        public override string Description => "This protection encodes and compresses the embedded resources.";

        public override string Id => _Id;

        public override string FullId => _FullId;

        public override ProtectionPreset Preset => ProtectionPreset.Normal;

        protected override void Initialize(ConfuserContext context)
        {
        }

        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPreStage(PipelineStage.ProcessModule, new InjectPhase(this));
        }
    }
}