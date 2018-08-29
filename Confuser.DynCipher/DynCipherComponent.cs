#region

using Confuser.Core;

#endregion

namespace Confuser.DynCipher
{
    internal class DynCipherComponent : ConfuserComponent
    {
        public const string _ServiceId = "Confuser.DynCipher";

        public override string Name => "Dynamic Cipher";

        public override string Description => "Provides dynamic cipher generation services.";

        public override string Id => _ServiceId;

        public override string FullId => _ServiceId;

        protected override void Initialize(ConfuserContext context)
        {
            context.Registry.RegisterService(_ServiceId, typeof(IDynCipherService), new DynCipherService());
        }

        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            //
        }
    }
}