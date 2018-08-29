#region

using dnlib.DotNet;

#endregion

namespace Confuser.Core.Project.Patterns
{
    /// <summary>
    ///     A function that indicate whether the item has the given custom attribute.
    /// </summary>
    public class HasAttrFunction : PatternFunction
    {
        internal const string FnName = "has-attr";

        /// <inheritdoc />
        public override string Name => FnName;

        /// <inheritdoc />
        public override int ArgumentCount => 1;

        /// <inheritdoc />
        public override object Evaluate(IDnlibDef definition)
        {
            var attrName = Arguments[0].Evaluate(definition).ToString();
            return definition.CustomAttributes.IsDefined(attrName);
        }
    }
}