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
/// <param name="no">Current bit number</param>
/// <returns>Map of identifier/number pairs initialized</returns>
let rec private unwrapBit (bit:Bit) (acc:Map<string,int * int>) (no:int): Map<string,int * int> * int =
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
/// <param name="alloc">Allocation structure in the AST.operator type</param>
/// <exception cref="System.Exception">Invalid allocation (identifier already used for quantum)</exception>
/// <returns>Tuple of quantum bit mapping and classical bit mapping</returns>
let internal allocateBits (alloc:Allocation):Map<string,int * int> * Map<string,int * int> =
    let (AllocQC (qbit, cbit)) = alloc
    let qlist, _ = unwrapBit qbit Map.empty 0 
    let clist, _ = unwrapBit cbit Map.empty 0
    // Check if there are common bits between quantum and classical (semantic error)
    let common = Map.intersect qlist clist
    if not (Map.isEmpty common) then
        let var, _ = List.head (Map.toList common)
        failwith $"Invalid allocation of classical register {var} (already allocated as quantum)."
        else (qlist, clist)

/// <summary>
/// Function to eager evaluate arithmetic expressions with reduction rules.
/// </summary>
/// <param name="expr">Arithmetic expression to be reduced</param>
/// <param name="memory">Mapping of arithmetic variables to expressions</param>
/// <exception cref="System.Exception">Arithmetic invalid variable access, division by zero</exception>
/// <returns>Reduced evaluation of AST arithmetic expression</returns>
let rec internal evalArith (expr:ArithExpr) (memory:Map<string, ArithExpr * int>) : ArithExpr =
    match expr with
    | Pi | Num _ | Float _ -> expr
    | VarA s -> try
                    let a, _ = Map.find s memory
                    evalArith a memory
                with _ -> failwith $"Unknown variable in expression - {s} has not been declared!"    
    | BinaryOp(x,Mul, y) ->  let x1 = evalArith x memory
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
                             | BinaryOp(a, Div, c), BinaryOp(b, Div, d) ->
                                 evalArith (BinaryOp(BinaryOp(a, Mul, b), Div, BinaryOp(c, Mul, d))) memory
                             | BinaryOp(a, Div, c), b | b, BinaryOp(a, Div, c) when b=c -> a
                             | BinaryOp(c, Add, d), Num b ->
                                 evalArith (BinaryOp(BinaryOp(c, Mul, Num b), Add, BinaryOp(d, Mul, Num b))) memory
                             | BinaryOp(c, Add, d), Float b ->
                                 evalArith (BinaryOp(BinaryOp(c, Mul, Float b), Add, BinaryOp(d, Mul, Float b))) memory
                             | Num a, BinaryOp(c, Add, d) ->
                                 evalArith (BinaryOp(BinaryOp(c, Mul, Num a), Add, BinaryOp(d, Mul, Num a))) memory
                             | Float a, BinaryOp(c, Add, d) ->
                                 evalArith (BinaryOp(BinaryOp(c, Mul, Float a), Add, BinaryOp(d, Mul, Float a))) memory
                             | UnaryOp (Minus, a), _ -> evalArith (UnaryOp(Minus, BinaryOp(a, Mul, y1))) memory
                             | _, UnaryOp (Minus, a) -> evalArith (UnaryOp(Minus, BinaryOp(x1, Mul, a))) memory
                             | Pi, c -> evalArith (BinaryOp(c, Mul, Pi)) memory
                             | _ -> BinaryOp(x1, Mul, y1)
    | BinaryOp(x,Div,y) -> let x1 = evalArith x memory
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
                           | UnaryOp (Minus, a), _ -> evalArith (UnaryOp (Minus,BinaryOp(a,Div,y1))) memory
                           | _, UnaryOp (Minus, a) -> evalArith (UnaryOp (Minus,BinaryOp(x1,Div,a))) memory
                           | _ -> BinaryOp(x1,Div,y1)
    | BinaryOp(x,Add, y) -> let x1 = evalArith x memory
                            let y1 = evalArith y memory
                            match x1, y1 with
                            | Num a, Num b -> Num (a + b)
                            | Num 0, _ -> y1
                            | c, d when c=d -> evalArith (BinaryOp(Num 2, Mul, x1)) memory
                            | _, Num 0 -> x1
                            | Float 0.0, _ -> y1
                            | _, Float 0.0 -> x1
                            | Float a, Num b -> Float (a + float b)
                            | Num a, Float b -> Float (float a + b)
                            | Float a, Float b -> Float (a + b)
                            | UnaryOp (Minus, a), _ -> evalArith (BinaryOp(a, Sub, y1)) memory
                            | _, UnaryOp (Minus, a) -> evalArith (BinaryOp(x1, Sub, a)) memory
                            | c, Pi -> evalArith (BinaryOp(Pi, Add, c)) memory
                            | _ -> BinaryOp(x1, Add, y1)    
    | BinaryOp(x,Sub, y) ->  let x1 = evalArith x memory
                             let y1 = evalArith y memory
                             match x1, y1 with
                             | Num a, Num b -> Num (a - b)
                             | c , d when c=d -> Num 0
                             | Num 0, _ -> evalArith (UnaryOp (Minus, y1)) memory
                             | _, Num 0 -> x1
                             | Float 0.0, _ -> evalArith (UnaryOp (Minus, y1)) memory
                             | _, Float 0.0 -> x1
                             | Float a, Num b -> Float (a - float b)
                             | Num a, Float b -> Float (float a - b)
                             | Float a, Float b -> Float (a - b)
                             | UnaryOp (Minus, a), _ -> evalArith (UnaryOp (Minus,BinaryOp(a, Add, y1))) memory
                             | _, UnaryOp (Minus, a) -> evalArith (BinaryOp(x1, Add, a)) memory
                             | _ -> BinaryOp(x1, Sub, y1)
    | UnaryOp(Minus,x) -> let x1 = evalArith x memory
                          match x1 with
                          | Num a -> Num (-a)
                          | Float a -> Float (-a)
                          | UnaryOp (Minus, a) -> a
                          | _ -> UnaryOp(Minus,x1)
    | UnaryOp (Plus, x) -> evalArith x memory
    | BinaryOp(x,Pow,y) -> let x1 = evalArith x memory
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
                           | _ -> BinaryOp(x1, Pow, y1)
    | BinaryOp(x,Mod,y) -> let x1 = evalArith x memory
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
                           | _ -> BinaryOp(x1, Mod, y1)
    | _ -> expr

