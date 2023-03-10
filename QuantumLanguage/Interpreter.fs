/// <summary>
/// Module defining the interpretation/optimization of QuLang AST to a quantum circuit.
/// </summary>
module QuantumLanguage.Interpreter
(** F#
 -*- coding: utf-8 -*-
QuLang Interpreter 

Description: Module defining the interpretation/optimization of QuLang AST to a quantum circuit.
Includes expression evaluation and semantic analyzers. 

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 24/02/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.3
@__Status --> DEV
*)
open System
open Microsoft.FSharp.Core
open QuantumLanguage.AST
open Microsoft.FSharp.Collections
module Map=    
    /// Map union function
    let union map1 map2 = Map.fold (fun acc key value -> Map.add key value acc) map1 map2
    
    /// Map intersect function
    let intersect map1 map2 =
        (Map.empty, map1) ||> Map.fold (fun acc k v1 ->
            (acc, Map.tryFind k map2) ||> Option.fold
                (fun acc v2 -> Map.add k (v1,v2) acc) )

/// <summary>
/// Function to collect bit identifiers from AST structure to map
/// </summary>
/// <param name="bit">Bit structure (array form/single/sequenced)</param>
/// <param name="acc">Accumulator map of identifier/number pairs</param>
/// <returns>Map of identifier/number pairs initialized</returns>
let rec unwrapBit (bit:bit) (acc:Map<string,int>): Map<string,int> =
    match bit with
    | BitA(s, i) -> Map.add s i acc
    | BitS s -> Map.add s 1 acc
    | BitSeq(bit1, bit2) -> let acc1 = unwrapBit bit1 acc
                            unwrapBit bit2 acc1

/// <summary>
/// Function to collect allocated Quantum bits and Classical bits in 2 map structures.
/// </summary>
/// <param name="operator">Allocation structure in the AST.operator type</param>
/// <returns>Tuple of quantum bit map and classical bit map</returns>
/// <exception cref="System.Exception">Invalid allocation (identifier already used for quantum)</exception>
let allocateBits (operator:operator):(Map<string,int> * Map<string,int>) =
    match operator with
    | AllocQC(qbit, cbit) ->
        let qlist = unwrapBit qbit Map.empty
        let clist = unwrapBit cbit Map.empty
        // Check if there are common bits between quantum and classical (semantic error)
        let common = Map.intersect qlist clist
        if not (Map.isEmpty common) then
            let (var, _) = List.head (Map.toList common)
            failwith $"Invalid allocation of classical register {var} (already allocated as quantum)."
            else (qlist, clist)
    | _ -> (Map.empty<string,int>, Map.empty<string,int>)

let evalArith (expr:arithExpr):arithExpr = expr //#TODO!
    
let evalBool (expr:boolExpr):boolExpr = expr //#TODO!

/// <summary>
/// Function to validate the target register of quantum operators.
/// </summary>
/// <param name="bit">Target register to be validated</param>
/// <param name="flag">Type of register expected</param>
/// <param name="memory">Memory mapping of corresponding types</param>
/// <exception cref="System.Exception">Invalid allocation of register</exception>
let rec validateRegister (bit:bit) (flag:string) (memory:Map<string, int>):unit =
    match bit with
    | BitA(s, i) ->
                let alloc = Map.find s memory
                if alloc >= i then ()
                else
                    failwith $"{flag} bit register {s}[{i}] has not been allocated!"
    | BitS s -> if Map.containsKey s memory then ()
                else failwith $"{flag} bit register {s} has not been allocated!"
    | _ -> failwith "Invalid operator target!"   

/// <summary>
/// Function to peek inside boolean expression and extract classical registers used.
/// </summary>
/// <param name="b">Boolean expression</param>
/// <param name="acc">Accumulator set of bits</param>
let rec peekRegister (b:boolExpr) (acc:Set<bit>):Set<bit> =
    match b with
    | LogAnd(b1, b2) | LogOr(b1, b2) ->
                        let acc2 = peekRegister b1 acc
                        peekRegister b2 acc2
    | Neg b -> peekRegister b acc
    | Check(bit, _) -> Set.add bit acc
    | _ -> Set.empty 

/// <summary>
/// Function to analyze the semantics of the defined circuit AST. Operators have to
/// be applied on quantum registers only, the measurement result is stored on classical register.
/// </summary>
/// <param name="ast">AST structure to be analyzed semantically</param>
/// <param name="memory">Record mappings of defined identifiers</param>
/// <exception cref="System.Exception">Invalid register definition (semantic)</exception>
let rec semanticAnalyzer (ast:operator) (memory:Memory):unit =
    match ast with
    | Order(op1, op2) ->
                        // Analyze the first operator
                        semanticAnalyzer op1 memory
                        // Analyze the second operator
                        semanticAnalyzer op2 memory
    | Condition(b, op) ->
              // Find used classical registers
              let c_set = peekRegister b Set.empty
              // Validate the found set of registers
              Set.iter (fun x ->
                    validateRegister x "Classical" memory.Classical) c_set
              // Analyze the following operator
              semanticAnalyzer op memory
    | Measure(q, c) -> 
                       validateRegister q "Quantum" memory.Quantum
                       validateRegister c "Classical" memory.Classical
    | Reset q | Barrier q -> validateRegister q "Quantum" memory.Quantum
    | H q | X q | Y q | Z q | T q | S q | I q -> validateRegister q "Quantum" memory.Quantum
    | TDG q | SDG q | SX q | SXDG q -> validateRegister q "Quantum" memory.Quantum
    | RZ(_,q) | RX(_, q) | RY(_, q) | U(_,_,_,q) -> validateRegister q "Quantum" memory.Quantum
    | CNOT(q1, q2) | SWAP(q1, q2)
        | RXX(_,q1, q2)| RZZ(_, q1, q2) ->
                         validateRegister q1 "Quantum" memory.Quantum
                         validateRegister q2 "Quantum" memory.Quantum
    | CCX(q1, q2, q3) -> validateRegister q1 "Quantum" memory.Quantum
                         validateRegister q2 "Quantum" memory.Quantum
                         validateRegister q3 "Quantum" memory.Quantum
    | _ -> ()
            
            
    