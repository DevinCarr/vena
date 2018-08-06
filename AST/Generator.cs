using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Vena.AST
{
    public class Generator : Expr.IVisitor<ExpressionSyntax>, Stmt.IVisitor<StatementSyntax>
    {
        public Generator()
        {
        }

        public string Emit(List<Stmt> stmts)
        {
            var Tvoid = SyntaxFactory.ParseTypeName("void");

            // Create the body of the main for now.
            var stmtBlock = new List<StatementSyntax>();
            foreach (var stmt in stmts)
            {
                var s = stmt.Accept(this);
                stmtBlock.Add(s);
            }
            var body = SyntaxFactory.Block(new SyntaxList<StatementSyntax>(stmtBlock));
            // Vena main compilation unit
            var vena = SyntaxFactory.CompilationUnit().AddMembers(
            SyntaxFactory.ClassDeclaration("VenaClass")
            .AddMembers(
                SyntaxFactory.MethodDeclaration(Tvoid, "Main")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                    .WithBody(body)
            )
            ).AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")))
             .WithAdditionalAnnotations(Formatter.Annotation).NormalizeWhitespace();
            return vena.ToFullString();
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

        public StatementSyntax VisitExpressionStmt(Expression stmt)
        {
            return SyntaxFactory.ExpressionStatement(stmt.Expr.Accept(this));
        }

        public StatementSyntax VisitPrintStmt(Print stmt)
        {
            return SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                         SyntaxFactory.IdentifierName("Console"),
                                                         SyntaxFactory.IdentifierName("WriteLine"))
                                    .WithOperatorToken(SyntaxFactory.Token(SyntaxKind.DotToken))
                ).WithArgumentList(
                    SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(stmt.Expr.Accept(this))
                    )).WithOpenParenToken(SyntaxFactory.Token(SyntaxKind.OpenParenToken)).WithCloseParenToken(SyntaxFactory.Token(SyntaxKind.CloseParenToken))
            ));
        }

        public ExpressionSyntax VisitBinaryExpr(Binary expr)
        {
            throw new NotImplementedException();
        }

        public ExpressionSyntax VisitGroupingExpr(Grouping expr)
        {
            throw new NotImplementedException();
        }

        public ExpressionSyntax VisitLiteralExpr(Literal expr)
        {
            if (expr.Value == null) return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression, SyntaxFactory.Token(SyntaxKind.NullKeyword));
            SyntaxKind kind = SyntaxKind.NumericLiteralExpression;
            SyntaxToken value = SyntaxFactory.Token(SyntaxKind.NullKeyword);
            switch (expr.Type)
            {
                case Expr.VType.Null: break;
                case Expr.VType.Int:
                    kind = SyntaxKind.NumericLiteralExpression;
                    value = SyntaxFactory.Literal(expr.GetIntValue().Value);
                    break;
                case Expr.VType.Double:
                    kind = SyntaxKind.NumericLiteralExpression;
                    value = SyntaxFactory.Literal(expr.GetDoubleValue().Value);
                    break;
                case Expr.VType.String:
                    kind = SyntaxKind.StringLiteralExpression;
                    value = SyntaxFactory.Literal(expr.GetStringValue());
                    break;
                case Expr.VType.Bool:
                    var v = expr.GetBoolValue().Value;
                    if (v)
                    {
                        kind = SyntaxKind.TrueLiteralExpression;
                        value = SyntaxFactory.Token(SyntaxKind.TrueKeyword);
                    }
                    else 
                    {
                        kind = SyntaxKind.FalseLiteralExpression;
                        value = SyntaxFactory.Token(SyntaxKind.FalseKeyword);
                    }
                    break;
            }
            return SyntaxFactory.LiteralExpression(kind, value);
        }

        public ExpressionSyntax VisitUnaryExpr(Unary expr)
        {
            throw new NotImplementedException();
        }
    }
}
