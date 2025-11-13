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
            T VisitVariableStmt(VariableStmt stmt);
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

    public class VariableStmt : Statement
    {
        public readonly Token Name;
        public readonly Token[] Modifiers;
        public readonly Expression? Initializer;

        public VariableStmt(Token name, Token[] modifiers, Expression? initializer)
        {
            Name = name;
            Modifiers = modifiers;
            Initializer = initializer;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitVariableStmt(this);
        }
    }
}
