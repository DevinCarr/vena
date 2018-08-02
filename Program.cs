using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Vena.Lexer;

namespace Vena
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: vena [file]");
            }
            else
            {
                if (args[0] == "-lex")
                {
                    string input = File.ReadAllText(args[1]);
                    Scanner scanner = new Scanner(input);
                    var tokens = scanner.ScanTokens();
                    int line = 1;
                    int iterator = 0;
                    int total = tokens.Count();
                    while (true)
                    {
                        Console.Write($"[{tokens.ElementAt(iterator)}] ");
                        if (iterator == total - 1) break;
                        iterator++;
                        if (line < tokens.ElementAt(iterator).Line)
                        {
                            line++;
                            Console.Write('\n');
                        }
                    }
                    Console.Write('\n');
                }
            }
        }
    }
}
