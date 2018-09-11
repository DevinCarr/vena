using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Formatting;

using static Vena.Lexer.TokenType;

namespace Vena.AST
{
    public class Generator : Expr.IVisitor<ExpressionSyntax>, Stmt.IVisitor<StatementSyntax>
    {
        public Generator()
        {
        }

        public CompilationUnitSyntax Emit(List<Stmt> stmts)
        {
            var Tvoid = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));

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
            return vena;
        }

        public EmitResult Compile(CompilationUnitSyntax ast, string output, string dotnetcoreRefPath = @"/usr/local/share/dotnet/sdk/NuGetFallbackFolder/microsoft.netcore.app/2.1.1/ref/netcoreapp2.1")
        {
            var references = Directory.GetFiles(dotnetcoreRefPath, "*.dll").Select(file => MetadataReference.CreateFromFile(file));
            var compileOptions = new CSharpCompilationOptions(
                                        outputKind: OutputKind.ConsoleApplication,
                                        optimizationLevel: OptimizationLevel.Debug,
                                        platform: Platform.AnyCpu
                                     );
            var assemblyName = (output.Contains('/') || output.Contains('\\')) ? Path.GetFileNameWithoutExtension(output) : output;
            var result = CSharpCompilation.Create(assemblyName, new SyntaxTree[] { ast.SyntaxTree }, references, compileOptions).Emit(output + ".dll");
            if (result.Success)
            {
                File.WriteAllText(output + ".runtimeconfig.json", @"
{
    ""runtimeOptions"": {
        ""tfm"": ""netcoreapp2.1"",
        ""framework"": {
            ""name"": ""Microsoft.NETCore.App"",
            ""version"": ""2.1.0""
        }
    }
}");
            }
            return result;
        }

        public 

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
                                                         SyntaxFactory.IdentifierName("Write"))
                                    .WithOperatorToken(SyntaxFactory.Token(SyntaxKind.DotToken))
                ).WithArgumentList(
                    SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(stmt.Expr.Accept(this))
                    )).WithOpenParenToken(SyntaxFactory.Token(SyntaxKind.OpenParenToken)).WithCloseParenToken(SyntaxFactory.Token(SyntaxKind.CloseParenToken))
            ));
        }

        public ExpressionSyntax VisitBinaryExpr(Binary expr)
        {
            ExpressionSyntax LeftExpr = expr.Left.Accept(this);
            ExpressionSyntax RightExpr = expr.Right.Accept(this);

            SyntaxKind op = SyntaxKind.NullKeyword;
            switch (expr.Op.Type)
            {
                // Arithmetic Operators
                case MINUS: op = SyntaxKind.SubtractExpression; break;
                case PLUS: op = SyntaxKind.AddExpression; break;
                case STAR: op = SyntaxKind.MultiplyExpression; break;
                case SLASH: op = SyntaxKind.DivideExpression; break;
                case PERCENT: op = SyntaxKind.ModuloExpression; break;
                // Conditional Operators
                case EQUAL_EQUAL: op = SyntaxKind.EqualsExpression; break;
                case BANG_EQUAL: op = SyntaxKind.NotEqualsExpression; break;
                case LESS: op = SyntaxKind.LessThanExpression; break;
                case LESS_EQUAL: op = SyntaxKind.LessThanOrEqualExpression; break;
                case GREATER: op = SyntaxKind.GreaterThanExpression; break;
                case GREATER_EQUAL: op = SyntaxKind.GreaterThanOrEqualExpression; break;
            }

            return SyntaxFactory.BinaryExpression(op, LeftExpr, RightExpr);
        }

        public ExpressionSyntax VisitGroupingExpr(Grouping expr)
        {
            return SyntaxFactory.ParenthesizedExpression(expr.Expr.Accept(this));
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
            ExpressionSyntax RightExpr = expr.Right.Accept(this);

            switch (expr.Op.Type)
            {
                case MINUS: 
                    return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.UnaryMinusExpression, RightExpr);
                case BANG:
                    return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.ExclamationToken, RightExpr);
            }

            // Invalid Unary operator
            return null;
        }
    }
}
