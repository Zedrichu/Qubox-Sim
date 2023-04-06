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
/// Function to translate quantum statements to QuLang declaration.
/// </summary>
/// <param name="st">Statement AST to be translated</param>
/// <returns>QuLang string representation</returns>
let rec private transStatement (st:Statement):string =
    match st with
    | Assign(var, value) -> $"{var} := {value};"
    | AssignB(var, value) -> $"{value} =| {var};"
    | Condition(b, op) -> $"If ( {b} ) {transStatement op}"
    | Measure(q, c) -> $"Measure {transBit q} -> {transBit c};"
    | Reset(bit) -> $"Reset {transBit bit};"
    | Barrier(bit) -> $"Barrier {transBit bit};"
    | PhaseDisk -> "PhaseDisk;"
    | UnaryGate(uTag, bit) -> $"{uTag} {transBit bit};"
    | BinaryGate(bTag, bit1, bit2) -> $"{bTag} {transBit bit1}, {transBit bit2};"
    | ParamGate(pTag, theta, bit) -> $"{pTag} ({theta}) {transBit bit};"
    | BinaryParamGate(bpTag, theta, bit1, bit2) ->
                $"{bpTag} ({theta}) {transBit bit1}, {transBit bit2};"
    | Unitary(theta, phi, lambda, bit) ->
                $"U ({theta}, {phi}, {lambda}) {transBit bit};"
    | Toffoli(bit, bit1, bit2) ->
                $"CCX {transBit bit}, {transBit bit1}, {transBit bit2};"

/// Helper function to aggregate all statements in a flow.    
let rec internal transFlow (flow:Statement list) : string =
    match flow with
    | head::tail -> $"{transStatement head}\n{transFlow tail}"          
    | [] -> ""
    