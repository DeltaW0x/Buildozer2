using ClangSharp;
using Serilog;

namespace Buildozer.ShaderCompiler
{
    internal class VariableEnvironment
    {
        public readonly Dictionary<string, object> Values = new Dictionary<string, object>();

        public object? Get(Token name)
        {
            if(Values.TryGetValue(name.Lexeme, out object? value))
                return value;

            Log.Error($"Undefined variable '{name.Lexeme}'.");
            return null;
        }

        public void Define(string name, object value)
        {
            Values[name] = value;
        }
    }

    public class Parser
    {
        private readonly Token[] _tokens;
        private int _current = 0;

        public Parser(Token[] tokens)
        {
            _tokens = tokens;
        }

        public Statement?[] Parse()
        {
            List<Statement?> statements = new List<Statement?>();

            while (!IsEnd())
                statements.Add(Declaration());

            return statements.ToArray();
        }

        private Statement? Declaration()
        {
            if (Match(TokenType.Error))
            {
                Syncronize();
                return null;
            }

            if (Match(TokenType.Var))
                return VarDeclaration();

            return Statement();
        }

        private Statement Statement()
        {
            return ExpressionStatement();
        }

        private Statement VarDeclaration()
        {
            List<Token> modifiers = new List<Token>();
            while (Match(TokenType.Const))
            {
                modifiers.Add(Previous());
            }

            Token? name = Consume(TokenType.Identifier, "Expected variable name");

            Expression? initializer = null;
            if (Match(TokenType.Equal))
            {
                initializer = Expression();
            }

            Consume(TokenType.Semicolon, "Expected ';' after variable declaration");
            return new VariableStmt(name!, modifiers.ToArray(), initializer);
        }

        private Statement ExpressionStatement()
        {
            Expression expr = Expression();
            Consume(TokenType.Semicolon, "Missing ';' after expression.");
            return new ExpressionStmt(expr);
        }

        private Expression Expression()
        {
            return Equality();
        }

        private Expression Equality()
        {
            Expression expr = Comparison();

            while(Match(TokenType.NotEqual, TokenType.EqualEqual))
            {
                expr = new BinaryExpr(expr, Previous(), Comparison());
            }

            return expr;
        }

        private Expression Comparison()
        {
            Expression expr = Term();

            while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                expr = new BinaryExpr(expr, Previous(), Term());
            }

            return expr;
        }

        private Expression Term()
        {
            Expression expr = Factor();

            while (Match(TokenType.Minus, TokenType.Plus))
            {
                expr = new BinaryExpr(expr, Previous(), Factor());
            }

            return expr;
        }

        private Expression Factor()
        {
            Expression expr = Unary();

            while (Match(TokenType.Slash, TokenType.Star))
            {
                expr = new BinaryExpr(expr, Previous(), Unary());
            }

            return expr;
        }

        private Expression Unary()
        {
           if(Match(TokenType.LogicalNot, TokenType.Minus))
           {
                return new UnaryExpr(Previous(), Unary());
           }
            return Primary();
        }

        private Expression Primary()
        {
            if (Match(TokenType.True)) 
                return new LiteralExpr(true);
            if (Match(TokenType.False))
                return new LiteralExpr(false);

            if (Match(TokenType.Integer, TokenType.Float, TokenType.String))
                return new LiteralExpr(Previous().Literal!);

            if (Match(TokenType.LeftParen))
            {
                Expression expr = Expression();
                Consume(TokenType.RightParen, "Expected ')' after expression");
                return new GroupingExpr(expr);
            }

            if (Match(TokenType.Identifier))
                return new VariableExpr(Previous());

            if(Match(TokenType.Error))
                return new ErrorExpr($"Invalid token {Previous().Lexeme} on line {Previous().Line}");

            return new ErrorExpr($"Error at line {Peek().Line} '{Peek().Lexeme}': Expected expression");
        }

        private Token? Consume(TokenType type, string errorMessage)
        {
            if (Check(type))
                return Advance();

            Log.Error($"Error at line {Peek().Line}: {errorMessage}");
            return null;
        }
        
        private Token Advance()
        {
            if (!IsEnd()) 
                _current++;
            return Previous();
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        private bool IsEnd()
        {
            return Peek().Type == TokenType.Eof;
        }

        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsEnd())
                return false;
            return Peek().Type == type;
        }

        private void Syncronize()
        {
            Advance();

            while (!IsEnd())
            {
                if (Previous().Type == TokenType.Semicolon)
                    return;

                switch (Peek().Type)
                {
                    case TokenType.Fn:
                    case TokenType.Return:
                    case TokenType.Var:
                    case TokenType.For:
                    case TokenType.While:
                    case TokenType.If:
                    case TokenType.Else:
                        return;
                }

                Advance();
            }
        }
    }
}
