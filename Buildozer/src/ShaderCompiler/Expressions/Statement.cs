using System;
using System.Collections.Generic;
using System.Text;

namespace Buildozer.ShaderCompiler
{
    public abstract class Statement
    {
        public interface Visitor<T>
        {
            T VisitExpressionStmt(ExpressionStmt stmt);
        }

        public abstract T Accept<T>(Visitor<T> visitor);
    }

    public class ExpressionStmt : Statement
    {
        public readonly Expression Expr;

        public ExpressionStmt(Expression expr)
        {
            Expr = expr;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitExpressionStmt(this);
        }
    }
}
