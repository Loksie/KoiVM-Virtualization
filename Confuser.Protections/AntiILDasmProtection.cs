#region

using System.Linq;
using Confuser.Core;
using dnlib.DotNet;

#endregion

namespace Confuser.Protections
{
    internal class AntiILDasmProtection : Protection
    {
        public const string _Id = "anti ildasm";
        public const string _FullId = "Ki.AntiILDasm";

        public override string Name => "Anti IL Dasm Protection";

        public override string Description => "This protection marks the module with a attribute that discourage ILDasm from disassembling it.";

        public override string Id => _Id;

        public override string FullId => _FullId;

        public override ProtectionPreset Preset => ProtectionPreset.Minimum;

        protected override void Initialize(ConfuserContext context)
        {
            //
        }

        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPreStage(PipelineStage.ProcessModule, new AntiILDasmPhase(this));
        }

        private class AntiILDasmPhase : ProtectionPhase
        {
            public AntiILDasmPhase(AntiILDasmProtection parent)
                : base(parent)
            {
            }

            public override ProtectionTargets Targets => ProtectionTargets.Modules;

            public override string Name => "Anti-ILDasm marking";

            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                foreach(var module in parameters.Targets.OfType<ModuleDef>())
                {
                    var attrRef = module.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", "SuppressIldasmAttribute");
                    var ctorRef = new MemberRefUser(module, ".ctor", MethodSig.CreateInstance(module.CorLibTypes.Void), attrRef);

                    var attr = new CustomAttribute(ctorRef);
                    module.CustomAttributes.Add(attr);
                }
            }
        }
    }
}