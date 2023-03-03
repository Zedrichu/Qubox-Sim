module QuantumLanguage.Interpreter
(** F#
 -*- coding: utf-8 -*-
Interpreter 

Description: Module defining the Q# compilation transitions and other useful handlers for interpretation/optimization of AST

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 24/02/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.2
@__Status --> DEV
*)
// #TODO! Separate the interpreter QuLang -> Circuit, translator QuLang -> Q# and compiler Circuit -> QuLang

open AST
open System

/// <summary>Function to evaluate arithmetic expressions given memory</summary>
/// <param name="expr"> Arithmetic expression to be evaluated</param>
/// <param name="memory">Mapping of variables to values used to evaluate expression on memory</param>
let rec evalArith expr memory =
    match expr with
    | Num x -> x:float
    | Float x -> x
    | StrA x -> try
                    Map.find x memory
                 with err ->
                     let mes = $"ERROR: Unknown arithmetic variable %s{x} in expression.
                                            \nRevise your variable declarations!"
                     failwith mes
    | Pi -> Math.PI
    | TimesExpr(x, y) -> (evalArith x memory) * (evalArith y memory)
    | DivExpr(x,y) -> 
                    let res = evalArith y memory
                    if res = 0 then 
                            failwith "ERROR: Invalid division by 0 - undefined."
                    else (evalArith x memory) / (evalArith y memory)
    | PlusExpr(x,y) -> (evalArith x memory) + (evalArith y memory) 
    | MinusExpr(x,y) -> (evalArith x memory) - (evalArith y memory)
    | UMinusExpr expr -> -(evalArith expr memory)
    | UPlusExpr expr -> (evalArith expr memory)

/// Function to evaluate boolean expressions given
///     a boolean memory and an arithmetic memory
let rec evalBool expr boolMem arithMem =
    match expr with
    | Bool x -> x
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

/// Result translator to Q# 
let rec transResult result =
    match result with
    | Click -> "Zero"
    | NoClick -> "One"

// Bit translator to Q#
let rec transBit expr =
    match expr with
    | BitA(q, i) -> q + $"[%i{i}]"
    | BitS(q) -> q
    | BitSeq(q,q_seq) -> transBit q + ", " + transBit q_seq
    
/// Arithmetic translator to Q#
let rec transArith expr =
    match expr with
    | StrA(x) -> x
    | Num(x) -> x.ToString()
    | Float(x) -> x.ToString()
    | Pi -> "PI ()"
    | TimesExpr(x,y) -> "("+(transArith x)+"*"+(transArith y)+")"
    | DivExpr(x,y) -> "("+(transArith x)+"/"+(transArith y)+")"
    | PlusExpr(x,y) -> "("+transArith x+"+"+(transArith y)+")"
    | MinusExpr(x,y) -> "("+(transArith x)+"-"+(transArith y)+")"
    | UPlusExpr(x) -> "(+"+(transArith x)+")"
    | UMinusExpr(x) -> "(-"+(transArith x)+")"
    
/// Boolean translator to Q#
let rec transBool expr = 
    match expr with 
    | Bool(x) -> x.ToString()
    | LogAnd(x,y) -> "("+(transBool x)+") and ("+(transBool y)+")"
    | LogOr(x,y) -> "("+(transBool x)+") or ("+(transBool y)+")"
    | Neg(x) -> "!("+(transBool x)+")"
    | Check(bit, res) -> "("+transBit bit+" |> "+(transResult res)+")"
    | Equal(x,y) -> (transArith x)+"=="+(transArith y)
    | NotEqual(x,y) -> (transArith x)+"!="+(transArith y)
    | Greater(x,y) -> (transArith x)+">"+(transArith y)
    | GreaterEqual(x,y) -> (transArith x)+">="+(transArith y)
    | Less(x,y) -> (transArith x)+"<"+(transArith y)
    | LessEqual(x,y) -> (transArith x)+"<="+(transArith y)

    
let rec transOperator expr =
    match expr with
    | AllocQC(BitSeq q, BitSeq c) -> "use " + transBit (BitSeq q) +
                                        " = Qubit();\nmutable " + transBit (BitSeq c)
                                            + " = new Result;"
    | AllocQC(BitA(q, n), BitA(c, i)) -> "use "+q+" = Qubit["+n.ToString()+
                                            "];\nmutable "+c+" = new Result["+i.ToString()+"];"
    | Measure(q_bit, c_bit) -> "let "+transBit c_bit+" = M("+transBit q_bit+");"
    | Assign(var, value) -> "let "+var+" = "+transArith value
    | AssignB(var, value) -> "let "+var+" = "+transBool value
    | Order(op1, op2) -> transOperator op1 + "\n" + transOperator op2
    | Reset(BitS(q)) -> "Reset("+q+");"
    | Reset(BitA(q, _)) -> "ResetAll("+q+");"
    | Condition(b, operator) -> "if ("+transBool b+") {\n"+transOperator operator+"\n}"
    //| Barrier() ->
    | H(bit) -> "H("+transBit bit+");"
    | I(bit) -> "I("+transBit bit+");"
    | X(bit) -> "X("+transBit bit+");"
    | Y(bit) -> "Y("+transBit bit+");"
    | Z(bit) -> "Z("+transBit bit+");"
    | TDG(bit) -> "TDG("+transBit bit+");" // T† #TODO!
    | SDG(bit) -> "Rz(-PI()/2, "+transBit bit+");" // S†
    | S(bit) -> "S("+transBit bit+");"
    | T(bit) -> "T("+transBit bit+");"
    | SX(bit) -> "Rx(PI()/2, "+transBit bit+");"
    | SXDG(bit) -> "Rx(-PI()/2, "+transBit bit+");"
    //| P(phase, bit) ->
    | RZ(angle, bit) -> "Rz("+(evalArith angle).ToString()+", "+transBit bit+");"
    | RY(angle, bit) -> "Ry("+(evalArith angle).ToString()+", "+transBit bit+");"
    | RX(angle, bit) -> "Rx("+(evalArith angle).ToString()+", "+transBit bit+");"
    //| U(exp1, exp2, exp3, bit) ->
    | CNOT(bit1, bit2) -> "CNOT("+transBit bit1+", "+transBit bit2+");"
    | CCX(bit1, bit2, bit3) -> "CCNOT("+transBit bit1+", "+transBit bit2+", "+transBit bit3+");"
    | SWAP(bit1, bit2) -> "SWAP("+transBit bit1+", "+transBit bit2+");"
    | RXX(theta, bit1, bit2) -> "Rxx("+(evalArith theta).ToString()+", "+transBit bit1+", "+transBit bit2+");"
    | RZZ(theta, bit1, bit2) -> "Rzz("+(evalArith theta).ToString()+", "+transBit bit1+", "+transBit bit2+");"
    | _ -> ""
    
// Map union function
let mapUnion map1 map2 = Map.fold (fun acc key value -> Map.add key value acc) map1 map2
