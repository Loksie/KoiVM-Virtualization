#region

using dnlib.DotNet;

#endregion

namespace Confuser.Core.Project.Patterns
{
    /// <summary>
    ///     A function that indicate whether the type inherits from the specified type.
    /// </summary>
    public class InheritsFunction : PatternFunction
    {
        internal const string FnName = "inherits";

        /// <inheritdoc />
        public override string Name => FnName;

        /// <inheritdoc />
        public override int ArgumentCount => 1;

        /// <inheritdoc />
        public override object Evaluate(IDnlibDef definition)
        {
            var name = Arguments[0].Evaluate(definition).ToString();

            var type = definition as TypeDef;
            if(type == null && definition is IMemberDef)
                type = ((IMemberDef) definition).DeclaringType;
            if(type == null)
                return false;

            if(type.InheritsFrom(name) || type.Implements(name))
                return true;

            return false;
        }
    }
}