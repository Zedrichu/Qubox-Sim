module public QuLangProcessor.Tags

/// Disjoint union type of unary gates
type UTag =
  | H | ID | X | Y | Z
  | TDG | SDG | S | T
  | SX | SXDG 

/// Disjoint union type of binary gates
type BTag =
  | SWAP | CNOT | CH | CS 

/// Disjoint union type of binary-parametric gates
type BPTag =
  | RXX | RYY | RZZ
  
/// Disjoint union type of unary-parametric gates
type PTag =
  | RX | RY | RZ | P 