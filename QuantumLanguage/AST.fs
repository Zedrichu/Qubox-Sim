module QuantumLanguage.AST
(* F#
 -*- coding: utf-8 -*-
Quantum Abstract Syntax Tree

Description: Declaration file containing the types required to build the abstract syntax tree.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 21/02/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*)

/// Type of basic arithmetic expressions
type arithExpr =
  | Pi // Mathematical π=3.141592...
  | Num of int
  | Float of float
  | StrA of string
  | TimesExpr of (arithExpr * arithExpr)
  | DivExpr of (arithExpr * arithExpr)
  | PlusExpr of (arithExpr * arithExpr)
  | MinusExpr of (arithExpr * arithExpr)
  | UPlusExpr of arithExpr
  | UMinusExpr of arithExpr
  
/// Type of quantum/classical bit declarations
type bit =
  | BitS of string
  | BitA of (string * int)
  | BitSeq of (bit * bit)

/// Type of measurement results
type result =    
  | Click // +1 Eigenspace (spin-up, |0⟩)
  | NoClick // -1 Eigenspace (spin-down, |1⟩)
  
/// Type of basic boolean expression
type boolExpr = 
  | Bool of bool
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
  
  
/// Type of quantum gates and operators
type operator =
  | NOP // No operation
  // Include line and column of error
  | Error of string // Accumulate grammar error (syntax/semantics/evaluations)
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