/// <summary>
/// Function to eager evaluate boolean expressions with reduction rules
/// </summary>
/// <param name="expr">Boolean expression to be reduced</param>
/// <param name="memoryB">Boolean mapping of identifiers</param>
/// <param name="memoryA">Arithmetic mapping of identifiers</param>
/// <exception cref="System.Exception">Boolean invalid variable access</exception>
/// <returns>Reduced evaluation of AST boolean expression</returns>
let rec internal evalBool (expr:BoolExpr) (memoryB:Map<string, BoolExpr * int>)
    (memoryA:Map<string, ArithExpr * int>) : BoolExpr =
    match expr with 
    | B _ -> expr
    | VarB s -> try
                    let b, _ = Map.find s memoryB
                    evalBool b memoryB memoryA
                with _ -> failwith $"Unknown variable in expression - {s} has not been declared!"    
    | LogicOp(x, And, y) ->  let x1 = evalBool x memoryB memoryA
                             let y1 = evalBool y memoryB memoryA
                             match x1, y1 with
                             | B x, B y -> B (x && y)
                             | c,d when c=d -> x1
                             | c, Not(d) | Not(d), c when c=d -> B false
                             | Check (b, r), d -> LogicOp(d, And, Check (b,r))
                             | _ -> LogicOp(x1, And, y1)
    | LogicOp(x,Or,y) -> let x1 = evalBool x memoryB memoryA
                         let y1 = evalBool y memoryB memoryA
                         match x1, y1 with
                         | B a, B b -> B (a || b)
                         | c,d when c=d -> x1
                         | c, Not(d) | Not(d), c when c=d -> B true
                         | Check (b, r), d -> LogicOp(d, Or, Check (b,r))
                         | _ -> LogicOp(x1, Or, y1)
    | LogicOp(x,Xor,y) -> let x1 = evalBool x memoryB memoryA
                          let y1 = evalBool y memoryB memoryA
                          match x1, y1 with
                          | B a, B b -> B (a <> b)
                          | c,d when c=d -> B false
                          | c, Not(d) | Not(d), c when c=d -> B true
                          | Check (b, r), d -> LogicOp(d, Xor, Check (b,r))
                          | _ -> LogicOp(x1, Xor, y1)
    | Not x ->  let x1 = evalBool x memoryB memoryA
                match x1 with
                | B a -> B (not a)
                | Not a -> a
                | _ -> Not(x)
    | RelationOp(x,EQ,y) -> let x1 = evalArith x memoryA
                            let y1 = evalArith y memoryA
                            match x1, y1 with
                            | Num a, Num b -> B (a = b)
                            | Num a, Float b -> B (float a = b)
                            | Float a, Num b -> B (a = float b)
                            | Float a, Float b -> B (a = b)
                            | c, d when c=d -> B true
                            | _ -> RelationOp(x1, EQ, y1)
    | RelationOp(x,NEQ,y)-> let x1 = evalArith x memoryA
                            let y1 = evalArith y memoryA
                            match x1, y1 with
                            | Num a, Num b -> B (a <> b)
                            | Num a, Float b -> B (float a <> b)
                            | Float a, Num b -> B (a <> float b)
                            | Float a, Float b -> B (a <> b)
                            | c, d when c=d -> B false
                            | _ -> Not(RelationOp(x1, EQ, y1))
    | RelationOp(x,LT,y) -> let x1 = evalArith x memoryA
                            let y1 = evalArith y memoryA
                            match x1, y1 with
                            | Num a, Num b -> B (a < b)
                            | Num a, Float b -> B (float a < b)
                            | Float a, Num b -> B (a < float b)
                            | Float a, Float b -> B (a < b)
                            | _ -> RelationOp(x1, LT, y1)
    | RelationOp(x,LTE,y) -> let x1 = evalArith x memoryA
                             let y1 = evalArith y memoryA
                             match x1, y1 with
                             | Num a, Num b -> B (a <= b)
                             | Num a, Float b -> B (float a <= b)
                             | Float a, Num b -> B (a <= float b)
                             | Float a, Float b -> B (a <= b)
                             // a <= b = not (b < a)
                             | _ -> evalBool (Not (RelationOp(y1, LT, x1))) memoryB memoryA
    | RelationOp(x,GT,y) -> let x1 = evalArith x memoryA
                            let y1 = evalArith y memoryA
                            match x1, y1 with
                            | Num a, Num b -> B (a > b)
                            | Num a, Float b -> B (float a > b)
                            | Float a, Num b -> B (a > float b)
                            | Float a, Float b -> B (a > b)
                            // a > b = b < a
                            | _ -> evalBool (RelationOp(y1, LT, x1)) memoryB memoryA
    | RelationOp(x,GTE,y) -> let x1 = evalArith x memoryA
                             let y1 = evalArith y memoryA
                             match x1, y1 with
                             | Num a, Num b -> B (a >= b)
                             | Num a, Float b -> B (float a >= b)
                             | Float a, Num b -> B (a >= float b)
                             | Float a, Float b -> B (a >= b)
                             // a >= b = not (a < b)
                             | _ -> evalBool (Not (RelationOp(x1, LT, y1))) memoryB memoryA
    | Check _ -> expr

