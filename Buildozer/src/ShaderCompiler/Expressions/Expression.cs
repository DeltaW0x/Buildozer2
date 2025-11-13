using System;
using System.Collections.Generic;
using System.Text;

namespace Buildozer.ShaderCompiler
{
    public abstract class Expression
    {
        public interface Visitor<T>
        {
            T VisitBinaryExpr(BinaryExpr expr);
            T VisitGroupingExpr(GroupingExpr expr);
            T VisitLiteralExpr(LiteralExpr expr);
            T VisitUnaryExpr(UnaryExpr expr);
            T VisitVariableExpr(VariableExpr expr);
            T VisitErrorExpr(ErrorExpr expr);
        }

        public abstract T Accept<T>(Visitor<T> visitor);
    }

    public class BinaryExpr : Expression
    {
        public readonly Expression Left;
        public readonly Expression Right;
        public readonly Token Operator;

        public BinaryExpr(Expression left, Token op, Expression right)
        {
            Left = left;
            Right = right;
            Operator = op;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
           return visitor.VisitBinaryExpr(this);
        }
    }

    public class GroupingExpr : Expression
    {
        public readonly Expression Expression;

        public GroupingExpr(Expression expr)
        {
            Expression = expr;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitGroupingExpr(this);
        }
    }

    public class LiteralExpr : Expression
    {
        public readonly object Value;

        public LiteralExpr(object value)
        {
            Value = value;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitLiteralExpr(this);
        }
    }

    public class UnaryExpr : Expression
    {
        public readonly Expression Right;
        public readonly Token Operator;
        
        public UnaryExpr(Token op, Expression right)
        {
            Right = right;
            Operator = op;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitUnaryExpr(this);
        }
    }

    public class VariableExpr : Expression
    {
        public readonly Token Name;

        public VariableExpr(Token name)
        {
            Name = name;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitVariableExpr(this);
        }
    }

    public class ErrorExpr : Expression
    {
        public readonly string Message;

        public ErrorExpr(string message)
        {
            Message = message;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitErrorExpr(this);
        }
    }
}
