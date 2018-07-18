open System
open System.IO
open Vena.AST
open Vena.Parser
open Vena.CodeGeneration
open Vena.CodeGenerationTest
open FParsec
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis

let failTest parse expect = printfn "Failure(Test): \n\texpected %A\n\tactual: %A" expect parse

let equal parse expect = 
    match (parse = expect) with
    | true -> ()
    | false -> failTest expect parse

let equalList parse expect = 
    if (List.length parse = List.length expect) then
        ignore (List.zip parse expect |> List.map equal)
    else failTest parse expect

let test p str exp = 
    match run p str with
    | Success(result, _, _)   -> equal result exp
    | Failure(errorMsg, _, _) -> printfn "Failure(Parse): %s" errorMsg

let testp p str exp = 
    match run p str with
    | Success(result, _, _)   -> equalList result exp
    | Failure(errorMsg, _, _) -> printfn "Failure(Parse): %s" errorMsg

let show p str _ = 
    match run p str with
    | Success(result, _, _)   -> printfn "Input: %A\nResult: %A" str result
    | Failure(errorMsg, _, _) -> printfn "Failure(Parse): %s" errorMsg

[<EntryPoint>]
let main argv =
    test pLiteral "1" (LInt 1L)
    test pLiteral "1.2" (LDouble 1.2)
    test pLiteral "true" (LBool true)
    test pLiteral "\"\"" (LString "")
    test pLiteral "\"asd\"" (LString "asd")
    test pIdentifier "a_" "a_"
    test pIdentifier "_a" "_a"
    test pIdentifier "AZ" "AZ"
    test pIdentifier "A:Z" "A"
//    test pExpr "1.0 + 3.0" (BinaryExpression ((LiteralExpression(LDouble 1.0)), Add, (LiteralExpression(LDouble 3.0))))
//    test pExpr "1.0 + 3.0 * 3.0" (BinaryExpression (
//                                    (LiteralExpression(LDouble 1.0)), Add, 
//                                        BinaryExpression (
//                                            (LiteralExpression(LDouble 3.0)), Multiply, LiteralExpression(LDouble 3.0))))
//    test pExpr "1+1.0*4.0*5.0/4.0%2.0" (BinaryExpression
//                                          (LiteralExpression (LInt 1L),Add,
//                                           BinaryExpression
//                                             (BinaryExpression
//                                                (BinaryExpression
//                                                   (BinaryExpression
//                                                      (LiteralExpression (LDouble 1.0),Multiply,
//                                                       LiteralExpression (LDouble 4.0)),Multiply,
//                                                    LiteralExpression (LDouble 5.0)),Divide,
//                                                 LiteralExpression (LDouble 4.0)),Modulus,LiteralExpression (LDouble 2.0))))
//    test pDecl "let a: int \n" (ScalarVarDecl ("a", TInt))
//    test pDecl "let b: double;\t\n" (ScalarVarDecl ("b", TDouble))
//    test pDecl "let c = 1.3\n" (ScalarVarDeclInit ("c", LiteralExpression(LDouble 1.3)))
//    test pDecl "let de = \"asd\";" (ScalarVarDeclInit ("de", LiteralExpression(LString "asd")))
//    test pDecl "let _ = \"asd\";" (ScalarVarDeclInit ("_", LiteralExpression(LString "asd")))
//    test pDecl "let _a_c_ = true;" (ScalarVarDeclInit ("_a_c_", LiteralExpression(LBool true)))
//    let r = [ (ScalarVarDecl ("a", TInt)); (ScalarVarDeclInit ("de", LiteralExpression(LDouble 0.1))) ]
//    testp pProgram "let a: int;let de = 0.1;" r
//    testp pProgram @"let a: int
//let de = 0.1;" r
//    show pProgram "let a = 0.1*8" [(ScalarVarDeclInit("a", BinaryExpression(LiteralExpression(LDouble 0.1), Multiply, LiteralExpression(LInt 8L)))) ]
//    let testexpr = [| SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("Ident"), null, SyntaxFactory.EqualsValueClause((LInt 64L).Emit())) |] |> Array.toSeq
//    let testtest = SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)), SyntaxFactory.SeparatedList<Syntax.VariableDeclaratorSyntax>(testexpr)))
//    printfn "%A" (testtest.ToFullString())
    let (testexpr, testtype) = emit (BinaryExpression(LiteralExpression(LDouble 0.1), Multiply, LiteralExpression(LInt 8L)))
    codegentest testexpr
    let compile = codegen testexpr "vena_codegen"
    let errors = 
        match (compile.Success) with
        | true -> printf "Compiled"
        | false -> printfn "Errors: %A" compile.Diagnostics
    0 // return an integer exit code
