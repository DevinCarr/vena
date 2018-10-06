using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Vena.Lexer;

using static Vena.Lexer.TokenType;
using static Vena.AST.Expr;

namespace Vena.AST
{
    public enum VType
    {
        Null,
        Int,
        Double,
        String,
        Bool
    };

    public class Parser
    {
        class ParseError : SystemException { }

        readonly List<Token> tokens;
        int current;

        private static Dictionary<TokenType, VType> keywords = new Dictionary<TokenType, VType>()
        {
            { INTEGER_KEYWORD, VType.Int },
            { DOUBLE_KEYWORD, VType.Double },
            { STRING_KEYWORD, VType.String },
            { BOOL_KEYWORD, VType.Bool }
        };

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
                    statements.Add(Declaration());
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

        bool Check(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (IsAtEnd()) return false;
                if (Peek().Type == type) return true;
            }

            return false;
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

        Stmt Declaration()
        {
            try
            {
                if (Check(INTEGER_KEYWORD, DOUBLE_KEYWORD, STRING_KEYWORD, BOOL_KEYWORD)) return VarDeclaration();
                if (Check(IDENTIFIER)) return AssignStmt();

                return Statement();
            }
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }

        Stmt VarDeclaration()
        {
            Token declType = null;
            if (Check(INTEGER_KEYWORD)) declType = Consume(INTEGER_KEYWORD, "Expected 'int' type.");
            if (Check(DOUBLE_KEYWORD)) declType = Consume(DOUBLE_KEYWORD, "Expected 'double' type.");
            if (Check(STRING_KEYWORD)) declType = Consume(STRING_KEYWORD, "Expected 'string' type.");
            if (Check(BOOL_KEYWORD)) declType = Consume(BOOL_KEYWORD, "Expected 'bool' type.");
            if (declType == null) Error(Peek(), "Unexpected type.");
            VType type = keywords[declType.Type];
            Token name = Consume(IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if (Match(EQUAL))
            {
                initializer = Expression();
            }

            Consume(SEMICOLON, "Expect ';' after variable declaration.");
            return new Var(type, name, initializer);
        }

        Stmt AssignStmt()
        {
            Token identifier = Consume(IDENTIFIER, "Expected variable name.");
            Consume(EQUAL, "Expected '=' after identifier.");
            Expr expr = Expression();

            Consume(SEMICOLON, "Expect ';' after variable assignment.");
            return new Assign(identifier, expr);
        }

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
            Expr expr = Modulus();

            while (Match(SLASH, STAR))
            {
                Token op = Previous();
                Expr right = Modulus();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        Expr Modulus()
        {
            Expr expr = Unary();

            while (Match(PERCENT))
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
            if (Match(FALSE)) return new Literal(false, VType.Bool);
            if (Match(TRUE)) return new Literal(true, VType.Bool);
            if (Match(NIL)) return new Literal(null, VType.Null);

            if (Match(DOUBLE))
            {
                return new Literal(Previous().Literal, VType.Double);
            }

            if (Match(INTEGER))
            {
                return new Literal(Previous().Literal, VType.Int);
            }

            if (Match(STRING))
            {
                return new Literal(Previous().Literal, VType.String);
            }

            if (Match(IDENTIFIER))
            {
                return new Variable(Previous());
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