/// <summary>
/// Function to optimize statement AST by reducing expressions and building memory
/// </summary>
/// <param name="st">Abstract Syntax Tree of Statement for optimization (AST.Statement)</param>
/// <param name="memArith">Initial arithmetic variable memory</param>
/// <param name="memBool">Initial boolean variable memory</param>
/// <param name="no">Ordering of assignments in circuit</param>
/// <returns>Tuple of arithmetic and boolean variable memories and optimized Statement</returns>    
let rec private optimizeStatement (st:Statement) (memArith:Map<string, ArithExpr * int>)
    (memBool:Map<string, BoolExpr * int>) (no:int) : int * Map<string, ArithExpr * int>
    * Map<string, BoolExpr * int> * Statement =
    match st with
    | Assign(s, value) -> let value1 = evalArith value memArith
                          (no+1), memArith.Add(s, (value1, no)), memBool, Assign(s, value1)
    | AssignB(s, value) -> let value1 = evalBool value memBool memArith
                           (no+1), memArith, memBool.Add(s, (value1, no)), AssignB(s, value1)
    | Condition(b, statement) -> let b1 = evalBool b memBool memArith
                                 let no1, memArith1, memBool1, statement1 = optimizeStatement statement memArith memBool no
                                 no1, memArith1, memBool1, Condition(b1, statement1)
    | _ -> no, memArith, memBool, st
    
