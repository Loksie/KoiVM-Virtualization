#region

using System.Collections.Generic;
using System.Linq;
using Confuser.Core.Services;
using Confuser.DynCipher.AST;

#endregion

namespace Confuser.DynCipher.Transforms
{
    internal class ShuffleTransform
    {
        private const int ITERATION = 20;

        private static IEnumerable<Variable> GetVariableUsage(Expression exp)
        {
            if(exp is VariableExpression)
                yield return ((VariableExpression) exp).Variable;
            else if(exp is ArrayIndexExpression)
                foreach(var i in GetVariableUsage(((ArrayIndexExpression) exp).Array))
                    yield return i;
            else if(exp is BinOpExpression)
                foreach(var i in GetVariableUsage(((BinOpExpression) exp).Left)
                    .Concat(GetVariableUsage(((BinOpExpression) exp).Right)))
                    yield return i;
            else if(exp is UnaryOpExpression)
                foreach(var i in GetVariableUsage(((UnaryOpExpression) exp).Value))
                    yield return i;
        }

        private static IEnumerable<Variable> GetVariableUsage(Statement st)
        {
            if(st is AssignmentStatement)
                foreach(var i in GetVariableUsage(((AssignmentStatement) st).Value))
                    yield return i;
        }

        private static IEnumerable<Variable> GetVariableDefinition(Expression exp)
        {
            if(exp is VariableExpression)
                yield return ((VariableExpression) exp).Variable;
        }

        private static IEnumerable<Variable> GetVariableDefinition(Statement st)
        {
            if(st is AssignmentStatement)
                foreach(var i in GetVariableDefinition(((AssignmentStatement) st).Target))
                    yield return i;
        }


        // Cannot go before the statements that use the variable defined at the statement
        // Cannot go further than the statements that override the variable used at the statement
        private static int SearchUpwardKill(TransformContext context, Statement st, StatementBlock block, int startIndex)
        {
            var usage = context.Usages[st];
            var definition = context.Definitions[st];
            for(var i = startIndex - 1; i >= 0; i--)
                if(context.Usages[block.Statements[i]].Intersect(definition).Count() > 0 ||
                   context.Definitions[block.Statements[i]].Intersect(usage).Count() > 0)
                    return i;
            return 0;
        }

        private static int SearchDownwardKill(TransformContext context, Statement st, StatementBlock block, int startIndex)
        {
            var usage = context.Usages[st];
            var definition = context.Definitions[st];
            for(var i = startIndex + 1; i < block.Statements.Count; i++)
                if(context.Usages[block.Statements[i]].Intersect(definition).Count() > 0 ||
                   context.Definitions[block.Statements[i]].Intersect(usage).Count() > 0)
                    return i;
            return block.Statements.Count - 1;
        }

        public static void Run(StatementBlock block, RandomGenerator random)
        {
            var context = new TransformContext
            {
                Statements = block.Statements.ToArray(),
                Usages = block.Statements.ToDictionary(s => s, s => GetVariableUsage(s).ToArray()),
                Definitions = block.Statements.ToDictionary(s => s, s => GetVariableDefinition(s).ToArray())
            };
            for(var i = 0; i < ITERATION; i++)
                foreach(var st in context.Statements)
                {
                    var index = block.Statements.IndexOf(st);
                    var vars = GetVariableUsage(st).Concat(GetVariableDefinition(st)).ToArray();

                    // Statement can move between defIndex & useIndex without side effects
                    var defIndex = SearchUpwardKill(context, st, block, index);
                    var useIndex = SearchDownwardKill(context, st, block, index);


                    // Move to a random spot in the interval
                    var newIndex = defIndex + random.NextInt32(1, useIndex - defIndex);
                    if(newIndex > index) newIndex--;
                    block.Statements.RemoveAt(index);
                    block.Statements.Insert(newIndex, st);
                }
        }

        private class TransformContext
        {
            public Dictionary<Statement, Variable[]> Definitions;
            public Statement[] Statements;
            public Dictionary<Statement, Variable[]> Usages;
        }
    }
}