using System;
using System.Collections.Generic;
using System.Text;
using Vena.Lexer;

namespace Vena.AST
{
    public abstract class Expr
    {
        public interface IVisitor<R>
        {
            R VisitBinaryExpr(Binary expr);
            R VisitGroupingExpr(Grouping expr);
            R VisitLiteralExpr(Literal expr);
            R VisitUnaryExpr(Unary expr);
            R VisitVariableExpr(Variable expr);
        }

        public abstract R Accept<R>(IVisitor<R> visitor);
    }

    public class Binary : Expr
    {
        public readonly Expr Left;
        public readonly Token Op;
        public readonly Expr Right;

        public Binary(Expr left, Token op, Expr right)
        {
            this.Left = left;
            this.Op = op;
            this.Right = right;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitBinaryExpr(this);
        }
    }

    public class Grouping : Expr
    {
        public readonly Expr Expr;

        public Grouping(Expr expr)
        {
            this.Expr = expr;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitGroupingExpr(this);
        }
    }

    public class Literal : Expr
    {
        public readonly object Value;
        public readonly VType Type;

        public Literal(object value, VType type)
        {
            this.Value = value;
            this.Type = type;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitLiteralExpr(this);
        }

        public object GetNullValue() { return Value; }
        public Int64? GetIntValue() { return Value as Int64?; }
        public Double? GetDoubleValue() { return Value as Double?; }
        public String GetStringValue() { return Value as String; }
        public Boolean? GetBoolValue() { return Value as Boolean?; }
    }

    public class Unary : Expr
    {
        public readonly Token Op;
        public readonly Expr Right;

        public Unary(Token op, Expr right)
        {
            this.Op = op;
            this.Right = right;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitUnaryExpr(this);
        }
    }

    public class Variable : Expr
    {
        public readonly Token Name;

        public Variable(Token name)
        {
            this.Name = name;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitVariableExpr(this);
        }
    }
}
