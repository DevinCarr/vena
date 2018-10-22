using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Vena.AST
{
    public class ASTPrinter : Expr.IVisitor<string>, Stmt.IVisitor<string>
    {
        public ASTPrinter()
        {
        }

        public string Print(List<Stmt> stmts)
        {
            StringBuilder output = new StringBuilder();
            foreach (var stmt in stmts)
            {
                output.Append("[").Append("Stmt ");
                output.Append(stmt.Accept(this));
                output.Append("]");
            }
            return output.ToString();
        }

        String Parenthesize(String name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expr expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }

        public string VisitExpressionStmt(Expression stmt)
        {
            return stmt.Expr.Accept(this);
        }

        public string VisitPrintStmt(Print stmt)
        {
            return Parenthesize("print", stmt.Expr);
        }

        public string VisitVarStmt(Var stmt)
        {
            return Parenthesize("var", stmt.Initializer);
        }

        public string VisitAssignStmt(Assign stmt)
        {
            return Parenthesize("assign", stmt.Expr);
        }

        public string VisitBinaryExpr(Binary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        }

        public string VisitGroupingExpr(Grouping expr)
        {
            return Parenthesize("group", expr.Expr);
        }

        public string VisitLiteralExpr(Literal expr)
        {
            if (expr.Value == null) return "nil";
            return expr.Value.ToString();
        }

        public string VisitUnaryExpr(Unary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Right);
        }

        public string VisitVariableExpr(Variable expr)
        {
            return expr.Name.Lexeme;
        }

        public string VisitNewLineStmt(NewLine stmt)
        {
            return "\n";
        }
    }
}
