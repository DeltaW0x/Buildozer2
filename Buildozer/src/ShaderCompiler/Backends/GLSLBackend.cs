namespace Buildozer.ShaderCompiler
{
    public class GLSLBackend : Expression.Visitor<string>, Statement.Visitor<string>
    {
        private VariableEnvironment _globals = new VariableEnvironment();

        public string VisitBinaryExpr(BinaryExpr expr)
        {
            throw new NotImplementedException();
        }

        public string VisitErrorExpr(ErrorExpr expr)
        {
            throw new NotImplementedException();
        }

        public string VisitGroupingExpr(GroupingExpr expr)
        {
            throw new NotImplementedException();
        }

        public string VisitLiteralExpr(LiteralExpr expr)
        {
            throw new NotImplementedException();
        }

        public string VisitUnaryExpr(UnaryExpr expr)
        {
            throw new NotImplementedException();
        }

        public string VisitVariableExpr(VariableExpr expr)
        {
            throw new NotImplementedException();
        }

        public string VisitExpressionStmt(ExpressionStmt stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitVariableStmt(VariableStmt stmt)
        {
            throw new NotImplementedException();
        }
    }
}
