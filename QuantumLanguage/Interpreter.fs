/// <summary>
/// Module defining the interpretation/optimization of QuLang AST to a quantum circuit.
/// </summary>
module QuantumLanguage.Interpreter
(** F#
 -*- coding: utf-8 -*-
QuLang Interpreter 

Description: Module defining the interpretation/optimization of QuLang AST to a quantum circuit.
Includes expression evaluation and syntactical analyzers. 

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 24/02/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.2
@__Status --> DEV
*)
open AST
open System

/// <summary>Function to lazy evaluate arithmetic expressions given memory</summary>
/// <param name="expr"> Arithmetic expression to be evaluated</param>
/// <param name="memory">Mapping of variables to values used to evaluate expression on memory</param>
let rec evalArith expr memory =
    match expr with
    | Num x -> x:float
    | Float x -> x
    | VarA x -> try
                    Map.find x memory
                 with err ->
                     let mes = $"ERROR: Unknown arithmetic variable %s{x} in expression.
                                            \nRevise your variable declarations!"
                     failwith mes
    | Pi -> Math.PI
    | TimesExpr(x, y) -> (evalArith x memory) * (evalArith y memory)
    | DivExpr(Num x, Num y) -> 
                    let res = evalArith (Num y) memory
                    if res = 0 then 
                            failwith "ERROR: Invalid division by 0 - undefined."
                    else float(int(evalArith (Num x) memory) / int(evalArith (Num y) memory))
    | DivExpr(x, y) ->
                    let res = evalArith y memory
                    if res = 0 then 
                            failwith "ERROR: Invalid division by 0 - infinity."
                    else (evalArith x memory) / (evalArith y memory)
    | PlusExpr(x,y) -> (evalArith x memory) + (evalArith y memory) 
    | MinusExpr(x,y) -> (evalArith x memory) - (evalArith y memory)
    | ModExpr(x,y) -> (evalArith x memory) % (evalArith y memory)
    | PowExpr(x, y) -> Math.Pow(evalArith x memory, evalArith y memory)
    | UMinusExpr expr -> -(evalArith expr memory)
    | UPlusExpr expr -> (evalArith expr memory)

/// Function to evaluate boolean expressions given
///     a boolean memory and an arithmetic memory
let rec evalBool expr boolMem arithMem =
    match expr with
    | Bool x -> x
    | VarB x -> try
                    Map.find x boolMem
                with err ->
                    let mes = $"ERROR: Unknown boolean variable %s{x} in expression.
                                            \nRevise your variable declarations!"
                    failwith mes
    | LogAnd(x,y) -> (evalBool x boolMem arithMem) && (evalBool y boolMem arithMem)
    | LogOr(x,y) -> (evalBool x boolMem arithMem) || (evalBool y boolMem arithMem)
    | Neg x -> not (evalBool x boolMem arithMem)
    | Check _ -> failwith "ERROR: Measurement evaluation requires simulation."
    | Equal(x,y) -> (evalArith x arithMem) = (evalArith y arithMem)
    | NotEqual(x,y) -> (evalArith x arithMem) <> (evalArith y arithMem)
    | Greater(x,y) -> (evalArith x arithMem) > (evalArith y arithMem)
    | GreaterEqual(x,y) -> (evalArith x arithMem) >= (evalArith y arithMem)
    | Less(x,y) -> (evalArith x arithMem) < (evalArith y arithMem)
    | LessEqual(x,y) -> (evalArith x arithMem) <= (evalArith y arithMem)
    
// Map union function
let mapUnion map1 map2 = Map.fold (fun acc key value -> Map.add key value acc) map1 map2
