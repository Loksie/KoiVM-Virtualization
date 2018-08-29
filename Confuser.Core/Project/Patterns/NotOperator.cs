#region

using dnlib.DotNet;

#endregion

namespace Confuser.Core.Project.Patterns
{
    /// <summary>
    ///     The NOT operator.
    /// </summary>
    public class NotOperator : PatternOperator
    {
        internal const string OpName = "not";

        /// <inheritdoc />
        public override string Name => OpName;

        /// <inheritdoc />
        public override bool IsUnary => true;

        /// <inheritdoc />
        public override object Evaluate(IDnlibDef definition)
        {
            return !(bool) OperandA.Evaluate(definition);
        }
    }
}