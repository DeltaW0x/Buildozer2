namespace Buildozer.ShaderCompiler
{
    public class GLSLBackend : Expression.Visitor<string>, Statement.Visitor<string>
    {
        public string Compile(Statement[] statements)
        {
            foreach (var statement in statements) 
            { 
                return statement.Accept(this);
            }
            return "";
        }

        public string VisitBinaryExpr(BinaryExpr expr)
        {
            return $"{Evaluate(expr.Left)} {expr.Operator.Lexeme} {Evaluate(expr.Right)}";
        }

        public string VisitErrorExpr(ErrorExpr expr)
        {
            throw new ArgumentException("Invalid expression");
        }

        public string VisitGroupingExpr(GroupingExpr expr)
        {
            return $"({Evaluate(expr.Expression)})";
        }

        public string VisitLiteralExpr(LiteralExpr expr)
        {
            return expr.Value.ToString()!;
        }

        public string VisitUnaryExpr(UnaryExpr expr)
        {
            return $"{expr.Operator.Lexeme}{Evaluate(expr.Right)}";
        }

        public string Evaluate(Expression expr)
        {
            return expr.Accept(this);
        }

        public string VisitExpressionStmt(ExpressionStmt stmt)
        {
            return $"{Evaluate(stmt.Expr)};";
        }
    }
}
