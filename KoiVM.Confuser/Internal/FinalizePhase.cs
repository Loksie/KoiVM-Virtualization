#region

using System.Reflection;
using Confuser.Core;

#endregion

namespace KoiVM.Confuser.Internal
{
    [Obfuscation(Exclude = false, Feature = "+koi;")]
    public class FinalizePhase : ProtectionPhase
    {
        public FinalizePhase(Protection parent)
            : base(parent)
        {
        }

        public override ProtectionTargets Targets => ProtectionTargets.Modules;

        public override string Name => "Finalize virtualization data";

        protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
        {
            //var vr = context.Annotations.Get<Virtualizer>(context, Fish.VirtualizerKey);
            //vr.CommitRuntime();
        }
    }
}