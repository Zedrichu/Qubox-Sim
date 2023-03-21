/// <summary>
/// Module defining the interpretation/optimization of QuLang AST to a quantum circuit.
/// </summary>
module internal QuantumLanguage.Interpreter
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
open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections
open QuantumLanguage.AST

module public Map =    
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
let rec private unwrapBit (bit:bit) (acc:Map<string,int * int>) (no:int): Map<string,int * int> * int =
    match bit with
    | BitA(s, i) -> let map = Map.add s (i, no) acc
                    (map, no + i)
    | BitS s -> let map = Map.add s (1, no) acc
                (map, no + 1)    
    | BitSeq(bit1, bit2) -> let acc1, newNo = unwrapBit bit1 acc no
                            unwrapBit bit2 acc1 newNo

/// <summary>
/// Function to collect allocated Quantum bits and Classical bits in 2 map structures.
/// </summary>
/// <param name="operator">Allocation structure in the AST.operator type</param>
/// <exception cref="System.Exception">Invalid allocation (identifier already used for quantum)</exception>
/// <returns>Tuple of quantum bit mapping and classical bit mapping</returns>
let internal allocateBits (operator:operator):Map<string,int * int> * Map<string,int * int> =
    match operator with
    | AllocQC(qbit, cbit) ->
        let qlist, _ = unwrapBit qbit Map.empty 0 
        let clist, _ = unwrapBit cbit Map.empty 0
        // Check if there are common bits between quantum and classical (semantic error)
        let common = Map.intersect qlist clist
        if not (Map.isEmpty common) then
            let var, _ = List.head (Map.toList common)
            failwith $"Invalid allocation of classical register {var} (already allocated as quantum)."
            else (qlist, clist)
    | _ -> (Map.empty<string,int * int>, Map.empty<string,int * int>)

