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
}
