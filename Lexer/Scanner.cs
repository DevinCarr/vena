﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vena.Lexer
{
    public enum TokenType
    {
        // Single-character tokens.
        LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
        COMMA, DOT, MINUS, NEW_LINE, PERCENT, PLUS, SEMICOLON,
        SLASH, STAR, 

        // One or two character tokens.
        BANG, BANG_EQUAL,
        EQUAL, EQUAL_EQUAL,
        GREATER, GREATER_EQUAL,
        LESS, LESS_EQUAL,

        // Literals.
        IDENTIFIER, VAR, STRING, INTEGER, DOUBLE,

        // Keywords.
        AND, CLASS, ELSE, FALSE, FUN, FOR, IF, LET, NIL,
        OR, PRINT, RETURN, SUPER, THIS, TRUE, WHILE,
        // Type Keywords.
        INTEGER_KEYWORD, DOUBLE_KEYWORD, STRING_KEYWORD, BOOL_KEYWORD,

        EOF
    };

    public class Scanner
    {
        private readonly string source;
        private readonly string file;
        private List<Token> tokens;
        private readonly int end;
        private int start = 0;
        private int current = 0;
        private int column = 0;
        private int line = 1;

        private static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
        {
            { "and",        TokenType.AND },
            { "bool",       TokenType.BOOL_KEYWORD },
            { "class",      TokenType.CLASS },
            { "double",     TokenType.DOUBLE_KEYWORD },
            { "else",       TokenType.ELSE },
            { "false",      TokenType.FALSE },
            { "for",        TokenType.FOR },
            { "fun",        TokenType.FUN },
            { "if",         TokenType.IF },
            { "int",        TokenType.INTEGER_KEYWORD },
            { "let",        TokenType.LET },
            { "nil",        TokenType.NIL },
            { "or",         TokenType.OR },
            { "print",      TokenType.PRINT },
            { "return",     TokenType.RETURN },
            { "string",     TokenType.STRING_KEYWORD },
            { "super",      TokenType.SUPER },
            { "this",       TokenType.THIS },
            { "true",       TokenType.TRUE },
            { "while",      TokenType.WHILE },
        };

        public Scanner(string source, string file)
        {
            this.source = source;
            this.file = file;
            this.end = source.Length;
            this.tokens = new List<Token>();
        }

        public ref List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, file, line));
            return ref tokens;
        }

        private bool IsAtEnd()
        {
            return current >= end;
        }

        private char Advance()
        {
            current++;
            column++;
            return source.ElementAt(current - 1);
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, Object literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, file, line));
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (source.ElementAt(current) != expected) return false;

            current++;
            column++;
            return true;
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return source.ElementAt(current);
        }

        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source.ElementAt(current + 1);
        }

        private void LexString()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            // Unterminated string.
            if (IsAtEnd())
            {
                VenaError.LexicalError(line, "Unterminated string.");
                return;
            }

            // The closing ".
            Advance();

            // Trim the surrounding quotes.
            String value = source.Substring(start + 1, (current - 1) - (start + 1));
            AddToken(TokenType.STRING, value);
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private void Number()
        {
            while (IsDigit(Peek())) Advance();
            TokenType type = TokenType.INTEGER;

            // Look for a fractional part.
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                // Consume the "."
                Advance();

                while (IsDigit(Peek())) Advance();
                type = TokenType.DOUBLE;
            }

            if (type == TokenType.INTEGER)
            {
                AddToken(TokenType.INTEGER,
                     long.Parse(source.Substring(start, current - start)));
            }
            else
            {
                AddToken(TokenType.DOUBLE,
                     double.Parse(source.Substring(start, current - start)));
            }
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            // See if the identifier is a reserved word.
            string text = source.Substring(start, current - start);

            bool valid = keywords.TryGetValue(text, out TokenType type);
            if (!valid) type = TokenType.IDENTIFIER;
            AddToken(type, text);
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '%': AddToken(TokenType.PERCENT); break;
                case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
                // Comments
                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                // Ignore whitespace.
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    if (column == 2) AddToken(TokenType.NEW_LINE);
                    line++;
                    column = 0;
                    break;
                // Strings
                case '"': LexString(); break;
                default:
                    if (IsDigit(c))
                    {
                        Number();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        VenaError.LexicalError(line, "Unexpected character.");
                    }
                    break;
            }
        }
    }
}
