module Vena.AST
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax

type Program = Decl list

and Decl =
    | StaticVarDecl of VarDecl

and VarDecl =
    | ScalarVarDecl of Identifier * TypeSpec
    | ScalarVarDeclInit of Identifier * Expression

and Expression =
    | BinaryExpression of Expression * BinaryOperator * Expression
    | LiteralExpression of Literal
    member x.Emit(): ExpressionSyntax =
        match x with
        | BinaryExpression(le,op,re) -> SyntaxFactory.BinaryExpression(op.Emit(),le.Emit(), re.Emit()) :> ExpressionSyntax
        | LiteralExpression(l) -> l.Emit() :> ExpressionSyntax

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
        | TInt -> "int"
        | TBool -> "bool"
        | TString -> "string"
    member x.Emit() =
        match x with
        | TDouble -> SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword))
        | TInt -> SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword))
        | TBool -> SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword))
        | TString -> SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))

and Literal =
    | LDouble of double
    | LInt of int64
    | LBool of bool
    | LString of string
    override x.ToString() =
        match x with
        | LDouble(f)  -> f.ToString()
        | LInt(i)    -> i.ToString()
        | LBool(b)   -> b.ToString()
        | LString(s) -> s.ToString()
    member x.ToTypeSpec() =
        match x with
        | LDouble(f)  -> TDouble
        | LInt(i)    -> TInt
        | LBool(b)   -> TBool
        | LString(s) -> TString
    member x.Emit() =
        match x with
        | LDouble(f) -> SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(f))
        | LInt(i) ->SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i))
        | LBool(b) -> (if b then (SyntaxKind.TrueLiteralExpression, SyntaxFactory.Token(SyntaxKind.TrueKeyword)) else (SyntaxKind.FalseLiteralExpression, SyntaxFactory.Token(SyntaxKind.FalseKeyword))) |> SyntaxFactory.LiteralExpression
        | LString(s) -> SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(s))

and Identifier = string