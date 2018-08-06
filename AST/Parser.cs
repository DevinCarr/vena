using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Vena.AST;
using Vena.Lexer;

using static Vena.Lexer.TokenType;

namespace Vena
{
    public class Parser
    {
        class ParseError : SystemException { }

        readonly List<Token> tokens;
        int current;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
            current = 0;
        }

        public List<Stmt> Parse()
        {
            try
            {
                List<Stmt> statements = new List<Stmt>();
                while (!IsAtEnd())
                {
                    statements.Add(Statement());
                }

                return statements;
            }
            catch (ParseError)
            {
                return null;
            }
        }

        #region Helpers

        bool IsAtEnd()
        {
            return Peek().Type == EOF;
        }

        Token Peek()
        {
            return tokens.ElementAt(current);
        }

        Token Previous()
        {
            return tokens.ElementAt(current - 1);
        }

        bool Check(TokenType tokenType)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == tokenType;
        }

        Token Advance()
        {
            if (!IsAtEnd()) current++;
            return Previous();
        }

        bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        Token Consume(TokenType type, String message)
        {
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }

        ParseError Error(Token token, String message)
        {
            VenaError.ParseError(token, message);
            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Type == SEMICOLON) return;

                switch (Peek().Type)
                {
                    case CLASS:
                    case FUN:
                    case LET:
                    case FOR:
                    case IF:
                    case WHILE:
                    case PRINT:
                    case RETURN:
                        return;
                }

                Advance();
            }
        }

        #endregion

        Stmt Statement()
        {
            if (Match(PRINT)) return PrintStatement();

            return ExpressionStatement();
        }

        Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(SEMICOLON, "Expect ';' after value.");
            return new Print(value);
        }

        Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(SEMICOLON, "Expect ';' after expression.");
            return new Expression(expr);
        }

        Expr Expression()
        {
            return Equality();
        }

        Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(BANG_EQUAL, EQUAL_EQUAL))
            {
                Token op = Previous();
                Expr right = Comparison();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        Expr Comparison()
        {
            Expr expr = Addition();

            while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
            {
                Token op = Previous();
                Expr right = Addition();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        Expr Addition()
        {
            Expr expr = Multiplication();

            while (Match(MINUS, PLUS))
            {
                Token op = Previous();
                Expr right = Multiplication();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        Expr Multiplication()
        {
            Expr expr = Unary();

            while (Match(SLASH, STAR))
            {
                Token op = Previous();
                Expr right = Unary();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        Expr Unary()
        {
            if (Match(BANG, MINUS))
            {
                Token op = Previous();
                Expr right = Unary();
                return new Unary(op, right);
            }

            return Primary();
        }

        Expr Primary()
        {
            if (Match(FALSE)) return new Literal(false, Expr.VType.Bool);
            if (Match(TRUE)) return new Literal(true, Expr.VType.Bool);
            if (Match(NIL)) return new Literal(null, Expr.VType.Null);

            if (Match(NUMBER))
            {
                return new Literal(Previous().Literal, Expr.VType.Double);
            }

            if (Match(STRING))
            {
                return new Literal(Previous().Literal, Expr.VType.String);
            }

            if (Match(RIGHT_PAREN))
            {
                throw Error(Peek(), "Unexpected ')'.");
            }

            if (Match(LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(RIGHT_PAREN, "Expect ')' after expression.");
                return new Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }
    }
}
