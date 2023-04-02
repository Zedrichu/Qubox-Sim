/// <summary>
/// Translator module handling the conversion from AST back to QuLang code definition.
/// </summary>
module internal QuantumLanguage.Translator
(* F#
 -*- coding: utf-8 -*-
AST to QuLang Translator (reverse interpreter)

Description: Translator module handling the conversion from AST back to QuLang code definition.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 07/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*)

open AST
open QuantumLanguage.AST

/// <summary>
/// Function to translate quantum bit to QuLang declaration.
/// </summary>
/// <param name="bit">Bit expression (AST.bit)</param>
/// <returns>QuLang string representation</returns>
let rec internal transBit (bit:Bit):string =
    match bit with
    | BitA(s, i) -> s + $"[%i{i}]"
    | BitS s -> s
    | BitSeq(bit, bit_seq) -> transBit bit + ", " + transBit bit_seq

/// <summary>
/// Function to translate arithmetic expressions to QuLang declaration.
/// Recursive on the structure of type arithExpr.
/// </summary>
/// <param name="expr">Arithmetic expression (AST.arithExpr)</param>
/// <returns>QuLang string representation</returns>
let rec private transArith (expr:ArithExpr):string =
    match expr with
    | VarA s -> s
    | Num i -> i.ToString()
    | Float f -> f.ToString()
    | Pi -> "Pi"
    | BinaryOp (a1, op, a2) -> $"({a1} {op} {a2})"
    | UnaryOp (op, a) -> $"{op} ({a})"

/// <summary>
/// Function to translate boolean expressions to QuLang declaration.
/// Recursive on the structure of type boolExpr.
/// </summary>
/// <param name="expr">Boolean expression (AST.boolExpr)</param>
/// <returns>QuLang string representation</returns>
let rec private transBool (expr:BoolExpr):string =
    match expr with
    | B b -> b.ToString()
    | VarB s -> "~"+s
    | LogicOp (b1, op, b2) -> $"({b1} {op} {b2})"
    | RelationOp (a1, op, a2) -> $"({a1} {op} {a2})"
    | Not b -> $"not ({b})"
    | Check (cb, r) -> $"{cb} |> {r}"

/// <summary>
/// Function to translate quantum statements to QuLang declaration.
/// </summary>
/// <param name="st">Statement AST to be translated</param>
/// <returns>QuLang string representation</returns>
let rec private transStatement (st:Statement):string =
    match st with
    | Assign(var, value) -> $"{var} := {transArith value};"
    | AssignB(var, value) -> $"{transBool value} =| {var};"
    | Condition(b, op) -> $"If ( {transBool b} ) {transStatement op}"
    | Measure(q, c) -> $"Measure {transBit q} -> {transBit c};"
    | Reset(bit) -> $"Reset {transBit bit};"
    | Barrier(bit) -> $"Barrier {transBit bit};"
    | PhaseDisk -> "PhaseDisk;"
    | UnaryGate(uTag, bit) -> $"{uTag} {transBit bit};"
    | BinaryGate(bTag, bit1, bit2) -> $"{bTag} {transBit bit1}, {transBit bit2};"
    | ParamGate(pTag, theta, bit) -> $"{pTag} ({transArith theta}) {transBit bit};"
    | BinaryParamGate(bpTag, theta, bit1, bit2) ->
                $"{bpTag} ({transArith theta}) {transBit bit1}, {transBit bit2};"
    | Unitary(theta, phi, lambda, bit) ->
                $"U ({transArith theta}, {transArith phi}, {transArith lambda}) {transBit bit};"
    | Toffoli(bit, bit1, bit2) ->
                $"CCX {transBit bit}, {transBit bit1}, {transBit bit2};"

/// Helper function to aggregate all statements in a flow.    
let rec internal transFlow (flow:Statement list) : string =
    match flow with
    | head::tail -> $"{transStatement head}\n{transFlow tail}"          
    | [] -> ""
    