using ClangSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Buildozer.ShaderCompiler
{
    
    public class AstPrinter : Expression.Visitor<string>
    {
        public string Print(Expression expr)
        {
            return expr.Accept(this);
        }

        public string VisitBinaryExpr(BinaryExpr expr)
        {
            return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
        }

        public string VisitErrorExpr(ErrorExpr expr)
        {
            return expr.Message;
        }

        public string VisitGroupingExpr(GroupingExpr expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        public string VisitLiteralExpr(LiteralExpr expr)
        {
            return expr.Value.ToString()!;
        }

        public string VisitUnaryExpr(UnaryExpr expr)
        {
            return Parenthesize(expr.Operator.Lexeme, expr.Right);
        }

        private string Parenthesize(String name, params Expression[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append($"({name}");
            foreach (Expression expr in exprs)
            {
                builder.Append($"  {expr.Accept(this)}");
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
    
}
