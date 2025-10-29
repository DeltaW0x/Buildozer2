namespace Buildozer.RasterCompiler
{
    public enum TokenType
    {
        LeftParen, RightParen,
        LeftBrace, RightBrace,
        LeftBracket, RightBracket,
 
        Comma, Dot, Minus, Plus, Semilcolon, Slash, Star,

        NotEqual, Equal, EqualEqual,
        Greater, GreaterEqual, Less, LessEqual,

        Identifier, Attribute, StringLiteral, IntegerLiteral, FloatingLiteral,

        BoolType,
        HalfType, FloatType, DoubleType,
        U8IntType, U16IntType, U32IntType, U64IntType,
        I8IntType, I16IntType, I32IntType, I64IntType,

        Bool2Type, Bool3Type, Bool4Type,
        Half2Type, Half3Type, Half4Type,
        Float2Type, Float3Type, Float4Type,

        Half2x2Type, Half2x3Type, Half2x4Type,
        Half3x2Type, Half3x3Type, Half3x4Type,
        Half4x2Type, Half4x3Type, Half4x4Type,
        Float2x2Type, Float2x3Type, Float2x4Type,
        Float3x2Type, Float3x3Type, Float3x4Type,
        Float4x2Type, Float4x3Type, Float4x4Type,

        And, Or, Not, True, False,
        If, Else, While, For, Fn, Return, Var, Const, Shared,
        
        PrintIntr, IsNanIntr,

        Error, Eof
    }

    public struct Token
    {
        public TokenType Type { get; private set; }
        public string  Lexeme { get; private set; }
        public object? Literal { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }

        public Token(TokenType type, string lexeme, object? literal, int line, int column)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"{Type} {Lexeme} {Literal}";
        }
    }
}
