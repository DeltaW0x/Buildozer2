using System.Collections.Frozen;
using static System.Net.Mime.MediaTypeNames;

namespace Buildozer.ShaderCompiler
{
    public class Scanner
    {
        private readonly string _source;
        private readonly List<Token> _tokens = new();
        private readonly FrozenDictionary<string, TokenType> _reservedKeywords;
        
        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        public Scanner(string source)
        {
            _source = source;

            _reservedKeywords = new Dictionary<string, TokenType>() 
            {
                {"if",      TokenType.If },
                {"else",    TokenType.Else },
                {"true",    TokenType.True },
                {"false",   TokenType.False },
                {"for",     TokenType.For },
                {"while",   TokenType.While },
                {"fn",      TokenType.Fn},
                {"var",     TokenType.Var },
                {"return",  TokenType.Return }
            }.ToFrozenDictionary();
        }

        public Token[] ScanTokens()
        {
            while (!IsEnd())
            {
                _start = _current;
                ScanToken();
            }
            _tokens.Add(new Token(TokenType.Eof, "", null, _line));
            return _tokens.ToArray();
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
                case ',': AddToken(TokenType.Comma); break;
                case '.': AddToken(TokenType.Dot); break;
                case '-': AddToken(TokenType.Minus); break;
                case '+': AddToken(TokenType.Plus); break;
                case ';': AddToken(TokenType.Semicolon); break;
                case '*': AddToken(TokenType.Star); break;
                case '!':
                    AddToken(Match('=') ? TokenType.NotEqual: TokenType.LogicalNot);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !IsEnd()) 
                            Advance();
                    }
                    else
                    {
                        AddToken(TokenType.Slash);
                    }
                    break;
                case '@':
                    AddIntrinsic();
                    break;
                case '"':
                    AddString();
                    break;
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    _line++;
                    break;
                default:
                    if (IsDigit(c))
                    {
                        AddNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        AddIdentifier();
                    }
                    else
                    {
                        AddErrorToken("Unexpected character");
                    }
                    break;
            }
        }

        private void AddErrorToken(string message)
        {
            _tokens.Add(new Token(TokenType.Error, _source.Substring(_start, _current - _start), message, _line));
        }

        private void AddIntrinsic()
        {
            if (!IsAlpha(Peek()))
            {
                AddErrorToken("Invalid intrinsic");
                return;
            }
            
            Advance();

            while(IsAlphaNumeric(Peek()) && !IsEnd())
                Advance();

            if (IsEnd())
            {
                AddErrorToken("Unterminated intrinsic");
                return;
            }

            AddToken(TokenType.Intrinsic, _source.Substring(_start, _current - _start));
        }

        private void AddString()
        {
            while (Peek() != '"' && !IsEnd())
            {
                if (Peek() == '\n') 
                    _line++;
                Advance();
            }

            if (IsEnd())
            {
                AddErrorToken("Unterminated string");
                return;
            }

            Advance();
            AddToken(TokenType.String, _source.Substring(_start + 1, (_current - 1) - (_start + 1)));
        }

        private void AddNumber()
        {
            TokenType type = TokenType.Integer;

            while (IsDigit(Peek()))
                Advance();

            if(Peek() == '.' && IsDigit(PeekNext()))
            {
                type = TokenType.Float;
               
                Advance();

                while (IsDigit(Peek()))
                    Advance();
            }
            
            string subStr = _source.Substring(_start, _current - _start);
            AddToken(type, type == TokenType.Integer ? UInt64.Parse(subStr) : double.Parse(subStr));
        }

        private void AddIdentifier()
        {
            while (IsAlphaNumeric(Peek())) 
                Advance();

            string text = _source.Substring(_start, _current - _start);
            AddToken(_reservedKeywords.TryGetValue(text, out TokenType type) ? type : TokenType.Identifier);
        }

        private bool IsEnd()
        {
            return _current >= _source.Length;
        }

        private char Advance()
        {
            return _source[_current++];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object? literal)
        {
            _tokens.Add(new Token(type, _source.Substring(_start, _current - _start), literal, _line));
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
            if (_current +1 >= _source.Length)
                return '\0';

            return _source[_current+1];
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
    }
}
