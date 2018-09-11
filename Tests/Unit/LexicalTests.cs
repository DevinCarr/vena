using System;
using Xunit;
using System.Linq;
using Vena.Lexer;

using static Vena.Lexer.TokenType;

namespace Vena.Test
{
    public class LexicalTests
    {
        //[Theory]
        //[InlineData("()", 3, new TokenType[] { LEFT_PAREN, RIGHT_PAREN, EOF })]
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
                EQUAL_EQUAL,
                LEFT_PAREN,
                RIGHT_PAREN,
                LEFT_BRACE,
                RIGHT_BRACE,
                COMMA,
                DOT,
                MINUS,
                PLUS,
                SEMICOLON,
                STAR,
                BANG,
                BANG_EQUAL,
                EQUAL,
                LESS,
                LESS_EQUAL,
                GREATER,
                GREATER_EQUAL,
                EOF
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
            Assert.True(VenaError.HasError);
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
            int lexedCount = tokens.Where(t => t.Type != EOF).Count();
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
            var token = tokens.Where(t => t.Type != EOF).First();
            string actual = token.Literal as string;
            Assert.Equal(STRING, token.Type);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1, "1")]
        [InlineData(2, "2")]
        [InlineData(1234567890, "1234567890")]
        public void NumberTest(long expected, string input)
        {
            var scanner = new Scanner(input);
            var tokens = scanner.ScanTokens();
            // Remove the EOF token
            var token = tokens.Where(t => t.Type != EOF).First();
            long? actual = token.Literal as long?;
            Assert.Equal(INTEGER, token.Type);
            Assert.Equal(expected, actual.Value);
        }

        [Theory]
        [InlineData(1.2, "1.2")]
        [InlineData(1.0, "1.0")]
        [InlineData(1234567890.1234567890, "1234567890.1234567890")]
        public void DoubleTest(double expected, string input)
        {
            var scanner = new Scanner(input);
            var tokens = scanner.ScanTokens();
            // Remove the EOF token
            var token = tokens.Where(t => t.Type != EOF).First();
            double? actual = token.Literal as double?;
            Assert.Equal(DOUBLE, token.Type);
            Assert.Equal(expected, actual.Value);
        }

        [Theory]
        [InlineData("asd")]
        [InlineData("var_iable")]
        [InlineData("verylongidentifierpleasebeokaywiththis_")]
        public void IdentifierTest(string input)
        {
            var scanner = new Scanner(input);
            var tokens = scanner.ScanTokens();
            // Remove the EOF token
            var token = tokens.Where(t => t.Type != EOF).First();
            string actual = token.Literal as string;
            Assert.Equal(IDENTIFIER, token.Type);
            Assert.Equal(input, actual);
        }
    }
}
