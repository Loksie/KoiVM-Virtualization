#region

using System.Collections.Generic;
using System.Text;

#endregion

namespace Confuser.DynCipher.AST
{
    public class StatementBlock : Statement
    {
        public StatementBlock()
        {
            Statements = new List<Statement>();
        }

        public IList<Statement> Statements
        {
            get;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            foreach(var i in Statements)
                sb.AppendLine(i.ToString());
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}