/// <summary>
/// Function to eager evaluate arithmetic expressions with reduction rules.
/// </summary>
/// <param name="expr">Arithmetic expression to be reduced</param>
/// <param name="memory">Mapping of arithmetic variables to expressions</param>
/// <exception cref="System.Exception">Arithmetic invalid variable access, division by zero</exception>
/// <returns>Reduced evaluation of AST arithmetic expression</returns>
let rec private evalArith (expr:arithExpr) (memory:Map<string, arithExpr>):arithExpr =
    match expr with
    | Pi | Num _ | Float _ -> expr
    | VarA s -> try
                    evalArith (Map.find s memory) memory
                with _ -> failwith $"Unknown variable in expression - {s} has not been declared!"    
    | TimesExpr(x, y) -> let x1 = evalArith x memory
                         let y1 = evalArith y memory
                         match x1, y1 with
                         | Num a, Num b -> Num (a * b)
                         | Num 0, _ | _, Num 0 -> Num 0
                         | Float 0.0, _ | _, Float 0.0 -> Float 0.0
                         | Num 1, _ | Float 1.0, _ -> y1
                         | _, Num 1 | _, Float 1.0 -> x1
                         | Float a, Num b -> Float (a * float b)
                         | Num a, Float b -> Float (float a * b)
                         | Float a, Float b -> Float (a * b)
                         | DivExpr(a, c), DivExpr(b, d) -> evalArith (DivExpr(TimesExpr(a, b), TimesExpr(c, d))) memory
                         | DivExpr(a, c), b | b, DivExpr(a,c) when b=c -> a
                         | PlusExpr(c, d), Num b -> evalArith (PlusExpr(TimesExpr(c, Num b), TimesExpr(d, Num b))) memory
                         | PlusExpr(c, d), Float b -> evalArith (PlusExpr(TimesExpr(c, Float b), TimesExpr(d, Float b))) memory
                         | Num a, PlusExpr(c, d) -> evalArith (PlusExpr(TimesExpr(c, Num a), TimesExpr(d, Num a))) memory
                         | Float a, PlusExpr(c, d) -> evalArith (PlusExpr(TimesExpr(c, Float a), TimesExpr(d, Float a))) memory
                         | UMinusExpr a, _ -> evalArith (UMinusExpr(TimesExpr(a, y1))) memory
                         | _, UMinusExpr a -> evalArith (UMinusExpr(TimesExpr(x1, a))) memory
                         | _ -> TimesExpr(x1, y1)
    | DivExpr(x, y) -> let x1 = evalArith x memory
                       let y1 = evalArith y memory
                       match x1, y1 with
                       | Num a, Num b -> Num (a / b)
                       | Num 0, _ -> Num 0
                       | _, Num 0 -> failwith $"Invalid division by zero - undefined!"
                       | Float 0.0, _ -> Float 0.0
                       | _, Float 0.0 -> failwith $"Invalid division by zero - undefined!"
                       | Num 1, _ -> y1
                       | _, Num 1 -> x1
                       | c, d when c=d -> Num 1
                       | Float 1.0, _ -> y1
                       | _, Float 1.0 -> x1
                       | Float a, Num b -> Float (a / float b)
                       | Num a, Float b -> Float (float a / b)
                       | Float a, Float b -> Float (a / b)
                       | UMinusExpr a, _ -> evalArith (UMinusExpr(DivExpr(a, y1))) memory
                       | _, UMinusExpr a -> evalArith (UMinusExpr(DivExpr(x1, a))) memory
                       | _ -> DivExpr(x1, y1)
    | PlusExpr(x, y) -> let x1 = evalArith x memory
                        let y1 = evalArith y memory
                        match x1, y1 with
                        | Num a, Num b -> Num (a + b)
                        | Num 0, _ -> y1
                        | c, d when c=d -> evalArith (TimesExpr(Num 2, x1)) memory
                        | _, Num 0 -> x1
                        | Float 0.0, _ -> y1
                        | _, Float 0.0 -> x1
                        | Float a, Num b -> Float (a + float b)
                        | Num a, Float b -> Float (float a + b)
                        | Float a, Float b -> Float (a + b)
                        | UMinusExpr a, _ -> evalArith (MinusExpr(a, y1)) memory
                        | _, UMinusExpr a -> evalArith (MinusExpr(x1, a)) memory
                        | _ -> PlusExpr(x1, y1)    
    | MinusExpr(x, y) -> let x1 = evalArith x memory
                         let y1 = evalArith y memory
                         match x1, y1 with
                         | Num a, Num b -> Num (a - b)
                         | c , d when c=d -> Num 0
                         | Num 0, _ -> evalArith (UMinusExpr(y1)) memory
                         | _, Num 0 -> x1
                         | Float 0.0, _ -> evalArith (UMinusExpr(y1)) memory
                         | _, Float 0.0 -> x1
                         | Float a, Num b -> Float (a - float b)
                         | Num a, Float b -> Float (float a - b)
                         | Float a, Float b -> Float (a - b)
                         | UMinusExpr a, _ -> evalArith (UMinusExpr(PlusExpr(a, y1))) memory
                         | _, UMinusExpr a -> evalArith (PlusExpr(x1, a)) memory
                         | _ -> MinusExpr(x1, y1)
    | UMinusExpr x -> let x1 = evalArith x memory
                      match x1 with
                      | Num a -> Num (-a)
                      | Float a -> Float (-a)
                      | UMinusExpr a -> a
                      | _ -> UMinusExpr(x1)
    | UPlusExpr x -> evalArith x memory
    | PowExpr(x, y) -> let x1 = evalArith x memory
                       let y1 = evalArith y memory
                       match x1, y1 with
                       | Num a, Num b -> Num (int (float a ** float b))
                       | Num 0, _ -> Num 0
                       | _, Num 0 -> Num 1
                       | Float 0.0, _ -> Float 0.0
                       | _, Float 0.0 -> Float 1.0
                       | Num 1, _ -> Num 1
                       | _, Num 1 -> x1
                       | Float 1.0, _ -> Float 1.0
                       | _, Float 1.0 -> x1
                       | Float a, Num b -> Float (a ** float b)
                       | Num a, Float b -> Float (float a ** b)
                       | Float a, Float b -> Float (a ** b)
                       | _ -> PowExpr(x1, y1)
    | ModExpr(x, y) -> let x1 = evalArith x memory
                       let y1 = evalArith y memory
                       match x1, y1 with
                       | Num a, Num b -> Num (a % b)
                       | Num 0, _ -> Num 0
                       | _, Num 0 -> failwith $"Invalid modulo by zero - undefined!"
                       | Float 0.0, _ -> Float 0.0
                       | _, Float 0.0 -> failwith $"Invalid modulo by zero - undefined!"
                       | Float a, Num b -> Float (a % float b)
                       | Num a, Float b -> Float (float a % b)
                       | Float a, Float b -> Float (a % b)
                       | _ -> ModExpr(x1, y1)

