/// <summary>
/// Declaration module containing the types required to build the abstract syntax tree of QuLang.
/// </summary>
module QuantumLanguage.AST
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
    member this.Accept (visitor: IVisitor<arithExpr>) = visitor.Visit this
  
/// Tagged type of quantum/classical bit declarations
type bit =
  | BitS of string
  | BitA of (string * int)
  | BitSeq of (bit * bit)
  interface IVisitable<bit> with
    member this.Accept (visitor: IVisitor<bit>) = visitor.Visit this

/// Tagged type of measurement results
type result =
  | Click // +1 Eigenspace (spin-up, |0⟩)
  | NoClick // -1 Eigenspace (spin-down, |1⟩)
  interface IVisitable<result> with
    member this.Accept (visitor: IVisitor<result>) = visitor.Visit this
  
  
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
    member this.Accept (visitor: IVisitor<boolExpr>) = visitor.Visit this
  
///Tagged type of errors in QuLang module (Accumulate grammar error (syntax/semantics/evaluations))
type error =
  | Success // No error
  | SyntaxError of (string * int * int) // Syntax error: invalid token at specific line/column
  | SemanticError of string // Semantic error: message
  | EvaluationError of string // Evaluation error: message
  
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
  
/// <summary>
/// Record type to hold the established memory bindings (arithmetic/boolean/classical/quantum)
/// </summary>
type Memory = {
    Arithmetic: Map<string,arithExpr>;
    Boolean: Map<string, boolExpr>; 
    Quantum: Map<string, int>;
    Classical: Map<string, int>;
}