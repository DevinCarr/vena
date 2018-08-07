using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Vena.AST;
using Vena.Lexer;

namespace Vena
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: vena [file] [output]");
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
                else if (args[0] == "-parse")
                {
                    string input = File.ReadAllText(args[1]);
                    Scanner scanner = new Scanner(input);
                    var tokens = scanner.ScanTokens();
                    Parser parser = new Parser(tokens);
                    List<Stmt> stmts = parser.Parse();

                    // Stop if there was a syntax error.
                    if (VenaError.HasError) return;

                    Console.WriteLine(new ASTPrinter().Print(stmts));
                }
                else if (args[0] == "-emit")
                {
                    string input = File.ReadAllText(args[1]);

                    Scanner scanner = new Scanner(input);
                    var tokens = scanner.ScanTokens();
                    // Stop if there was a syntax error.
                    if (VenaError.HasError) return;

                    Parser parser = new Parser(tokens);
                    List<Stmt> stmts = parser.Parse();
                    // Stop if there was a parser error.
                    if (VenaError.HasError) return;

                    Console.WriteLine(new Generator().Emit(stmts).ToFullString());
                }
                else
                {
                    string input = File.ReadAllText(args[0]);

                    Scanner scanner = new Scanner(input);
                    var tokens = scanner.ScanTokens();
                    // Stop if there was a syntax error.
                    if (VenaError.HasError) return;

                    Parser parser = new Parser(tokens);
                    List<Stmt> stmts = parser.Parse();
                    // Stop if there was a parser error.
                    if (VenaError.HasError) return;

                    Generator gen = new Generator();
                    var ast = gen.Emit(stmts);
                    var result = gen.Compile(ast, args[1]);
                    if (!result.Success)
                    {
                        foreach (var diag in result.Diagnostics)
                        {
                            Console.Error.WriteLine(diag);
                        }
                    }
                }
            }
        }
    }
}