/// <summary>
/// Function to eager evaluate boolean expressions with reduction rules
/// </summary>
/// <param name="expr">Boolean expression to be reduced</param>
/// <param name="memory">Boolean mapping of identifiers</param>
/// <param name="memarith">Arithmetic mapping of identifiers</param>
/// <exception cref="System.Exception">Boolean invalid variable access</exception>
/// <returns>Reduced evaluation of AST boolean expression</returns>
let rec private evalBool (expr:boolExpr) (memory:Map<string,boolExpr>) (memarith:Map<string, arithExpr>):boolExpr =
    match expr with 
    | Bool _ -> expr
    | VarB s -> try
                    evalBool (Map.find s memory) memory memarith
                with _ -> failwith $"Unknown variable in expression - {s} has not been declared!"    
    | LogAnd(x,y) -> let x1 = evalBool x memory memarith
                     let y1 = evalBool y memory memarith
                     match x1, y1 with
                     | Bool a, Bool b -> Bool (a && b)
                     | Bool true, _ -> y1
                     | _, Bool true -> x1
                     | Bool false, _ -> Bool false
                     | _, Bool false -> Bool false
                     | c,d when c=d -> x1
                     | c, Neg(d) | Neg(d), c when c=d -> Bool false
                     | _ -> LogAnd(x1, y1)
    | LogOr(x, y) -> let x1 = evalBool x memory memarith
                     let y1 = evalBool y memory memarith
                     match x1, y1 with
                     | Bool a, Bool b -> Bool (a || b)
                     | Bool true, _ -> Bool true
                     | _, Bool true -> Bool true
                     | Bool false, _ -> y1
                     | _, Bool false -> x1
                     | c,d when c=d -> x1
                     | c, Neg(d) | Neg(d), c when c=d -> Bool true
                     | _ -> LogOr(x1, y1)
    | Neg x ->  let x1 = evalBool x memory memarith
                match x1 with
                | Bool a -> Bool (not a)
                | Neg a -> a
                | _ -> Neg(x)
    | Equal(x, y) -> let x1 = evalArith x memarith
                     let y1 = evalArith y memarith
                     match x1, y1 with
                     | Num a, Num b -> Bool (a = b)
                     | Num a, Float b -> Bool (float a = b)
                     | Float a, Num b -> Bool (a = float b)
                     | Float a, Float b -> Bool (a = b)
                     | c, d when c=d -> Bool true
                     | _ -> Equal(x1, y1)
    | NotEqual(x, y) -> let x1 = evalArith x memarith
                        let y1 = evalArith y memarith
                        match x1, y1 with
                        | Num a, Num b -> Bool (a <> b)
                        | Num a, Float b -> Bool (float a <> b)
                        | Float a, Num b -> Bool (a <> float b)
                        | Float a, Float b -> Bool (a <> b)
                        | c, d when c=d -> Bool false
                        | _ -> Neg(Equal(x1, y1))
    | Less(x, y) -> let x1 = evalArith x memarith
                    let y1 = evalArith y memarith
                    match x1, y1 with
                    | Num a, Num b -> Bool (a < b)
                    | Num a, Float b -> Bool (float a < b)
                    | Float a, Num b -> Bool (a < float b)
                    | Float a, Float b -> Bool (a < b)
                    | _ -> Less(x1, y1)
    | LessEqual(x, y) -> let x1 = evalArith x memarith
                         let y1 = evalArith y memarith
                         match x1, y1 with
                         | Num a, Num b -> Bool (a <= b)
                         | Num a, Float b -> Bool (float a <= b)
                         | Float a, Num b -> Bool (a <= float b)
                         | Float a, Float b -> Bool (a <= b)
                         // a <= b = not (b < a)
                         | _ -> evalBool (Neg (Less(y1, x1))) memory memarith
    | Greater(x, y) -> let x1 = evalArith x memarith
                       let y1 = evalArith y memarith
                       match x1, y1 with
                       | Num a, Num b -> Bool (a > b)
                       | Num a, Float b -> Bool (float a > b)
                       | Float a, Num b -> Bool (a > float b)
                       | Float a, Float b -> Bool (a > b)
                       // a > b = b < a
                       | _ -> evalBool (Less(y1, x1)) memory memarith
    | GreaterEqual(x,y) -> let x1 = evalArith x memarith
                           let y1 = evalArith y memarith
                           match x1, y1 with
                           | Num a, Num b -> Bool (a >= b)
                           | Num a, Float b -> Bool (float a >= b)
                           | Float a, Num b -> Bool (a >= float b)
                           | Float a, Float b -> Bool (a >= b)
                           // a >= b = not (a < b)
                           | _ -> evalBool (Neg (Less(x1, y1))) memory memarith
    | Check _ -> expr
    
