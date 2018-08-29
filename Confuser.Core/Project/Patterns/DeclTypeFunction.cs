#region

using dnlib.DotNet;

#endregion

namespace Confuser.Core.Project.Patterns
{
    /// <summary>
    ///     A function that compare the full name of declaring type.
    /// </summary>
    public class DeclTypeFunction : PatternFunction
    {
        internal const string FnName = "decl-type";

        /// <inheritdoc />
        public override string Name => FnName;

        /// <inheritdoc />
        public override int ArgumentCount => 1;

        /// <inheritdoc />
        public override object Evaluate(IDnlibDef definition)
        {
            if(!(definition is IMemberDef) || ((IMemberDef) definition).DeclaringType == null)
                return false;
            var fullName = Arguments[0].Evaluate(definition);
            return ((IMemberDef) definition).DeclaringType.FullName == fullName.ToString();
        }
    }
}