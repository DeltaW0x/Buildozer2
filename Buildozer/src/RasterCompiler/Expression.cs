using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildozer.RasterCompiler
{
    public abstract class Expression
    {
        public interface ExprVisitor<T>
        {
            public T VisitBinaryExpr(BinaryExpr expr);
            public T VisitGroupingExpr(GroupingExpr expr);
            public T VisitLiteralExpr(LiteralExpr expr);
            public T VisitUnaryExpr(UnaryExpr expr);
        }
        
        public abstract T Accept<T>(ExprVisitor<T> visitor);
    }

    public class BinaryExpr : Expression 
    {
        public readonly Token Operator;
        public readonly Expression Left;
        public readonly Expression Right;

        public BinaryExpr(Expression left, Token op, Expression right)
        {
            Left = left;
            Right = right;
            Operator = op;
        }

        public override T Accept<T>(ExprVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpr(this);
        }
    }

    public class GroupingExpr : Expression
    {
        public readonly Expression Expr;

        public GroupingExpr(Expression expr)
        {
            Expr = expr;
        }

        public override T Accept<T>(ExprVisitor<T> visitor)
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

        public override T Accept<T>(ExprVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpr(this);
        }
    }

    public class UnaryExpr : Expression
    {
        public readonly Token Operator;
        public readonly Expression Right;

        public UnaryExpr(Token op, Expression right) 
        {
            Right = right;
            Operator = op;
        }

        public override T Accept<T>(ExprVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpr(this);
        }
    }
}
