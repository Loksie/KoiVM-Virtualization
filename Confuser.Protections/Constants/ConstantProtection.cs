#region

using Confuser.Core;
using Confuser.Protections.Constants;
using dnlib.DotNet;

#endregion

namespace Confuser.Protections
{
    public interface IConstantService
    {
        void ExcludeMethod(ConfuserContext context, MethodDef method);
    }

    [BeforeProtection("Ki.ControlFlow")]
    [AfterProtection("Ki.RefProxy")]
    internal class ConstantProtection : Protection, IConstantService
    {
        public const string _Id = "constants";
        public const string _FullId = "Ki.Constants";
        public const string _ServiceId = "Ki.Constants";
        internal static readonly object ContextKey = new object();

        public override string Name => "Constants Protection";

        public override string Description => "This protection encodes and compresses constants in the code.";

        public override string Id => _Id;

        public override string FullId => _FullId;

        public override ProtectionPreset Preset => ProtectionPreset.Normal;

        public void ExcludeMethod(ConfuserContext context, MethodDef method)
        {
            ProtectionParameters.GetParameters(context, method).Remove(this);
        }

        protected override void Initialize(ConfuserContext context)
        {
            context.Registry.RegisterService(_ServiceId, typeof(IConstantService), this);
        }

        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPreStage(PipelineStage.ProcessModule, new InjectPhase(this));
            pipeline.InsertPostStage(PipelineStage.ProcessModule, new EncodePhase(this));
        }
    }
}