/// <summary>
/// Function to optimize the flow AST by reducing each statement in the list
/// </summary>
/// <param name="expr">Abstract Syntax Tree of circuit flow for optimization (AST.Elements)</param>
/// <param name="memArith">Initial arithmetic variable memory</param>
/// <param name="memBool">Initial boolean variable memory</param>
/// <param name="no">Ordering of assignments in circuit</param>
/// <returns>Tuple of arithmetic and boolean variable memories and optimized Flow</returns>
let rec internal optimizeCircuit (expr:Statement list) (memArith:Map<string, ArithExpr * int>)
    (memBool:Map<string, BoolExpr * int>) (no:int) : int * Map<string, ArithExpr * int>
    * Map<string, BoolExpr * int> * Statement list =     
    match expr with
    | head::tail -> let no1, memArith1, memBool1, head1 = optimizeStatement head memArith memBool no
                    let no2, memArith2, memBool2, tail1 = optimizeCircuit tail memArith1 memBool1 no1
                    no2, memArith2, memBool2, head1::tail1
    | [] -> no, memArith, memBool, []
    
    
/// <summary>
/// Function to validate the target register of quantum operators.
/// </summary>
/// <param name="bit">Target register to be validated</param>
/// <param name="flag">Type of register expected</param>
/// <param name="memory">Memory mapping of corresponding types</param>
/// <exception cref="System.Exception">Invalid allocation of register</exception>
let rec private validateRegister (bit:Bit) (flag:string) (memory:Map<string, int * int>):unit =
    match bit with
    | BitA(s, i) ->
                try
                    let alloc, _ = Map.find s memory
                    if (i<0 || alloc <= i) then failwith "Overflow of register"
                with _ -> 
                    failwith $"{flag} bit register {s}[{i}] has not been allocated!"
    | BitS s -> if Map.containsKey s memory then ()
                else failwith $"{flag} bit register {s} has not been allocated!"

/// <summary>
/// Function to peek inside boolean expression and extract classical registers used.
/// </summary>
/// <param name="b">Boolean expression</param>
/// <param name="acc">Accumulator set of bits</param>
let rec private peekRegister (b:BoolExpr) (acc:Set<Bit>):Set<Bit> =
    match b with
    | LogicOp(b1, _, b2) ->
                        let acc2 = peekRegister b1 acc
                        peekRegister b2 acc2
    | Not b -> peekRegister b acc
    | Check(bit, _) -> Set.add bit acc
    | _ -> Set.empty 

/// <summary>
/// Function to analyze the semantics of a Statement AST. Operators have to
/// be applied on quantum registers only, the measurement result is stored on classical register.
/// </summary>
/// <param name="st">Statement to be analyzed semantically (AST.Statement)</param>
/// <param name="memory">Record mappings of defined identifiers</param>
/// <exception cref="System.Exception">Invalid register definition (semantic)</exception>
let rec analyseStatement (st:Statement) (memory:Memory):unit =
    match st with
    | Condition(b, st) ->
              // Find used classical registers
              let c_set = peekRegister b Set.empty
              // Validate the found set of registers
              Set.iter (fun x ->
                    validateRegister x "Classical" memory.Classical) c_set
              // Analyze the following statement
              analyseStatement st memory
    | Measure(q, s) ->
                validateRegister q "Quantum" memory.Quantum
                validateRegister s "Classical" memory.Classical
    | Reset q | Barrier q | Unitary(_, _, _, q)-> validateRegister q "Quantum" memory.Quantum
    | UnaryGate(_, q) -> validateRegister q "Quantum" memory.Quantum
    | ParamGate(_, _, q) -> validateRegister q "Quantum" memory.Quantum
    | BinaryGate(_, q1, q2) -> validateRegister q1 "Quantum" memory.Quantum
                               validateRegister q2 "Quantum" memory.Quantum
    | BinaryParamGate(_, _, q1, q2) ->
                validateRegister q1 "Quantum" memory.Quantum
                validateRegister q2 "Quantum" memory.Quantum
    | Toffoli(q1, q2, q3) ->
                validateRegister q1 "Quantum" memory.Quantum
                validateRegister q2 "Quantum" memory.Quantum
                validateRegister q3 "Quantum" memory.Quantum
    | _ -> ()     

/// <summary>
/// Function to analyze the semantics of the defined flow AST (iterating statements)
/// </summary>
/// <param name="ast">Flow AST to be analyzed semantically</param>
/// <param name="memory">Record mappings of defined identifiers</param>
/// <exception cref="System.Exception">Invalid register definition (semantic)</exception>
let rec internal semanticAnalyzer (ast:Statement list) (memory:Memory):unit =
    match ast with
    | head::tail ->
                // Analyze the first operator
                analyseStatement head memory
                // Analyze the rest of the list
                semanticAnalyzer tail memory
    | [] -> ()
            
            
    