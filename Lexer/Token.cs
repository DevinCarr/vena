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
        readonly int line;

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            this.Type = type;
            this.lexeme = lexeme;
            this.Literal = literal;
            this.line = line;
        }

        public override string ToString()
        {
            return Type + " " + lexeme + " " + Literal;
        }
    }
}
