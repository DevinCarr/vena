module Vena.CodeGeneration
open Vena.AST

let emit (expr:Expression) = expr.Emit()