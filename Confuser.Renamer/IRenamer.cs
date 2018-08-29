#region

using Confuser.Core;
using dnlib.DotNet;

#endregion

namespace Confuser.Renamer
{
    public interface IRenamer
    {
        void Analyze(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def);
        void PreRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def);
        void PostRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def);
    }
}