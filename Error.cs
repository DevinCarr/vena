using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Vena.Lexer;

namespace Vena
{
    public static class Error
    {
        public static bool HasError { get; private set; }
        public static void LexicalError(int line, string message)
        {
            Console.Error.WriteLine($"[line {line}] Lexical Error: {message}");
            HasError = true;
        }
    }
}
