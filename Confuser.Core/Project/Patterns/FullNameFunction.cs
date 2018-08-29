#region

using dnlib.DotNet;

#endregion

namespace Confuser.Core.Project.Patterns
{
    /// <summary>
    ///     A function that compare the full name of definition.
    /// </summary>
    public class FullNameFunction : PatternFunction
    {
        internal const string FnName = "full-name";

        /// <inheritdoc />
        public override string Name => FnName;

        /// <inheritdoc />
        public override int ArgumentCount => 1;

        /// <inheritdoc />
        public override object Evaluate(IDnlibDef definition)
        {
            var name = Arguments[0].Evaluate(definition);
            return definition.FullName == name.ToString();
        }
    }
}