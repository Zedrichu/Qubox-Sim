module QuantumLanguage.Interpreter

(*
/* F#
 -*- coding: utf-8 -*-
Interpreter

Description: Module defining the Q# compilation transitions and other useful handlers for interpretation/optimization of AST

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 24/02/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.1
@__Status --> DEV
*/

*)

open AST
open Handler
open System
open System.IO

// Arithmetic translator to Q#
let rec transArith expr =
    match expr with
    | StrA(x) -> x
    | Num(x) -> x.ToString()
    | Float(x) -> x.ToString()
    | One -> "ONE"
    | Zero -> "ZERO"
    | Pi -> Math.PI.ToString()
    | TimesExpr(x,y) -> "("+(transArith x)+"*"+(transArith y)+")"
    | DivExpr(x,y) -> "("+(transArith x)+"/"+(transArith y)+")"
    | PlusExpr(x,y) -> "("+transArith x+"+"+(transArith y)+")"
    | MinusExpr(x,y) -> "("+(transArith x)+"-"+(transArith y)+")"
    | UPlusExpr(x) -> "(+"+(transArith x)+")"
    | UMinusExpr(x) -> "(-"+(transArith x)+")"
    
// Boolean translator to Q#
let rec transBool expr = 
    match expr with 
    | Bool(x) -> x.ToString()
    | StrB(x) -> x
    | ShortCircuitAnd(x,y) -> "("+(transBool x)+")&&("+(transBool y)+")"
    | ShortCircuitOr(x,y) -> "("+(transBool x)+")||("+(transBool y)+")"
    | LogAnd(x,y) -> "("+(transBool x)+") and ("+(transBool y)+")"
    | LogOr(x,y) -> "("+(transBool x)+") or ("+(transBool y)+")"
    | Neg(x) -> "!("+(transBool x)+")"
    | Equal(x,y) -> (transArith x)+"=="+(transArith y)
    | NotEqual(x,y) -> (transArith x)+"!="+(transArith y)
    | Greater(x,y) -> (transArith x)+">"+(transArith y)
    | GreaterEqual(x,y) -> (transArith x)+">="+(transArith y)
    | Less(x,y) -> (transArith x)+"<"+(transArith y)
    | LessEqual(x,y) -> (transArith x)+"<="+(transArith y)

// Bit translator to Q#
let rec transBit expr =
    match expr with
    | BitA(q, i) -> q + $"[%i{i}]"
    | BitS(q) -> q
    | BitSeq(q,q_seq) -> transBit q + ", " + transBit q_seq
    
let rec transOperator expr =
    match expr with
    | AllocSeq(q_seq, c_seq) -> "use " + transBit q_seq +
                                  " = Qubit();\nmutable " + transBit c_seq
                                    + " = new Result;"
    | AllocQC(BitA(q, n), BitA(c, i)) -> "use "+q+" = Qubit["+n.ToString()+
                                            "];\nmutable "+c+" = new Result["+i.ToString()+"];"
    //| Measure() ->
    //| Measure() -> 
    | Assign(var, value) -> "let "+var+" = "+transArith value
    | Order(op1, op2) -> transOperator op1 + transOperator op2
    | Reset(BitS(q)) -> "Reset("+q+");"
    | Reset(BitA(q, _)) -> "ResetAll("+q+");"
    //| Condition() ->
    //| Barrier() ->
    | H(bit) -> "H("+transBit bit+");"
    | I(bit) -> "I("+transBit bit+");"
    | X(bit) -> "X("+transBit bit+");"
    | Y(bit) -> "Y("+transBit bit+");"
    | Z(bit) -> "Z("+transBit bit+");"
    | TDG(bit) -> "TDG("+transBit bit+");"
    | SDG(bit) -> "SDG("+transBit bit+");"
    | S(bit) -> "S("+transBit bit+");"
    | T(bit) -> "T("+transBit bit+");"
    | SX(bit) -> "SX("+transBit bit+");"
    | SXDG(bit) -> "SXDG("+transBit bit+");"
    //| P(phase, bit) ->
    //| RZ(angle, bit) ->
    //| RY(angle, bit) ->
    //| RX(angle, bit) ->
    //| U(exp1, exp2, exp3, bit) ->
    | CNOT(bit1, bit2) -> "CNOT("+transBit bit1+", "+transBit bit2+");"
    //| CCX(bit1, bit2, bit3) ->
    | SWAP(bit1, bit2) -> "SWAP("+transBit bit1+", "+transBit bit2+");"
    //| RXX(expr, bit1, bit2) ->
    //| RZZ(expr, bit1, bit2) ->
    | _ -> ""
    
// Map union function
let mapUnion map1 map2 = Map.fold (fun acc key value -> Map.add key value acc) map1 map2


