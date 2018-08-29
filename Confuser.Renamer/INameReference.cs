#region

using Confuser.Core;

#endregion

namespace Confuser.Renamer
{
    public interface INameReference
    {
        bool UpdateNameReference(ConfuserContext context, INameService service);

        bool ShouldCancelRename();
    }

    public interface INameReference<out T> : INameReference
    {
    }
}