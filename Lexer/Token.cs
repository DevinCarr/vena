using System;
using System.Collections.Generic;
using System.Text;

namespace Vena.Lexer
{
    public class Token
    {
        public TokenType Type { get; private set; }
        readonly string lexeme;
        public object Literal { get; private set; }
        public int Line { get; private set; }

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            this.Type = type;
            this.lexeme = lexeme;
            this.Literal = literal;
            this.Line = line;
        }

        public override string ToString()
        {
            return Type + " " + lexeme + " " + Literal;
        }
    }
}
