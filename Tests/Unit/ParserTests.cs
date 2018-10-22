using System;
using Xunit;
using System.Linq;
using Vena.AST;
using Vena.Lexer;
using System.Collections.Generic;

namespace Vena.Test
{
    public class ParserTests
    {
        [Theory]
        [InlineData("1 + 2 - (-3) * 9 / (5.6 + 8.0);",
                    "[Stmt (- (+ 1 2) (/ (* (group (- 3)) 9) (group (+ 5.6 8))))]")]
        [InlineData("9 * 9 * 9 * 9 * 9 * 9;",
                    "[Stmt (* (* (* (* (* 9 9) 9) 9) 9) 9)]")]
        [InlineData("((((1 + 2)))) + 1;",
                    "[Stmt (+ (group (group (group (group (+ 1 2))))) 1)]")]
        public void ExpressionTest(string input, string expected)
        {
            Scanner scanner = new Scanner(input, "file.vena");
            var tokens = scanner.ScanTokens();
            Parser parser = new Parser(tokens);
            List<Stmt> stmts = parser.Parse();
            Assert.Equal(expected, new ASTPrinter().Print(stmts));
        }

        [Theory]
        [InlineData("1 + 1; 3 * 4;",
                    "[Stmt (+ 1 1)][Stmt (* 3 4)]")]
        [InlineData("1;\n\t2;",
                    "[Stmt 1][Stmt 2]")]
        public void StatementTest(string input, string expected)
        {
            Scanner scanner = new Scanner(input, "file.vena");
            var tokens = scanner.ScanTokens();
            Parser parser = new Parser(tokens);
            List<Stmt> stmts = parser.Parse();
            Assert.Equal(expected, new ASTPrinter().Print(stmts));
        }
    }
}