using System;
using Xunit;
using System.Linq;
using Vena.Lexer;

namespace Vena.Test
{
    public class LexicalTests
    {
        [Theory]
        [InlineData("()", 3, new TokenType[] { TokenType.LEFT_PAREN, TokenType.RIGHT_PAREN, TokenType.EOF })]
        public void LexicalTest1(string input, int len, TokenType[] expected)
        {
            var scanner = new Scanner(input);
            var tokens = scanner.ScanTokens();
            Assert.Equal(len, tokens.Count);
            var pairs = expected.Zip(tokens, (e, a) => (e, a.Type));
            foreach (var (exp, act) in pairs)
            {
                Assert.Equal(exp, act);
            }
        }

        [Fact]
        public void LexicalTest2()
        {
            var input = "==(){},.-+;*!!==<<=>>=";
            var expected = new TokenType[]
            {
                TokenType.EQUAL_EQUAL,
                TokenType.LEFT_PAREN,
                TokenType.RIGHT_PAREN,
                TokenType.LEFT_BRACE,
                TokenType.RIGHT_BRACE,
                TokenType.COMMA,
                TokenType.DOT,
                TokenType.MINUS,
                TokenType.PLUS,
                TokenType.SEMICOLON,
                TokenType.STAR,
                TokenType.BANG,
                TokenType.BANG_EQUAL,
                TokenType.EQUAL,
                TokenType.LESS,
                TokenType.LESS_EQUAL,
                TokenType.GREATER,
                TokenType.GREATER_EQUAL,
                TokenType.EOF
            };
            var scanner = new Scanner(input);
            var tokens = scanner.ScanTokens();
            var len = expected.Length;
            Assert.Equal(len, tokens.Count);
            var pairs = expected.Zip(tokens, (e, a) => (e, a.Type));
            foreach (var (exp, act) in pairs)
            {
                Assert.Equal(exp, act);
            }
        }

        [Fact]
        public void InvalidLexicalTest()
        {
            var input = "&";
            var scanner = new Scanner(input);
            var tokens = scanner.ScanTokens();
            Assert.True(Error.HasError);
        }
    }
}
