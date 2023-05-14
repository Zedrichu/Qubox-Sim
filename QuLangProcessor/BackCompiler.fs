/// <summary>
/// Back-compiler module handling the conversion from AST back to QuLang code definition.
/// </summary>
module internal QuLangProcessor.BackCompiler
(** F#
 -*- coding: utf-8 -*-
AST to QuLang Back-compiler (reverse compiler)

Description: Back-compiler module handling the conversion from AST back to QuLang code definition.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 07/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.1
@__Status --> DEV
*)

open AST

/// <summary>
/// Function to translate quantum bit to QuLang declaration.
/// </summary>
/// <param name="bit">Bit expression (AST.bit)</param>
/// <returns>QuLang string representation</returns>
let rec internal backCompileBit (bit:Bit):string =
    match bit with
    | BitA(s, i) -> s + $"[%i{i}]"
    | BitS s -> s
    | BitSeq(bit, bit_seq) -> backCompileBit bit + ", " + backCompileBit bit_seq

/// <summary>
/// Function to translate quantum statements to QuLang declaration.
/// </summary>
/// <param name="st">Statement AST to be translated</param>
/// <returns>QuLang string representation</returns>
let rec private backCompileStatement (st:Statement):string =
    match st with
    | Assign(var, value) -> $"{var} := {value};"
    | AssignB(var, value) -> $"{value} =| {var};"
    | Condition(b, op) -> $"If ( {b} ) {backCompileStatement op}"
    | Measure(q, c) -> $"Measure {backCompileBit q} -> {backCompileBit c};"
    | Reset(bit) -> $"Reset {backCompileBit bit};"
    | Barrier(bit) -> $"Barrier {backCompileBit bit};"
    | PhaseDisk -> "PhaseDisk;"
    | UnaryGate(uTag, bit) -> $"{uTag} {backCompileBit bit};"
    | BinaryGate(bTag, bit1, bit2) -> $"{bTag} {backCompileBit bit1}, {backCompileBit bit2};"
    | ParamGate(pTag, theta, bit) -> $"{pTag} ({theta}) {backCompileBit bit};"
    | BinaryParamGate(bpTag, theta, bit1, bit2) ->
                $"{bpTag} ({theta}) {backCompileBit bit1}, {backCompileBit bit2};"
    | Unitary(theta, phi, lambda, bit) ->
                $"U ({theta}, {phi}, {lambda}) {backCompileBit bit};"
    | Toffoli(bit, bit1, bit2) ->
                $"CCX {backCompileBit bit}, {backCompileBit bit1}, {backCompileBit bit2};"

/// Helper function to aggregate all statements in a flow.    
let rec internal backCompileFlow (flow:Statement list) : string =
    match flow with
    | head::tail -> $"{backCompileStatement head}\n{backCompileFlow tail}"          
    | [] -> ""
    