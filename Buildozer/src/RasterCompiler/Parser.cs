using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Buildozer.RasterCompiler
{
    public class Parser
    {
        private int _current = 0;
        private readonly Token[] _tokens;

        public Parser(Token[] tokens)
        {
            _tokens = tokens;
        }

        public Expression Parse()
        {
            try
            {
                return ParseExpression();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
                throw new Exception();
            }
        }

        private Expression ParseExpression() 
        {
            return ParseEquality();
        }

        private Expression ParseEquality()
        {
            Expression expr = ParseComparison();

            while(Match(TokenType.NotEqual, TokenType.EqualEqual))
            {
                Token op = Previous();
                Expression right = ParseComparison();
                expr = new BinaryExpr(expr, op, right);
            }

            return expr;
        }

        private Expression ParseComparison()
        {
            Expression expr = ParseTerm();

            while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                Token op = Previous();
                Expression right = ParseTerm();
                expr = new BinaryExpr(expr, op, right);
            }

            return expr;
        }

        private Expression ParseTerm()
        {
            Expression expr = ParseFactor();

            while (Match(TokenType.Minus, TokenType.Plus))
            {
                Token op = Previous();
                Expression right = ParseFactor();
                expr = new BinaryExpr(expr, op, right);
            }

            return expr;
        }

        private Expression ParseFactor()
        {
            Expression expr = ParseUnary();

            while (Match(TokenType.Slash, TokenType.Star))
            {
                Token op = Previous();
                Expression right = ParseUnary();
                expr = new BinaryExpr(expr, op, right);
            }

            return expr;
        }

        private Expression ParseUnary()
        {
            if(Match(TokenType.Not, TokenType.Minus))
            {
                Token op = Previous();
                Expression right = ParseUnary();
                return new UnaryExpr(op, right);
            }

            return ParsePrimary();
        }

        private Expression ParsePrimary()
        {
            if (Match(TokenType.False))
                return new LiteralExpr(false);
            if (Match(TokenType.True))
                return new LiteralExpr(true);

            if (Match(TokenType.StringLiteral, TokenType.IntegerLiteral, TokenType.FloatingLiteral))
                return new LiteralExpr(Previous().Literal!);

            if (Match(TokenType.LeftParen))
            {
                Expression expr = ParseExpression();
                Consume(TokenType.RightParen, "Missing right parentesis");
                return new GroupingExpr(expr);
            }

            throw ParseError(Peek(), "Expected expression");
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) 
                return Advance();

            throw ParseError(Peek(), message);
        }

        private Exception ParseError(Token token, string message)
        {
           return new Exception($"{message}, line {token.Line}, token {token.Lexeme}");
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

        private Token Advance()
        {
            if (!IsEnd()) 
                _current++;

            return Previous();
        }

        private bool IsEnd()
        {
            return Peek().Type == TokenType.Eof;
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }

    }
}
