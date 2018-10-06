using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Vena.Lexer;

namespace Vena
{
    public static class VenaError
    {
        public static bool HasError { get; private set; }
        public static void LexicalError(int line, string message)
        {
            Console.Error.WriteLine($"[line {line}] Lexical Error: {message}");
            HasError = true;
        }

        public static void ParseError(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Console.Error.WriteLine($"[line {token.Line} at end] Parse Error: {message}");
            }
            else
            {
                Console.Error.WriteLine($"[line {token.Line} at '{token.Lexeme}'] Parse Error: {message}");
            }
            HasError = true;
        }

        public static void CompileError(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Console.Error.WriteLine($"[line {token.Line} at end] Compile Error: {message}");
            }
            else
            {
                Console.Error.WriteLine($"[line {token.Line} at '{token.Lexeme}'] Compile Error: {message}");
            }
            HasError = true;
        }
    }
}
