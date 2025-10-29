using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Buildozer.RasterCompiler
{
    public class Scanner
    {
        private readonly string _source;
        private readonly List<Token> _tokens = new();
        private readonly FrozenDictionary<string, TokenType> _reservedKeyworks;

        private int _start = 0;
        private int _current = 0;
        private int _line = 1;
       
        public Scanner(string source)
        {
            _source = source;

            _reservedKeyworks = new Dictionary<string, TokenType>()
            {
                {"and", TokenType.And },
                {"or", TokenType.Or },

                {"if", TokenType.If },
                {"else", TokenType.Else },

                {"for", TokenType.For },
                {"while", TokenType.While },

                {"true", TokenType.True },
                {"false", TokenType.False },

                {"var", TokenType.Var},
                {"fn", TokenType.Fn},
                {"return", TokenType.Return},

                {"const", TokenType.Const},
                {"shared", TokenType.Shared},

                {"bool", TokenType.BoolType},

                {"half", TokenType.HalfType},
                {"float", TokenType.FloatType},
                {"double", TokenType.DoubleType},

                {"uint", TokenType.U32IntType},
                {"uint8", TokenType.U8IntType},
                {"uint16", TokenType.U16IntType},
                {"uint32", TokenType.U32IntType},
                {"uint64", TokenType.U64IntType},

                {"int", TokenType.I32IntType },
                {"int8", TokenType.I8IntType},
                {"int16", TokenType.I16IntType},
                {"int32", TokenType.I32IntType},
                {"int64", TokenType.I64IntType},

                {"half2", TokenType.Half2Type},
                {"half3", TokenType.Half3Type},
                {"half4", TokenType.Half4Type},

                {"float2", TokenType.Float2Type},
                {"float3", TokenType.Float3Type},
                {"float4", TokenType.Float4Type},

                {"half2x2",TokenType.Half2x2Type},
                {"half2x3",TokenType.Half2x3Type},
                {"half2x4",TokenType.Half2x4Type},
                {"half3x2",TokenType.Half3x2Type},
                {"half3x3",TokenType.Half3x3Type},
                {"half3x4",TokenType.Half3x4Type},
                {"half4x2",TokenType.Half4x2Type},
                {"half4x3",TokenType.Half4x3Type},
                {"half4x4",TokenType.Half4x4Type},

                {"float2x2",TokenType.Float2x2Type},
                {"float2x3",TokenType.Float2x3Type},
                {"float2x4",TokenType.Float2x4Type},
                {"float3x2",TokenType.Float3x2Type},
                {"float3x3",TokenType.Float3x3Type},
                {"float3x4",TokenType.Float3x4Type},
                {"float4x2",TokenType.Float4x2Type},
                {"float4x3",TokenType.Float4x3Type},
                {"float4x4",TokenType.Float4x4Type},

                {"@print", TokenType.PrintIntr}

            }.ToFrozenDictionary();

        }

        public Token[] ScanTokens()
        {
            while (!IsEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.Eof, "", null, _line, _current));
            return _tokens.ToArray();
        }

        private bool IsEnd()
        {
            return _current >= _source.Length;
        }

        private char Advance()
        {
            return _source[_current++];
        }

        private bool Match(char expected)
        {
            if (IsEnd())
                return false;

            if (_source[_current] != expected)
                return false;

            _current++;
            return true;
        }

        private char Peek()
        {
            if (IsEnd())
                return '\0';
            return _source[_current];
        }

        private char PeekNext()
        {
            if (_current + 1 >= _source.Length)
                return '\0';

            return _source[_current + 1];
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object? literal)
        {
            string text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line, _current));
        }

        private void AddErrorToken(string message)
        {
            _tokens.Add(new Token(TokenType.Error, message, _source.Substring(_start, _current - _start), _line, _current));
        }

        private void AddString()
        {
            int startLine = _line;

            while (Peek() != '"' && !IsEnd())
            {
                if (Peek() == '\n')
                    _line++;
                Advance();
            }

            if (IsEnd())
            {
                AddErrorToken($"Unterminated string on line {startLine}");
                return;
            }

            Advance();
            AddToken(TokenType.StringLiteral, _source.Substring(_start + 1, (_current - 1) - (_start + 1)));
        }

        private void AddNumber()
        {
            bool isFloating = false;

            while (IsDigit(Peek()))
                Advance();

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                isFloating = true;

                Advance();
                while (IsDigit(Peek()))
                    Advance();
            }
            AddToken(
                isFloating ?
                    TokenType.FloatingLiteral :
                    TokenType.IntegerLiteral,
                isFloating ?
                    float.Parse(_source.Substring(_start, _current - _start)) :
                    int.Parse(_source.Substring(_start, _current - _start)));
        }

        private void AddIdentifier()
        {
            TokenType type = TokenType.Identifier;

            while (IsAlphaNumeric(Peek()))
                Advance();

            if (_reservedKeyworks.TryGetValue(_source.Substring(_start, _current - _start), out TokenType value))
                type = value;

            AddToken(type);
        }

        private void HandleComments()
        {
            if (Match('/'))
            {
                while (Peek() != '\n' && !IsEnd())
                    Advance();
            }
            else
            {
                AddToken(TokenType.Slash);
            }
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LeftParen); break;
                case ')': AddToken(TokenType.RightParen); break;
                case '{': AddToken(TokenType.LeftBrace); break;
                case '}': AddToken(TokenType.RightBrace); break;
                case '[': AddToken(TokenType.LeftBracket); break;
                case ']': AddToken(TokenType.RightBracket); break;
                case ',': AddToken(TokenType.Comma); break;
                case '.': AddToken(TokenType.Dot); break;
                case '-': AddToken(TokenType.Minus); break;
                case '+': AddToken(TokenType.Plus); break;
                case ';': AddToken(TokenType.Semilcolon); break;
                case '*': AddToken(TokenType.Star); break;
                case '!': AddToken(Match('=') ? TokenType.NotEqual : TokenType.Not); break;
                case '=': AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal); break;
                case '<': AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less); break;
                case '>': AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater); break;
                case '/': HandleComments(); break;
                case '"': AddString(); break;
                case '@':
                    if (Match('[')) AddToken(TokenType.Attribute);
                    else AddIdentifier();
                    break;
                case '\n': _line++; break;
                case ' ':
                case '\r':
                case '\t':
                    break;
                default:
                    if (IsDigit(c)) AddNumber();
                    else if (IsAlpha(c)) AddIdentifier();
                    else AddErrorToken($"Unexpected token {c} on line {_line}, column {_current+1}");
                    break;
            }
        }
    }
}
