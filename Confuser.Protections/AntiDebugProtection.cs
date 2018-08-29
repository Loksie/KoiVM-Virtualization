#region

using System.Linq;
using Confuser.Core;
using Confuser.Core.Helpers;
using Confuser.Core.Services;
using Confuser.Renamer;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

#endregion

namespace Confuser.Protections
{
    [BeforeProtection("Ki.ControlFlow")]
    internal class AntiDebugProtection : Protection
    {
        public const string _Id = "anti debug";
        public const string _FullId = "Ki.AntiDebug";

        public override string Name => "Anti Debug Protection";

        public override string Description => "This protection prevents the assembly from being debugged or profiled.";

        public override string Id => _Id;

        public override string FullId => _FullId;

        public override ProtectionPreset Preset => ProtectionPreset.Minimum;

        protected override void Initialize(ConfuserContext context)
        {
            //
        }

        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPreStage(PipelineStage.ProcessModule, new AntiDebugPhase(this));
        }

        private class AntiDebugPhase : ProtectionPhase
        {
            public AntiDebugPhase(AntiDebugProtection parent)
                : base(parent)
            {
            }

            public override ProtectionTargets Targets => ProtectionTargets.Modules;

            public override string Name => "Anti-debug injection";

            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                var rt = context.Registry.GetService<IRuntimeService>();
                var marker = context.Registry.GetService<IMarkerService>();
                var name = context.Registry.GetService<INameService>();

                foreach(var module in parameters.Targets.OfType<ModuleDef>())
                {
                    var mode = parameters.GetParameter(context, module, "mode", AntiMode.Safe);

                    TypeDef rtType;
                    TypeDef attr = null;
                    const string attrName = "System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute";
                    switch(mode)
                    {
                        case AntiMode.Safe:
                            rtType = rt.GetRuntimeType("Confuser.Runtime.AntiDebugSafe");
                            break;
                        case AntiMode.Win32:
                            rtType = rt.GetRuntimeType("Confuser.Runtime.AntiDebugWin32");
                            break;
                        case AntiMode.Antinet:
                            rtType = rt.GetRuntimeType("Confuser.Runtime.AntiDebugAntinet");

                            attr = rt.GetRuntimeType(attrName);
                            module.Types.Add(attr = InjectHelper.Inject(attr, module));
                            foreach(var member in attr.FindDefinitions())
                            {
                                marker.Mark(member, (Protection) Parent);
                                name.Analyze(member);
                            }
                            name.SetCanRename(attr, false);
                            break;
                        default:
                            throw new UnreachableException();
                    }

                    var members = InjectHelper.Inject(rtType, module.GlobalType, module);

                    var cctor = module.GlobalType.FindStaticConstructor();
                    var init = (MethodDef) members.Single(method => method.Name == "Initialize");
                    cctor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, init));

                    foreach(var member in members)
                    {
                        marker.Mark(member, (Protection) Parent);
                        name.Analyze(member);

                        var ren = true;
                        if(member is MethodDef)
                        {
                            var method = (MethodDef) member;
                            if(method.Access == MethodAttributes.Public)
                                method.Access = MethodAttributes.Assembly;
                            if(!method.IsConstructor)
                                method.IsSpecialName = false;
                            else
                                ren = false;

                            var ca = method.CustomAttributes.Find(attrName);
                            if(ca != null)
                                ca.Constructor = attr.FindMethod(".ctor");
                        }
                        else if(member is FieldDef)
                        {
                            var field = (FieldDef) member;
                            if(field.Access == FieldAttributes.Public)
                                field.Access = FieldAttributes.Assembly;
                            if(field.IsLiteral)
                            {
                                field.DeclaringType.Fields.Remove(field);
                                continue;
                            }
                        }
                        if(ren)
                        {
                            member.Name = name.ObfuscateName(member.Name, RenameMode.Unicode);
                            name.SetCanRename(member, false);
                        }
                    }
                }
            }

            private enum AntiMode
            {
                Safe,
                Win32,
                Antinet
            }
        }
    }
}