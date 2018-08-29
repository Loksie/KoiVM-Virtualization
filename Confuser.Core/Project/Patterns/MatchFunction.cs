#region

using System.Text.RegularExpressions;
using dnlib.DotNet;

#endregion

namespace Confuser.Core.Project.Patterns
{
    /// <summary>
    ///     A function that match the full name of the definition with specified RegEx.
    /// </summary>
    public class MatchFunction : PatternFunction
    {
        internal const string FnName = "match";

        /// <inheritdoc />
        public override string Name => FnName;

        /// <inheritdoc />
        public override int ArgumentCount => 1;

        /// <inheritdoc />
        public override object Evaluate(IDnlibDef definition)
        {
            var regex = Arguments[0].Evaluate(definition).ToString();
            return Regex.IsMatch(definition.FullName, regex);
        }
    }

    /// <summary>
    ///     A function that match the name of the definition with specified RegEx.
    /// </summary>
    public class MatchNameFunction : PatternFunction
    {
        internal const string FnName = "match-name";

        /// <inheritdoc />
        public override string Name => FnName;

        /// <inheritdoc />
        public override int ArgumentCount => 1;

        /// <inheritdoc />
        public override object Evaluate(IDnlibDef definition)
        {
            var regex = Arguments[0].Evaluate(definition).ToString();
            return Regex.IsMatch(definition.Name, regex);
        }
    }

    /// <summary>
    ///     A function that match the name of declaring type with specified RegEx.
    /// </summary>
    public class MatchTypeNameFunction : PatternFunction
    {
        internal const string FnName = "match-type-name";

        /// <inheritdoc />
        public override string Name => FnName;

        /// <inheritdoc />
        public override int ArgumentCount => 1;

        /// <inheritdoc />
        public override object Evaluate(IDnlibDef definition)
        {
            if(definition is TypeDef)
            {
                var regex = Arguments[0].Evaluate(definition).ToString();
                return Regex.IsMatch(definition.Name, regex);
            }
            if(definition is IMemberDef && ((IMemberDef) definition).DeclaringType != null)
            {
                var regex = Arguments[0].Evaluate(definition).ToString();
                return Regex.IsMatch(((IMemberDef) definition).DeclaringType.Name, regex);
            }
            return false;
        }
    }
}