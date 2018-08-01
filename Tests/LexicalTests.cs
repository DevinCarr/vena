using System;
using Xunit;
using System.Linq;
using Vena.Lexer;

namespace Vena.Test
{
    public class LexicalTests
    {
        //[Theory]
        //[InlineData("()", 3, new TokenType[] { TokenType.LEFT_PAREN, TokenType.RIGHT_PAREN, TokenType.EOF })]
        //public void LexicalTest1(string input, int len, TokenType[] expected)
        //{
        //    var scanner = new Scanner(input);
        //    var tokens = scanner.ScanTokens();
        //    Assert.Equal(len, tokens.Count);
        //    var pairs = expected.Zip(tokens, (e, a) => (e, a.Type));
        //    foreach (var (exp, act) in pairs)
        //    {
        //        Assert.Equal(exp, act);
        //    }
        //}

        [Fact]
        public void LexicalTokensTest()
        {
            var input = "== () {\n},.\r-+;\t*!!==<<=>>=";
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
        public void InvalidCharacterTest()
        {
            var input = "&";
            var scanner = new Scanner(input);
            var tokens = scanner.ScanTokens();
            Assert.True(Error.HasError);
        }

        [Theory]
        [InlineData(0, "//asd")]
        [InlineData(0, @"// asd
// comment 2")]
        [InlineData(1, ">//asd")]
        public void CommentTest(int tokenCount, string input)
        {
            var scanner = new Scanner(input);
            var tokens = scanner.ScanTokens();
            // Remove the EOF token
            int lexedCount = tokens.Where(t => t.Type != TokenType.EOF).Count();
            Assert.Equal(tokenCount, lexedCount);
        }

        [Theory]
        [InlineData("a", "\"a\"")]
        [InlineData("abasdbasdbasd", "\"abasdbasdbasd\"")]
        [InlineData("aba \rsdb as\ndb as//d", "\"aba \rsdb as\ndb as//d\"")]
        public void StringTest(string expected, string input)
        {
            var scanner = new Scanner(input);
            var tokens = scanner.ScanTokens();
            // Remove the EOF token
            var token = tokens.Where(t => t.Type != TokenType.EOF).First();
            string actual = token.Literal as string;
            Assert.Equal(TokenType.STRING, token.Type);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1.2, "1.2")]
        [InlineData(1.0, "1.0")]
        [InlineData(1234567890.1234567890,"1234567890.1234567890")]
        public void NumberTest(double expected, string input)
        {
            var scanner = new Scanner(input);
            var tokens = scanner.ScanTokens();
            // Remove the EOF token
            var token = tokens.Where(t => t.Type != TokenType.EOF).First();
            double? actual = token.Literal as double?;
            Assert.Equal(TokenType.NUMBER, token.Type);
            Assert.Equal(expected, actual.Value);
        }
    }
}
