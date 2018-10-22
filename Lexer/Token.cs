using System;
using System.Collections.Generic;
using System.Text;

namespace Vena.Lexer
{
    public class Token
    {
        public TokenType Type { get; private set; }
        public string Lexeme { get; private set; }
        public object Literal { get; private set; }
        public string File { get; private set; }
        public int Line { get; private set; }

        public Token(TokenType type, string lexeme, object literal, string file, int line)
        {
            this.Type = type;
            this.Lexeme = lexeme;
            this.Literal = literal;
            this.File = file;
            this.Line = line;
        }

        public override string ToString()
        {
            return Type + " " + Lexeme + " " + Literal;
        }
    }
}
