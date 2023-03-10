/// <summary>
/// Translator module handling the conversion from AST back to QuLang code definition.
/// </summary>
module QuantumLanguage.Translator
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

/// <summary>
/// Function to translate quantum results to QuLang declaration.
/// </summary>
/// <param name="result">Result expression (AST.result)</param>
/// <returns>QuLang string representation</returns>
let rec transResult (result:result):string =
    match result with
    | Click -> "Click"
    | NoClick -> "NoClick"

/// <summary>
/// Function to translate quantum bit to QuLang declaration.
/// </summary>
/// <param name="bit">Bit expression (AST.bit)</param>
/// <returns>QuLang string representation</returns>
let rec transBit (bit:bit):string =
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
let rec transArith (expr:arithExpr):string =
    match expr with
    | VarA s -> s
    | Num i -> i.ToString()
    | Float f -> f.ToString()
    | Pi -> "Pi"
    | TimesExpr(x, y) -> "("+transArith x + " * " + transArith y+")"
    | DivExpr(x, y) -> "("+transArith x + " / " + transArith y+")"
    | PlusExpr(x, y) -> "("+transArith x + " + " + transArith y+")"
    | MinusExpr(x, y) -> "("+transArith x + " - " + transArith y+")"
    | PowExpr(x, y) -> "("+transArith x + " ^ " + transArith y+")"
    | ModExpr(x, y) -> "("+transArith x + " % " + transArith y+")"
    | UMinusExpr x -> "(- "+ transArith x+")"
    | UPlusExpr x -> "(+ "+ transArith x+")"

/// <summary>
/// Function to translate boolean expressions to QuLang declaration.
/// Recursive on the structure of type boolExpr.
/// </summary>
/// <param name="expr">Boolean expression (AST.boolExpr)</param>
/// <returns>QuLang string representation</returns>
let rec transBool (expr:boolExpr):string =
    match expr with
    | Bool b -> b.ToString()
    | VarB s -> "~"+s
    | LogAnd(x, y) -> "("+transBool x + " && " + transBool y+")"
    | LogOr(x, y) -> "("+transBool x + " || " + transBool y+")"
    | Neg x -> "not ( "+ transBool x+")"
    | Check(bit, res) -> "("+transBit bit + " |> " + transResult res + ")"
    | Equal(x, y) -> transArith x + " == " + transArith y
    | NotEqual(x, y) -> transArith x + " != " + transArith y
    | Less(x, y) -> transArith x + " < " + transArith y
    | LessEqual(x, y) -> transArith x + " <= " + transArith y
    | Greater(x, y) -> transArith x + " > " + transArith y
    | GreaterEqual(x, y) -> transArith x + " >= " + transArith y

/// <summary>
/// Function to translate quantum operators to QuLang declaration.
/// </summary>
/// <param name="operator">Operator expression (AST.operator)</param>
/// <returns>QuLang string representation</returns>
let rec transOperator (operator:operator):string =
    match operator with
    | AllocQC(bit1, bit2) -> "Qalloc "+transBit bit1+";\nCalloc "+transBit bit2+";"
    | Measure(q, c) -> "Measure "+transBit q+" -> "+transBit c+";"
    | Reset(bit) -> "Reset "+transBit bit+";"
    | Barrier(bit) -> "Barrier "+transBit bit+";"
    | Assign(var, value) -> var+" := "+transArith value+";"
    | AssignB(var, value) -> transBool value+" =| "+var+";"
    | Order(op1, op2) -> transOperator op1+"\n"+transOperator op2
    | Condition(b, op) -> "If ( "+transBool b+" ) "+transOperator op+";"
    | PhaseDisk -> "PhaseDisk ;"
    | H(q) -> "H "+transBit q+";"
    | I(q) -> "ID "+transBit q+";"
    | X(q) -> "X "+transBit q+";"
    | Y(q) -> "Y "+transBit q+";"
    | Z(q) -> "Z "+transBit q+";"
    | S(q) -> "S "+transBit q+";"
    | T(q) -> "T "+transBit q+";"
    | SDG(q) -> "SDG "+transBit q+";"
    | TDG(q) -> "TDG "+transBit q+";"
    | SX q -> "SX "+transBit q+";"
    | SXDG q -> "SXDG "+transBit q+";"
    | P(phase, q) -> "P("+transArith phase+") "+transBit q+";"
    | RZ(angle, bit) -> "RZ("+transArith angle+") "+transBit bit+";"
    | RX(angle, bit) -> "RX("+transArith angle+") "+transBit bit+";"
    | RY(angle, bit) -> "RY("+transArith angle+") "+transBit bit+";"
    | CNOT(bit1, bit2) -> "CNOT "+transBit bit1+", "+transBit bit2+";"
    | CCX(bit1, bit2, bit3) -> "CCX "+transBit bit1+", "+transBit bit2+", "+transBit bit3+";"
    | SWAP(bit1, bit2) -> "SWAP "+transBit bit1+", "+transBit bit2+";"
    | RZZ(theta, bit1, bit2) -> "RZZ("+transArith theta+") "+transBit bit1+", "+transBit bit2+";"
    | RXX(theta, bit1, bit2) -> "RXX("+transArith theta+") "+transBit bit1+", "+transBit bit2+";"
    | U(exp1, exp2, exp3, bit) -> "U("+transArith exp1+", "+transArith exp2+", "+transArith exp3+") "+transBit bit+";"
    | NOP -> ""
    