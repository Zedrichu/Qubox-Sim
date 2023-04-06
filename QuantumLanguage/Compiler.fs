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
open QuantumLanguage.AST
open Tags

/// <summary>
/// Function to compile the quantum results to Q# syntax.
/// </summary>
/// <param name="result">Result expression (AST.Result)</param>
/// <returns>Q# string representation</returns>
let rec private compileResult (result:Result):string =
    match result with
    | Click -> "Zero"
    | NoClick -> "One"

/// <summary>
/// Function to compile bit structures to Q# syntax.
/// </summary>
/// <param name="expr">Bit expression (sequence, array-like or single)</param>
/// <param name="flag">True for qubits, false for classical bits</param>
/// <returns>Q# string representation</returns>
let rec internal compileAlloc (expr:Bit) (flag:bool):string =
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

/// <summary>
/// Function to compile a single bit to Q# syntax.
/// </summary>
/// <param name="bit">Q# bit representation</param>
let rec private compileBit (bit:Bit) : string =
    match bit with
    | BitA(b,i) -> $"{b}[%i{i}]" 
    | BitS b -> b

/// <summary>
/// Function to compile arithmetic expressions to Q# syntax.
/// Recursive on the structure of type arithExpr.
/// </summary>
/// <param name="expr">Arithmetic expression (AST.ArithExpr)</param>
/// <returns>Q# string representation</returns>
let rec private compileArith (expr:ArithExpr):string = 
    match expr with
    | VarA x -> x
    | Num x -> x.ToString()
    | Float x -> x.ToString()
    | Pi -> "PI ()"
    | UnaryOp(op, x) -> "("+op.ToString()+(compileArith x)+")"
    | BinaryOp(x, op, y) -> "("+(compileArith x)+op.ToString()+(compileArith y)+")"
   
/// <summary>
/// Function to compile boolean expressions to Q# syntax.
/// Recursive on the structure of type boolExpr
/// </summary>
/// <param name="expr">Boolean expression (AST.boolExpr)</param>
/// <returns>Q# string representation</returns>
let rec private compileBool (expr:BoolExpr):string = 
    match expr with 
    | B x -> x.ToString()
    | VarB s -> s
    | LogicOp(x,And,y) -> ""+(compileBool x)+" and "+(compileBool y)+""
    | LogicOp(x,Or,y) -> ""+(compileBool x)+" or "+(compileBool y)+""
    | LogicOp(x,Xor,y) -> "("+(compileBool x)+" and not "+(compileBool y)+") or ("
                             + (compileBool y)+" and not "+(compileBool x)+")"    
    | Not x -> $"(not {compileBool x})"
    | Check(bit, res) -> "("+compileBit bit+" == "+(compileResult res)+")"
    | RelationOp(x,op,y) -> $"{compileArith x} {op} {compileArith y}"


/// <summary>
/// Function to compile statement to Q# syntax.
/// Recursive on the Flow structure.
/// </summary>
/// <param name="expr">Statement expression (AST.Statement)</param>
/// <returns>Q# string representation</returns>
let rec private compileStatement (expr:Statement):string =
    match expr with
    | Assign(var, value) -> "let "+var+" = "+compileArith value
    | AssignB(var, value) -> "let "+var+" = "+compileBool value
    | Condition(b, st) -> "if ("+compileBool b+") {"+compileStatement st+"}"
    | Measure(q_bit, c_bit) -> "let "+compileBit c_bit+" = M("+compileBit q_bit+");"
    | Reset(BitS(q)) -> "Reset("+q+");"
    | Reset(BitA(q, _)) -> "ResetAll("+q+");"
    | UnaryGate(TDG, bit) -> "Rz(-PI()/4, "+compileBit bit+");" // T†
    | UnaryGate(SDG, bit) -> "Rz(-PI()/2, "+compileBit bit+");" // S†
    | UnaryGate(SX, bit) -> "Rx(PI()/2, "+compileBit bit+");" // Global phase 
    | UnaryGate(SXDG, bit) -> "Rx(-PI()/2, "+compileBit bit+");" // Global phase
    | UnaryGate(tag, bit) -> $"{tag}({compileBit bit});"
    | ParamGate(P, phase, bit) -> "Rz("+(compileArith phase).ToString()+", "+compileBit bit+");" // up to global phase
    | ParamGate(RZ, angle, bit) -> "Rz("+(compileArith angle).ToString()+", "+compileBit bit+");"
    | ParamGate(RY, angle, bit) -> "Ry("+(compileArith angle).ToString()+", "+compileBit bit+");"
    | ParamGate(RX, angle, bit) -> "Rx("+(compileArith angle).ToString()+", "+compileBit bit+");"
    //| U(exp1, exp2, exp3, bit) -> #TODO! Find equivalent of unitary in Q# syntax
    | BinaryGate(CNOT, bit1, bit2) -> "CNOT("+compileBit bit1+", "+compileBit bit2+");"
    | BinaryGate(SWAP,bit1, bit2) -> "SWAP("+compileBit bit1+", "+compileBit bit2+");"
    //| BinaryGate(CH, bit1, bit2) -> #TODO! Find equivalent Q#
    //| BinaryGate(CS, bit1, bit2) -> #TODO! Find equivalent Q#
    | BinaryParamGate(RXX, theta, bit1, bit2) -> "Rxx("+(compileArith theta).ToString()+", "+compileBit bit1+", "+compileBit bit2+");"
    | BinaryParamGate(RYY, theta, bit1, bit2) -> "Ryy("+(compileArith theta).ToString()+", "+compileBit bit1+", "+compileBit bit2+");"
    | BinaryParamGate(RZZ, theta, bit1, bit2) -> "Rzz("+(compileArith theta).ToString()+", "+compileBit bit1+", "+compileBit bit2+");"
    | Toffoli(bit1, bit2, bit3) -> "CCNOT("+compileBit bit1+", "+compileBit bit2+", "+compileBit bit3+");"
    | _ -> ""

/// Helper to aggregate statements in a flow    
let rec internal compileFlow (flow:Statement list):string =
    match flow with
    | head::tail -> compileStatement head + "\n" + compileFlow tail
    | [] -> ""