module Vena.AST
open System
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax

type Program = Decl list

and Decl =
    | StaticVarDecl of VarDecl

and VarDecl =
    | ScalarVarDecl of Identifier * TypeSpec
    | ScalarVarDeclInit of Identifier * Expression
    member x.Emit(): LocalDeclarationStatementSyntax =
        match x with
        | ScalarVarDecl(i,t) ->  SyntaxFactory.LocalDeclarationStatement(
                                    SyntaxFactory.VariableDeclaration(
                                        t.Emit(), SyntaxFactory.SeparatedList<Syntax.VariableDeclaratorSyntax>(
                                            [|SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(i))|]
                                        )
                                    )
                                )
        | ScalarVarDeclInit(i,e) ->  SyntaxFactory.LocalDeclarationStatement(
                                        SyntaxFactory.VariableDeclaration(
                                            e.ToTypeSpec().Emit(), SyntaxFactory.SeparatedList<Syntax.VariableDeclaratorSyntax>(
                                                    [|SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(i), null, SyntaxFactory.EqualsValueClause((e.Emit() |> fun (e,t) -> e)))|]
                                                )
                                            )
                                    )

and Expression =
    | BinaryExpression of (Expression * TypeSpec) * BinaryOperator * (Expression * TypeSpec)
    | LiteralExpression of Literal
    member x.Emit(): ExpressionSyntax * TypeSpec =
        match x with
        | BinaryExpression((le,lt),op,(re,rt)) -> SyntaxFactory.BinaryExpression(
                                                    op.Emit(),
                                                    (le.Emit() |> fun (e,t) -> e),
                                                    (re.Emit() |> fun (e,t) -> e)
                                                    ) :> ExpressionSyntax, lt.ImplicitCast(rt)
        | LiteralExpression(l) -> l.Emit() :> ExpressionSyntax, l.ToTypeSpec()
    member x.ToTypeSpec(): TypeSpec =
        match x with
        | BinaryExpression((le,lt),op,(re,rt)) -> lt.ImplicitCast(rt)
        | LiteralExpression(l) -> l.ToTypeSpec()

and BinaryOperator =
    | ConditionalOr
    | Equal
    | NotEqual
    | LessEqual
    | Less
    | GreaterEqual
    | Greater
    | ConditionalAnd
    | Multiply
    | Divide
    | Add
    | Subtract
    | Modulus
    override x.ToString() =
        match x with
        | ConditionalOr  -> "||"
        | Equal          -> "=="
        | NotEqual       -> "!="
        | LessEqual      -> "<="
        | Less           -> "<"
        | GreaterEqual   -> ">="
        | Greater        -> ">"
        | ConditionalAnd -> "&&"
        | Multiply       -> "*"
        | Divide         -> "/"
        | Add            -> "+"
        | Subtract       -> "-"
        | Modulus        -> "%"
    member x.Emit() =
        match x with
        | ConditionalOr  -> SyntaxKind.LogicalOrExpression
        | Equal          -> SyntaxKind.EqualsExpression
        | NotEqual       -> SyntaxKind.NotEqualsExpression
        | LessEqual      -> SyntaxKind.LessThanOrEqualExpression
        | Less           -> SyntaxKind.LessThanExpression
        | GreaterEqual   -> SyntaxKind.GreaterThanOrEqualExpression
        | Greater        -> SyntaxKind.GreaterThanExpression
        | ConditionalAnd -> SyntaxKind.LogicalAndExpression
        | Multiply       -> SyntaxKind.MultiplyExpression
        | Divide         -> SyntaxKind.DivideExpression
        | Add            -> SyntaxKind.AddExpression
        | Subtract       -> SyntaxKind.SubtractExpression
        | Modulus        -> SyntaxKind.ModuloExpression

and TypeSpec = 
    | TDouble
    | TInt
    | TBool
    | TString
    override x.ToString() =
        match x with
        | TDouble -> "double"
        | TInt    -> "int"
        | TBool   -> "bool"
        | TString -> "string"
    member x.Emit() =
        match x with
        | TDouble -> SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword))
        | TInt    -> SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword))
        | TBool   -> SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword))
        | TString -> SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))
    member x.ImplicitCast(y) = 
        let err left right = Error(sprintf "Invalid implicit cast: %A and %A" left right)
        match x with
        | TDouble -> match y with
                     | TDouble -> Ok(TDouble)
                     | TInt    -> Ok(TDouble)
                     | TBool   -> err x y
                     | TString -> err x y
        | TInt   -> match y with
                     | TDouble -> Ok(TDouble)
                     | TInt    -> Ok(TDouble)
                     | TBool   -> err x y
                     | TString -> err x y
        | TBool   -> match y with
                     | TDouble -> err x y
                     | TInt    -> err x y
                     | TBool   -> Ok(TBool)
                     | TString -> err x y
        | TString   -> match y with
                     | TDouble -> err x y
                     | TInt    -> err x y
                     | TBool   -> err x y
                     | TString -> Ok(TString)

and Literal =
    | LDouble of double
    | LInt of int64
    | LBool of bool
    | LString of string
    override x.ToString() =
        match x with
        | LDouble(f) -> f.ToString()
        | LInt(i)    -> i.ToString()
        | LBool(b)   -> b.ToString()
        | LString(s) -> s.ToString()
    member x.ToTypeSpec() =
        match x with
        | LDouble(f) -> TDouble
        | LInt(i)    -> TInt
        | LBool(b)   -> TBool
        | LString(s) -> TString
    member x.Emit() =
        match x with
        | LDouble(f) -> SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(f))
        | LInt(i)    -> SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i))
        | LBool(b)   -> (if b then (SyntaxKind.TrueLiteralExpression, SyntaxFactory.Token(SyntaxKind.TrueKeyword)) else (SyntaxKind.FalseLiteralExpression, SyntaxFactory.Token(SyntaxKind.FalseKeyword))) |> SyntaxFactory.LiteralExpression
        | LString(s) -> SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(s))

and Identifier = string