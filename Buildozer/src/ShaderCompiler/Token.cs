using System;
using System.Collections.Generic;
using System.Text;

namespace Buildozer.ShaderCompiler
{
    public enum TokenType
    {
        LeftParen, RightParen, LeftBrace, RightBrace,

        Comma, Dot, Minus, Plus, Semicolon, Slash, Star, Equal,

        LogicalOr, LogicalAnd, LogicalNot,

        NotEqual, EqualEqual, Greater, GreaterEqual, Less, LessEqual,

        Identifier, String, Float, Integer,

        If, Else, For, While,

        True, False,

        Fn, Return,

        Var,

        Intrinsic,

        Error, Eof
    }

    public class Token
    {
        public readonly TokenType Type;
        public readonly string Lexeme;
        public readonly object? Literal;
        public readonly int Line;

        public Token(TokenType type, string lexeme, object? literal, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }

        public override string ToString()
        {
            return $"{Type} {Lexeme} {Literal}";
        }
    }
}
