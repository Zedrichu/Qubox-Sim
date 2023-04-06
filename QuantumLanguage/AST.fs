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

open QuantumLanguage.VisitorPattern
open QuantumLanguage.Tags

/// Disjoint union type of arithmetic operators
type AOp = // Arithmetic operators
  | Add | Sub | Mul
  | Div | Pow | Mod
  | Plus | Minus
  override this.ToString ()=
    match this with
    | Add -> "+"
    | Sub -> "-"
    | Mul -> "*"
    | Div -> "/"
    | Pow -> "^"
    | Mod -> "%"
    | Plus -> ""
    | Minus -> "-"

/// Discriminated type of basic arithmetic expressions
type ArithExpr =
  | Pi // Mathematical π=3.141592...
  | Num of int
  | Float of float
  | VarA of string
  | BinaryOp of (ArithExpr * AOp * ArithExpr)
  | UnaryOp of (AOp * ArithExpr)
  member this.Accept (visitor: IVisitor<ArithExpr, 'a>) = (this :> IVisitable<ArithExpr>).Accept visitor
  interface IVisitable<ArithExpr> with
    member this.Accept (visitor: IVisitor<ArithExpr, 'a>) = visitor.Visit this
    
  override this.ToString () =
    match this with
    | Pi -> "Pi"
    | Num i -> i.ToString()
    | Float f -> f.ToString()
    | VarA s -> s
    | BinaryOp (a1, op, a2) -> $"({a1} {op} {a2})"
    | UnaryOp (op, a) -> $"({op} {a})"
  
/// Tagged type of quantum/classical bit declarations
type Bit =
  | BitS of string
  | BitA of (string * int)
  | BitSeq of (Bit * Bit)
  override this.ToString () =
    match this with
    | BitS s -> s
    | BitA (s, i) -> $"%s{s}[%d{i}]"
    | BitSeq (b1, b2) -> $"%s{b1.ToString()}, %s{b2.ToString()}"
  
  
/// Tagged type of measurement results
type Result =
  | Click // +1 Eigenspace (spin-up, |0⟩)
  | NoClick // -1 Eigenspace (spin-down, |1⟩)
  override this.ToString () =
    match this with
    | Click -> "Click"
    | NoClick -> "NoClick"
  
/// Disjoint union type of logical operators
type BOp = // Boolean operators
  | And | Or | Xor
  override this.ToString ()=
    match this with
    | And -> "and"
    | Or -> "or"
    | Xor -> "xor"
    
/// Disjoint union type of relational operators
type ROp = // Relational operators
  | EQ | NEQ
  | GT | GTE
  | LT | LTE
  override this.ToString ()=
    match this with
    | EQ -> "=="
    | NEQ -> "!="
    | GT -> ">"
    | GTE -> ">="
    | LT -> "<"
    | LTE -> "<="
  
/// Discriminated type of basic logical expression
type BoolExpr = 
  | B of bool
  | VarB of string
  | LogicOp of (BoolExpr * BOp * BoolExpr)
  | RelationOp of (ArithExpr * ROp * ArithExpr)
  | Not of BoolExpr
  | Check of (Bit * Result) // check measurement result
  member this.Accept (visitor: IVisitor<BoolExpr, 'a>) = (this :> IVisitable<BoolExpr>).Accept visitor
  interface IVisitable<BoolExpr> with
    member this.Accept (visitor: IVisitor<BoolExpr, 'a>) = visitor.Visit this
  override this.ToString () =
    match this with
    | B b -> b.ToString().ToLower()
    | VarB s -> "~"+s
    | LogicOp (b1, op, b2) -> $"({b1} {op} {b2})"
    | RelationOp (a1, op, a2) -> $"({a1} {op} {a2})"
    | Not b -> $"not ({b})"
    | Check (cb, r) -> $"{cb} |> {r}"
    
  
///Tagged type of errors in QuLang module (Accumulate grammar error (syntax/semantics/evaluations))
type Error =
  /// Signals successful language processing
  | Success
  //| Warning of string // Warning: message
  /// Signals a syntax error in the input - invalid token at specific line/column
  | SyntaxError of (string * int * int)
  /// Signals a semantic error with message
  | SemanticError of string
  /// Signals an evaluation error with message
  | EvaluationError of string
  override this.ToString () =
    match this with
    | Success -> "Success"
    | SyntaxError (msg, line, col) -> $"Syntax error: %s{msg} at line %d{line}, column %d{col}"
    | SemanticError msg -> $"Semantic error: %s{msg}"
    | EvaluationError msg -> $"Evaluation error: %s{msg}"

/// Discriminated type of quantum gates and operators
type Statement =
  /// Arithmetic variable assignment
  | Assign of (string * ArithExpr)
  /// Logical variable declaration
  | AssignB of (string * BoolExpr)
  /// Conditional quantum gate application
  | Condition of (BoolExpr * Statement)
  /// Measurement of qubit on classical bit (Z-basis)
  | Measure of (Bit * Bit)
  /// Reset of qubit to |0⟩ - computational basis
  | Reset of Bit // Reset bit to |0⟩
  /// Circuit barrier (isolate gates & optimizations)
  | Barrier of Bit
  /// Phase disk computation on all qubits
  | PhaseDisk
  /// Unary quantum gates
  | UnaryGate of (UTag * Bit)
  /// Binary quantum gates
  | BinaryGate of (BTag * Bit * Bit) 
  /// Unary parametric quantum gates
  | ParamGate of (PTag * ArithExpr * Bit)
  /// Binary parametric quantum gates
  | BinaryParamGate of (BPTag * ArithExpr *  Bit * Bit)
  /// Unitary triple-parametric quantum gate
  | Unitary of (ArithExpr * ArithExpr * ArithExpr * Bit)
  /// Control-control-NOT gate (3-way entangler)
  | Toffoli of (Bit * Bit * Bit)
  member this.Accept (visitor: IVisitor<Statement, 'a>) = (this :> IVisitable<Statement>).Accept visitor
  interface IVisitable<Statement> with
    member this.Accept (visitor: IVisitor<Statement, 'a>) = visitor.Visit this

/// Type of quantum/classical register allocation
type Allocation = AllocQC of (Bit * Bit)
/// Type of quantum circuit AST as list of Statements
type Schema = Flow of Statement list
/// Type of program as unified allocation and circuit AST
type Circuit = Allocation * Schema
   
/// <summary>
/// Record type to hold the established memory bindings (arithmetic/boolean/classical/quantum)
/// </summary>
type Memory =
   { Arithmetic: Map<string, ArithExpr * int>;
     Boolean: Map<string, BoolExpr * int>; 
     Quantum: Map<string, int * int>;
     Classical: Map<string, int * int> }
   static member empty = { Arithmetic = Map.empty; Boolean = Map.empty;
                           Quantum = Map.empty; Classical = Map.empty; }
   
   member this.SetArithmetic map = { this with Arithmetic = map }
   member this.SetBoolean map = { this with Boolean = map }
   member this.SetQuantumClassic qmap cmap = { this with Quantum = qmap; Classical = cmap }
   member this.CountQuantum = (Map.fold (fun acc _ (value, _) -> acc+value) 0 this.Quantum)
   member this.CountClassical = (Map.fold (fun acc _ (value, _) -> acc+value) 0 this.Classical)
   member this.GetOrder (bit:Bit) =
     match bit with
     | BitA(s, i) -> let _, order = Map.find s this.Quantum in order + i
     | BitS s -> let _, order = Map.find s this.Quantum in order
     | BitSeq _ -> failwith "Invalid request of order for bit sequence!"