#region

using dnlib.DotNet;

#endregion

namespace Confuser.Core.Project.Patterns
{
    /// <summary>
    ///     The OR operator.
    /// </summary>
    public class OrOperator : PatternOperator
    {
        internal const string OpName = "or";

        /// <inheritdoc />
        public override string Name => OpName;

        /// <inheritdoc />
        public override bool IsUnary => false;

        /// <inheritdoc />
        public override object Evaluate(IDnlibDef definition)
        {
            var a = (bool) OperandA.Evaluate(definition);
            if(a) return true;
            return (bool) OperandB.Evaluate(definition);
        }
    }
}