/// <summary>
/// Function to optimize the AST by reducing expressions and building memory
/// </summary>
/// <param name="expr">Abstract Syntax Tree expression for optimization</param>
/// <param name="memArith">Initial arithmetic variable memory</param>
/// <param name="memBool">Initial boolean variable memory</param>
/// <returns>Tuple of arithmetic and boolean variable memories and optimized AST</returns>
let rec internal optimizeOperator (expr:operator) (memArith:Map<string, arithExpr>)
    (memBool:Map<string, boolExpr>):Map<string, arithExpr> * Map<string, boolExpr> * operator =     
    match expr with
    | Assign(s, value) -> let value1 = evalArith value memArith
                          memArith.Add(s, value1), memBool, Assign(s, value1)
    | AssignB(s, value) -> let value1 = evalBool value memBool memArith
                           memArith, memBool.Add(s, value1), AssignB(s, value1)
    | Condition(b, op) -> let b1 = evalBool b memBool memArith
                          memArith, memBool, Condition(b1, op)
    | Order(opx, opy) -> let memArith1, memBool1, opx1 = optimizeOperator opx memArith memBool
                         let memArith2, memBool2, opy1 = optimizeOperator opy memArith1 memBool1
                         memArith2, memBool2, Order(opx1, opy1)
    | _ -> memArith, memBool, expr
    
    
/// <summary>
/// Function to validate the target register of quantum operators.
/// </summary>
/// <param name="bit">Target register to be validated</param>
/// <param name="flag">Type of register expected</param>
/// <param name="memory">Memory mapping of corresponding types</param>
/// <exception cref="System.Exception">Invalid allocation of register</exception>
let rec private validateRegister (bit:bit) (flag:string) (memory:Map<string, int * int>):unit =
    match bit with
    | BitA(s, i) ->
                try
                    let alloc, _ = Map.find s memory
                    if (i<0 && alloc <= i) then failwith "Overflow of register"
                with _ -> 
                    failwith $"{flag} bit register {s}[{i}] has not been allocated!"
    | BitS s -> if Map.containsKey s memory then ()
                else failwith $"{flag} bit register {s} has not been allocated!"
    | _ -> failwith "Invalid operator target!"   

/// <summary>
/// Function to peek inside boolean expression and extract classical registers used.
/// </summary>
/// <param name="b">Boolean expression</param>
/// <param name="acc">Accumulator set of bits</param>
let rec private peekRegister (b:boolExpr) (acc:Set<bit>):Set<bit> =
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
let rec internal semanticAnalyzer (ast:operator) (memory:Memory):unit =
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
            
            
    