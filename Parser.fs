module Vena.Parser
open System
open FParsec
open Vena.AST

type UserState = unit // doesn't have to be unit, of course
type Parser<'t> = Parser<'t, UserState>

// Common Helpers
let ws = skipAnyOf " \t"
let str_ws s = pstring s .>> ws
let ws_str s = ws >>. pstring s
let str s = pstring s
let term = (pchar ';' |>> ignore) <|> skipRestOfLine true
//let betweenParens p = str_ws "(" >>. sepBy (p .>> ws) (str_ws ",") .>> str_ws ")"

// Literals

// Make sure to check if the numeric value is float or int64
let pnum: Parser<Literal,UserState> =
            numberLiteral (    NumberLiteralOptions.AllowHexadecimal
                           ||| NumberLiteralOptions.AllowFraction
                           ||| NumberLiteralOptions.AllowExponent) "number"
            >>= fun x -> 
                if x.IsInteger then preturn (LInt(int64 x.String))
                else preturn (LDouble(double x.String))

let boolean =     ((stringReturn "true"  true)  |>> LBool)
              <|> ((stringReturn "false" false) |>> LBool)

let mpstring = pchar '"' >>. manyCharsTill anyChar (pchar '"') |>> LString 

let pLiteral =     pnum
               <|> boolean
               <|> mpstring

let pType =     (pstring (TDouble.ToString()) >>. preturn TDouble)
            <|> (pstring (TInt.ToString()) >>. preturn TInt)
            <|> (pstring (TBool.ToString()) >>. preturn TBool)
            <|> (pstring (TString.ToString()) >>. preturn TString)

// Expressions
let iop = new OperatorPrecedenceParser<Expression,_,unit>()
iop.TermParser <- pLiteral .>> spaces |>> LiteralExpression
let opstr_ws = spaces
let addop op prec assoc = 
    let newop = InfixOperator(op.ToString(), opstr_ws, prec, assoc, fun l r -> BinaryExpression(l, op, r))
    iop.AddOperator(newop)

// Operator precendence follows same as C++
// http://en.cppreference.com/w/cpp/language/operator_precedence
// 17 - p from the chart
//    Binary           Prec Assoc
addop Multiply          12  Associativity.Left
addop Divide            12  Associativity.Left
addop Modulus           12  Associativity.Left
addop Add               11  Associativity.Left
addop Subtract          11  Associativity.Left
addop LessEqual          8  Associativity.Left
addop Less               8  Associativity.Left
addop GreaterEqual       8  Associativity.Left
addop Greater            8  Associativity.Left
addop Equal              7  Associativity.Left
addop NotEqual           7  Associativity.Left
addop ConditionalAnd     3  Associativity.Left
addop ConditionalOr      2  Associativity.Left

let pExpr = iop.ExpressionParser

// Declarations
let pIdentifier: Parser<Identifier,UserState> = many1Chars (asciiLetter <|> pchar '_')
let decltype (i:Identifier) = skipChar ':' >>. ws >>. pType |>> fun t -> ScalarVarDecl (i, t)
let declinit (i:Identifier) = ws_str "=" >>. ws >>. pExpr |>> fun e -> ScalarVarDeclInit (i, e)
let declstart = str "let" >>. ws >>. pIdentifier
let pDecl = declstart >>= fun i -> (decltype i <|> declinit i) .>> term

let pProgram = many pDecl