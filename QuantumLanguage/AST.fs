/// <summary>
/// Declaration module containing the types required to build the abstract syntax tree of QuLang.
/// </summary>
module public QuantumLanguage.AST
(* F#
 -*- coding: utf-8 -*-
Quantum Abstract Syntax Tree

Description: Declaration module containing the types required to build the abstract syntax tree.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 21/02/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*)

open System.Text.Json.Serialization.Metadata
open VisitorPattern

/// Discriminated type of basic arithmetic expressions
type arithExpr =
  | Pi // Mathematical π=3.141592...
  | Num of int
  | Float of float
  | VarA of string
  | TimesExpr of (arithExpr * arithExpr)
  | DivExpr of (arithExpr * arithExpr)
  | PlusExpr of (arithExpr * arithExpr)
  | MinusExpr of (arithExpr * arithExpr)
  | PowExpr of (arithExpr * arithExpr)
  | ModExpr of (arithExpr * arithExpr)
  | UPlusExpr of arithExpr
  | UMinusExpr of arithExpr
  interface IVisitable<arithExpr> with
    member this.Accept (visitor: IVisitor<arithExpr, 'a>) = visitor.Visit this
  override this.ToString () =
    match this with
    | Pi -> "π"
    | Num i -> i.ToString()
    | Float f -> f.ToString()
    | VarA s -> s
    | TimesExpr (a1, a2) -> $"{a1.ToString()} * {a2.ToString}"
    | DivExpr (a1, a2) -> $"%s{a1.ToString()} / %s{a2.ToString()}"
    | PlusExpr (a1, a2) -> $"%s{a1.ToString()} + %s{a2.ToString()}"
    | MinusExpr (a1, a2) -> $"%s{a1.ToString()} - %s{a2.ToString()}"
    | PowExpr (a1, a2) -> $"%s{a1.ToString()} ^ %s{a2.ToString()}"
    | ModExpr (a1, a2) -> $"%s{a1.ToString()} % %s{a2.ToString()}"
    | UPlusExpr a -> $"+%s{a.ToString()}"
    | UMinusExpr a -> $"-%s{a.ToString()}"
  
/// Tagged type of quantum/classical bit declarations
type bit =
  | BitS of string
  | BitA of (string * int)
  | BitSeq of (bit * bit)
  override this.ToString () =
    match this with
    | BitS s -> s
    | BitA (s, i) -> $"%s{s}[%d{i}]"
    | BitSeq (b1, b2) -> $"%s{b1.ToString()}, %s{b2.ToString()}"
  
  
/// Tagged type of measurement results
type result =
  | Click // +1 Eigenspace (spin-up, |0⟩)
  | NoClick // -1 Eigenspace (spin-down, |1⟩)
  
  
/// Discriminated type of basic boolean expression
type boolExpr = 
  | Bool of bool
  | VarB of string
  | LogAnd of (boolExpr * boolExpr)
  | LogOr of (boolExpr * boolExpr)
  | Neg of boolExpr
  | Check of (bit * result) // check measurement result
  | Equal of (arithExpr * arithExpr)
  | NotEqual of (arithExpr * arithExpr)
  | Greater of (arithExpr * arithExpr)
  | GreaterEqual of (arithExpr * arithExpr)
  | Less of (arithExpr * arithExpr)
  | LessEqual of (arithExpr * arithExpr)
  interface IVisitable<boolExpr> with
    member this.Accept (visitor: IVisitor<boolExpr, 'a>) = visitor.Visit this
  override this.ToString () =
    match this with
    | Bool b -> b.ToString()
    | VarB s -> s
    | LogAnd (b1, b2) -> $"%s{b1.ToString()} && %s{b2.ToString()}"
    | LogOr (b1, b2) -> $"%s{b1.ToString()} || %s{b2.ToString()}"
    | Neg b -> $"not (%s{b.ToString()})"
    | Check (bit, result) -> $"(%s{bit.ToString()} |> %s{result.ToString()})"
    | Equal (a1, a2) -> $"%s{a1.ToString()} == %s{a2.ToString()}"
    | NotEqual (a1, a2) -> $"%s{a1.ToString()} != %s{a2.ToString()}"
    | Greater (a1, a2) -> $"%s{a1.ToString()} > %s{a2.ToString()}"
    | GreaterEqual (a1, a2) -> $"%s{a1.ToString()} >= %s{a2.ToString()}"
    | Less (a1, a2) -> $"%s{a1.ToString()} < %s{a2.ToString()}"
    | LessEqual (a1, a2) -> $"%s{a1.ToString()} <= %s{a2.ToString()}"
    
  
///Tagged type of errors in QuLang module (Accumulate grammar error (syntax/semantics/evaluations))
type error =
  | Success // No error
  | SyntaxError of (string * int * int) // Syntax error: invalid token at specific line/column
  | SemanticError of string // Semantic error: message
  | EvaluationError of string // Evaluation error: message
  interface IVisitable<error> with
    member this.Accept (visitor: IVisitor<error, 'a>) = visitor.Visit this
  member this.ToString =
    match this with
    | Success -> "Success"
    | SyntaxError (msg, line, col) -> $"Syntax error: %s{msg} at line %d{line}, column %d{col}"
    | SemanticError msg -> $"Semantic error: %s{msg}"
    | EvaluationError msg -> $"Evaluation error: %s{msg}"
  
/// Discriminated type of quantum gates and operators
type operator =
  | NOP // No operation
  | AllocQC of (bit * bit) // Allocate arrays/sequences of qubits/cbits
  | Measure of (bit * bit) // Computational measurement of qubit on classical bit
  | AssignB of (string * boolExpr)
  | Assign of (string * arithExpr) // Arithmetic variable declaration
  | Order of (operator * operator) // Operator linker
  | Reset of bit // Reset bit to |0⟩
  | Condition of (boolExpr * operator)
  | Barrier of bit // Separate optimizations
  | PhaseDisk // Phase disk operation on all qubits
  | H of bit // Hadamard
  | I of bit // Identity
  | X of bit // Pauli X (NOT)
  | Y of bit // Pauli Y
  | Z of bit // Pauli Z
  | TDG of bit // T-dagger
  | SDG of bit // S-dagger
  | S of bit // S gate
  | T of bit // T gate
  | SX of bit // Square root X (square NOT)
  | SXDG of bit // Square root X - dagger
  | P of (arithExpr * bit) // Phase gate
  | RZ of (arithExpr * bit) // Rotation Z
  | RY of (arithExpr * bit) // Rotation Y
  | RX of (arithExpr * bit) // Rotation X
  | U of (arithExpr * arithExpr * arithExpr * bit) // Unitary parametric
  | CNOT of (bit * bit) // Control-NOT gate (entangler)
  | CCX of (bit * bit * bit) // Control-control-NOT gate (3-way entangler)
  | SWAP of (bit * bit) // SWAP gate
  | RXX of (arithExpr * bit * bit) // Rotation X-X symmetric
  | RZZ of (arithExpr * bit * bit) // Rotation Z-Z symmetric
  interface IVisitable<operator> with
    member this.Accept (visitor: IVisitor<operator, 'a>) = visitor.Visit this
  member this.DestructSingle () =
    match this with
    | H bit -> ("H", bit)
    | I bit -> ("I", bit)
    | X bit -> ("X", bit)
    | Y bit -> ("Y", bit)
    | Z bit -> ("Z", bit)
    | TDG bit -> ("TDG", bit)
    | SDG bit -> ("SDG", bit)
    | S bit -> ("S", bit)
    | T bit -> ("T", bit)
    | SX bit -> ("SX", bit)
    | SXDG bit -> ("SXDG", bit)
    | _ -> "", BitS ""
    
     
  
/// <summary>
/// Record type to hold the established memory bindings (arithmetic/boolean/classical/quantum)
/// </summary>
type Memory =
   { Arithmetic: Map<string,arithExpr>;
     Boolean: Map<string, boolExpr>; 
     Quantum: Map<string, int * int>;
     Classical: Map<string, int * int> }
   static member empty = { Arithmetic = Map.empty; Boolean = Map.empty;
                           Quantum = Map.empty; Classical = Map.empty; }
   
   member this.SetArithmetic map = { this with Arithmetic = map }
   member this.SetBoolean map = { this with Boolean = map }
   member this.SetQuantumClassic qmap cmap = { this with Quantum = qmap; Classical = cmap }
   member this.CountQuantum = (Map.fold (fun acc key (value, _) -> acc+value) 0 this.Quantum)
   member this.CountClassical = (Map.fold (fun acc key (value, _) -> acc+value) 0 this.Classical)
   member this.GetOrder (bit:bit) =
     match bit with
     | BitA(s, i) -> let _, order = Map.find s this.Quantum in order + i
     | BitS s -> let _, order = Map.find s this.Quantum in order
     | BitSeq _ -> failwith "Invalid request of order for bit sequence!"