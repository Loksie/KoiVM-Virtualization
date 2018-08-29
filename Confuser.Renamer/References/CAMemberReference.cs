#region

using Confuser.Core;
using dnlib.DotNet;

#endregion

namespace Confuser.Renamer.References
{
    internal class CAMemberReference : INameReference<IDnlibDef>
    {
        private readonly IDnlibDef definition;
        private readonly CANamedArgument namedArg;

        public CAMemberReference(CANamedArgument namedArg, IDnlibDef definition)
        {
            this.namedArg = namedArg;
            this.definition = definition;
        }

        public bool UpdateNameReference(ConfuserContext context, INameService service)
        {
            namedArg.Name = definition.Name;
            return true;
        }

        public bool ShouldCancelRename()
        {
            return false;
        }
    }
}