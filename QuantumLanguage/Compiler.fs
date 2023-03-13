/// <summary>
/// Compiler module handling the conversion from AST structure to Q# compiling code
/// </summary>
module public QuantumLanguage.Compiler
(* F#
 -*- coding: utf-8 -*-
Q# Compiler from QuLang AST

Description: Compiler module handling the conversion from AST structure to Q# compiling code

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 06/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*)

open AST

/// <summary>
/// Function to compile the quantum results to Q# syntax.
/// </summary>
/// <param name="result">Result expression (AST.result)</param>
/// <returns>Q# string representation</returns>
let rec private compileResult (result:result):string =
    match result with
    | Click -> "Zero"
    | NoClick -> "One"

/// <summary>
/// Function to compile bit structures to Q# syntax.
/// </summary>
/// <param name="expr">Bit expression (sequence, array-like or single)</param>
/// <returns>Q# string representation</returns>
let rec private compileAlloc (expr:bit) (flag:bool):string =
    match flag with
    | true ->
        match expr with
        | BitA(q, i) -> $"use {q}[%i{i}] = Qubit [%i{i}];"
        | BitS(q) -> $"use {q} = Qubit();"
        | BitSeq(q,q_seq) -> compileAlloc q true + "\n" + compileAlloc q_seq true
    | false ->
        match expr with
        | BitA(s, i) -> $"mutable {s}[%i{i}] = new Result[%i{i}];"
        | BitS(s) -> $"mutable {s} = new Result;"
        | BitSeq(q,q_seq) -> compileAlloc q false + "\n" + compileAlloc q_seq false
    
let rec private compileBit (bit:bit):string =
    match bit with
    | BitA(b,i) -> $"{b}[%i{i}]" 
    | BitS b -> b
    | _ -> ""

/// <summary>
/// Function to compile arithmetic expressions to Q# syntax.
/// Recursive on the structure of type arithExpr.
/// </summary>
/// <param name="expr">Arithmetic expression (AST.arithExpr)</param>
/// <returns>Q# string representation</returns>
let rec private compileArith (expr:arithExpr):string = 
    match expr with
    | VarA x -> x
    | Num x -> x.ToString()
    | Float x -> x.ToString()
    | Pi -> "PI ()"
    | TimesExpr(x,y) -> "("+(compileArith x)+" * "+(compileArith y)+")"
    | DivExpr(x,y) -> "("+(compileArith x)+" / "+(compileArith y)+")"
    | PlusExpr(x,y) -> "("+compileArith x+" + "+(compileArith y)+")"
    | MinusExpr(x,y) -> "("+(compileArith x)+" - "+(compileArith y)+")"
    | PowExpr(x, y) -> "("+(compileArith x)+" ^ "+(compileArith y)+")"
    | ModExpr(x, y) -> "("+(compileArith x)+" % "+(compileArith y)+")"
    | UPlusExpr(x) -> "(+ "+(compileArith x)+")"
    | UMinusExpr(x) -> "(- "+(compileArith x)+")"

/// <summary>
/// Function to compile boolean expressions to Q# syntax.
/// Recursive on the structure of type boolExpr
/// </summary>
/// <param name="expr">Boolean expression (AST.boolExpr)</param>
/// <returns>Q# string representation</returns>
let rec private compileBool (expr:boolExpr):string = 
    match expr with 
    | Bool x -> x.ToString()
    | VarB s -> s
    | LogAnd(x,y) -> ""+(compileBool x)+" and "+(compileBool y)+""
    | LogOr(x,y) -> ""+(compileBool x)+" or "+(compileBool y)+""
    | Neg x -> "not ("+(compileBool x)+")"
    | Check(bit, res) -> "("+compileBit bit+" == "+(compileResult res)+")"
    | Equal(x,y) -> (compileArith x)+"=="+(compileArith y)
    | NotEqual(x,y) -> (compileArith x)+"!="+(compileArith y)
    | Greater(x,y) -> (compileArith x)+">"+(compileArith y)
    | GreaterEqual(x,y) -> (compileArith x)+">="+(compileArith y)
    | Less(x,y) -> (compileArith x)+"<"+(compileArith y)
    | LessEqual(x,y) -> (compileArith x)+"<="+(compileArith y)


/// <summary>
/// Function to compile the quantum operators to Q# syntax.
/// Recursive on the structure of type operator.
/// </summary>
/// <param name="expr">Operator expression (AST.operator)</param>
/// <returns>Q# string representation</returns>
let rec internal compileOperator (expr:operator):string =
    match expr with
    | AllocQC(q_bit, c_bit) -> compileAlloc q_bit true + "\n" + compileAlloc c_bit false + "\n"
    | Measure(q_bit, c_bit) -> "let "+compileBit c_bit+" = M("+compileBit q_bit+");"
    | Assign(var, value) -> "let "+var+" = "+compileArith value
    | AssignB(var, value) -> "let "+var+" = "+compileBool value
    | Order(op1, op2) -> compileOperator op1 + "\n" + compileOperator op2
    | Reset(BitS(q)) -> "Reset("+q+");"
    | Reset(BitA(q, _)) -> "ResetAll("+q+");"
    | Condition(b, operator) -> "if ("+compileBool b+") {"+compileOperator operator+"}"
    | H(bit) -> "H("+compileBit bit+");"
    | I(bit) -> "I("+compileBit bit+");"
    | X(bit) -> "X("+compileBit bit+");"
    | Y(bit) -> "Y("+compileBit bit+");"
    | Z(bit) -> "Z("+compileBit bit+");"
    | TDG(bit) -> "Rz(-PI()/4, "+compileBit bit+");" // T†
    | SDG(bit) -> "Rz(-PI()/2, "+compileBit bit+");" // S†
    | S(bit) -> "S("+compileBit bit+");"
    | T(bit) -> "T("+compileBit bit+");"
    | SX(bit) -> "Rx(PI()/2, "+compileBit bit+");" // Global phase 
    | SXDG(bit) -> "Rx(-PI()/2, "+compileBit bit+");" // Global phase
    | P(phase, bit) -> "Rz("+(compileArith phase).ToString()+", "+compileBit bit+");" // up to global phase
    | RZ(angle, bit) -> "Rz("+(compileArith angle).ToString()+", "+compileBit bit+");"
    | RY(angle, bit) -> "Ry("+(compileArith angle).ToString()+", "+compileBit bit+");"
    | RX(angle, bit) -> "Rx("+(compileArith angle).ToString()+", "+compileBit bit+");"
    //| U(exp1, exp2, exp3, bit) ->
    | CNOT(bit1, bit2) -> "CNOT("+compileBit bit1+", "+compileBit bit2+");"
    | CCX(bit1, bit2, bit3) -> "CCNOT("+compileBit bit1+", "+compileBit bit2+", "+compileBit bit3+");"
    | SWAP(bit1, bit2) -> "SWAP("+compileBit bit1+", "+compileBit bit2+");"
    | RXX(theta, bit1, bit2) -> "Rxx("+(compileArith theta).ToString()+", "+compileBit bit1+", "+compileBit bit2+");"
    | RZZ(theta, bit1, bit2) -> "Rzz("+(compileArith theta).ToString()+", "+compileBit bit1+", "+compileBit bit2+");"
    | _ -> ""