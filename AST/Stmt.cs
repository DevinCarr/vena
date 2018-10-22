using System;
using System.Collections.Generic;
using System.Text;
using Vena.Lexer;

namespace Vena.AST
{
    public abstract class Stmt
    {
        public interface IVisitor<R>
        {
            R VisitExpressionStmt(Expression stmt);
            R VisitPrintStmt(Print stmt);
            R VisitVarStmt(Var stmt);
            R VisitAssignStmt(Assign stmt);
            R VisitNewLineStmt(NewLine stmt);
        }

        public abstract R Accept<R>(IVisitor<R> visitor);
    }

    public class Expression : Stmt
    {
        public readonly Expr Expr;
        public Expression(Expr expr)
        {
            this.Expr = expr;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitExpressionStmt(this);
        }
    }

    public class Print : Stmt
    {
        public readonly Expr Expr;
        public Print(Expr expr)
        {
            this.Expr = expr;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitPrintStmt(this);
        }
    }

    public class Var : Stmt
    {
        public readonly VType Type;
        public readonly Token Name;
        public readonly Expr Initializer;
        public Var(VType type, Token name, Expr initializer)
        {
            this.Type = type;
            this.Name = name;
            this.Initializer = initializer;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitVarStmt(this);
        }
    }

    public class Assign : Stmt
    {
        public readonly Token Name;
        public readonly Expr Expr;
        public Assign(Token name, Expr expr)
        {
            this.Name = name;
            this.Expr = expr;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitAssignStmt(this);
        }
    }

    public class NewLine : Stmt
    {
        public readonly Token Token;
        public NewLine(Token token)
        {
            this.Token = token;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitNewLineStmt(this);
        }
    }
}
