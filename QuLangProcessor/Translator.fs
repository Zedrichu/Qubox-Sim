/// <summary>
/// Translator module handling the conversion from AST structure to Q# compiling code
/// </summary>
module public QuLangProcessor.Translator
(** F#
 -*- coding: utf-8 -*-
Q# Translator from QuLang AST

Description: Translator module handling the conversion from AST structure to Q# compiling code

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 06/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.1
@__Status --> DEV
*)

open AST
open Tags

/// <summary>
/// Function to compile the quantum results to Q# syntax.
/// </summary>
/// <param name="result">Result expression (AST.Result)</param>
/// <returns>Q# string representation</returns>
let rec private translateResult (result:Result):string =
    match result with
    | Click -> "Zero"
    | NoClick -> "One"

/// <summary>
/// Function to translate bit structures to Q# syntax.
/// </summary>
/// <param name="expr">Bit expression (sequence, array-like or single)</param>
/// <param name="flag">True for qubits, false for classical bits</param>
/// <returns>Q# string representation</returns>
let rec internal translateAlloc (expr:Bit) (flag:bool):string =
    match flag with
    | true ->
        match expr with
        | BitA(q, i) -> $"use {q}[%i{i}] = Qubit [%i{i}];"
        | BitS(q) -> $"use {q} = Qubit();"
        | BitSeq(q,q_seq) -> translateAlloc q true + "\n" + translateAlloc q_seq true
    | false ->
        match expr with
        | BitA(s, i) -> $"mutable {s}[%i{i}] = new Result[%i{i}];"
        | BitS(s) -> $"mutable {s} = new Result;"
        | BitSeq(q,q_seq) -> translateAlloc q false + "\n" + translateAlloc q_seq false

/// <summary>
/// Function to translate a single bit to Q# syntax.
/// </summary>
/// <param name="bit">Q# bit representation</param>
let rec private translateBit (bit:Bit) : string =
    match bit with
    | BitA(b,i) -> $"{b}[%i{i}]" 
    | BitS b -> b

/// <summary>
/// Function to translate arithmetic expressions to Q# syntax.
/// Recursive on the structure of type arithExpr.
/// </summary>
/// <param name="expr">Arithmetic expression (AST.ArithExpr)</param>
/// <returns>Q# string representation</returns>
let rec private translateArith (expr:ArithExpr):string = 
    match expr with
    | VarA x -> x
    | Num x -> $"%f{(float x)}"
    | Float x -> $"%f{x}"
    | Pi -> "PI()"
    | UnaryOp(op, x) -> "("+op.ToString()+(translateArith x)+")"
    | BinaryOp(x, op, y) -> "("+(translateArith x)+op.ToString()+(translateArith y)+")"
   
/// <summary>
/// Function to translate boolean expressions to Q# syntax.
/// Recursive on the structure of type boolExpr
/// </summary>
/// <param name="expr">Boolean expression (AST.boolExpr)</param>
/// <returns>Q# string representation</returns>
let rec private translateBool (expr:BoolExpr):string = 
    match expr with 
    | B x -> let t = x.ToString()
             t.ToLower()
    | VarB s -> s
    | LogicOp(x,And,y) -> ""+(translateBool x)+" and "+(translateBool y)+""
    | LogicOp(x,Or,y) -> ""+(translateBool x)+" or "+(translateBool y)+""
    | LogicOp(x,Xor,y) -> "("+(translateBool x)+" and not "+(translateBool y)+") or ("
                             + (translateBool y)+" and not "+(translateBool x)+")"    
    | Not x -> $"(not {translateBool x})"
    | Check(bit, res) -> "("+translateBit bit+" == "+(translateResult res)+")"
    | RelationOp(x,op,y) -> $"{translateArith x} {op} {translateArith y}"


/// <summary>
/// Function to translate statement to Q# syntax.
/// Recursive on the Flow structure.
/// </summary>
/// <param name="expr">Statement expression (AST.Statement)</param>
/// <returns>Q# string representation</returns>
let rec private translateStatement (expr:Statement):string =
    match expr with
    | Assign(var, value) -> "let "+var+" = "+translateArith value
    | AssignB(var, value) -> "let "+var+" = "+translateBool value
    | Condition(b, st) -> "if ("+translateBool b+") {"+translateStatement st+"}"
    | Measure(q_bit, BitA(s, i)) -> $"set {s} /= {i} <- M("+ translateBit q_bit+");"
    | Measure(q_bit, BitS s) -> $"let {s} = M("+ translateBit q_bit+");"
    | Reset(BitS(q)) -> "Reset("+q+");"
    | Reset(BitA(q, _)) -> "ResetAll("+q+");"
    | UnaryGate(TDG, bit) -> "Rz(-PI()/4.0, "+translateBit bit+");" // T†
    | UnaryGate(SDG, bit) -> "Rz(-PI()/2.0, "+translateBit bit+");" // S†
    | UnaryGate(SX, bit) -> "Rx(PI()/2.0, "+translateBit bit+");" // Global phase 
    | UnaryGate(SXDG, bit) -> "Rx(-PI()/2.0, "+translateBit bit+");" // Global phase
    | UnaryGate(tag, bit) -> $"{tag}({translateBit bit});"
    | ParamGate(P, phase, bit) -> "Rz("+(translateArith phase).ToString()+", "+translateBit bit+");" // up to global phase
    | ParamGate(RZ, angle, bit) -> "Rz("+(translateArith angle).ToString()+", "+translateBit bit+");"
    | ParamGate(RY, angle, bit) -> "Ry("+(translateArith angle).ToString()+", "+translateBit bit+");"
    | ParamGate(RX, angle, bit) -> "Rx("+(translateArith angle).ToString()+", "+translateBit bit+");"
    //| U(exp1, exp2, exp3, bit) -> #TODO! Find equivalent of unitary in Q# syntax
    | BinaryGate(CNOT, bit1, bit2) -> "CNOT("+translateBit bit1+", "+translateBit bit2+");"
    | BinaryGate(SWAP,bit1, bit2) -> "SWAP("+translateBit bit1+", "+translateBit bit2+");"
    //| BinaryGate(CH, bit1, bit2) -> #TODO! Find equivalent Q#
    //| BinaryGate(CS, bit1, bit2) -> #TODO! Find equivalent Q#
    | BinaryParamGate(RXX, theta, bit1, bit2) -> "Rxx("+(translateArith theta).ToString()+", "+translateBit bit1+", "+translateBit bit2+");"
    | BinaryParamGate(RYY, theta, bit1, bit2) -> "Ryy("+(translateArith theta).ToString()+", "+translateBit bit1+", "+translateBit bit2+");"
    | BinaryParamGate(RZZ, theta, bit1, bit2) -> "Rzz("+(translateArith theta).ToString()+", "+translateBit bit1+", "+translateBit bit2+");"
    | Toffoli(bit1, bit2, bit3) -> "CCNOT("+translateBit bit1+", "+translateBit bit2+", "+translateBit bit3+");"
    | _ -> ""

/// Helper to aggregate translated statements in a flow    
let rec internal translateFlow (flow:Statement list):string =
    match flow with
    | head::tail -> translateStatement head + "\n\t" + translateFlow tail
    | [] -> ""