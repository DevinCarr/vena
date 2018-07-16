module Vena.CodeGenerationTest
open System
open System.Text
open System.Collections.Generic
open System.Reflection
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.Formatting
open System.IO

let Tvoid = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword))
let call expr = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                             SyntaxFactory.IdentifierName("Console"),
                                                             SyntaxFactory.IdentifierName("WriteLine"))
                                        .WithOperatorToken(SyntaxFactory.Token(SyntaxKind.DotToken))
                    ).WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList<Syntax.ArgumentSyntax>(
                                SyntaxFactory.Argument(expr)
                            )).WithOpenParenToken(SyntaxFactory.Token(SyntaxKind.OpenParenToken)).WithCloseParenToken(SyntaxFactory.Token(SyntaxKind.CloseParenToken)))
                    )

let body expr = new SyntaxList<Syntax.StatementSyntax>(call expr)

let vena expr = SyntaxFactory.CompilationUnit().AddMembers(
                    SyntaxFactory.ClassDeclaration("Vena")
                        .AddMembers(
                            SyntaxFactory.MethodDeclaration(Tvoid, "Main")
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                                .WithBody(
                                    SyntaxFactory.Block(body expr)
                                )
                        ).AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    ).AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")))
                    //.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName("System"),SyntaxFactory.IdentifierName("IO"))))
                    .WithAdditionalAnnotations(Formatter.Annotation).NormalizeWhitespace()


//let runtimeFormat (s:string) = MetadataReference.CreateFromFile(String.Format(@"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\microsoft.netcore.app\2.1.0\ref\netcoreapp2.1\{0}.dll", s))
let metaref file = MetadataReference.CreateFromFile(file) :> MetadataReference
let referencesCore = Directory.GetFiles(@"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\microsoft.netcore.app\2.1.0\ref\netcoreapp2.1", "*.dll") |> Array.map metaref |> Array.toSeq
let runtimeFormat (s:string) = MetadataReference.CreateFromFile(String.Format(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\{0}.dll", s))
let references = new List<MetadataReference>()
let refs = [
    runtimeFormat "mscorlib"
]

refs |> List.map references.Add |> ignore
    
let loc = typeof<Object>.Assembly.Location
let options = new CSharpCompilationOptions(outputKind = OutputKind.ConsoleApplication, optimizationLevel = OptimizationLevel.Release, platform = Platform.AnyCpu)
let codegen expr file = CSharpCompilation.Create(file, [ (vena expr).SyntaxTree ], referencesCore, options).Emit(file + ".dll")

let codegentest expr = printfn "%A" ((vena expr).ToFullString())