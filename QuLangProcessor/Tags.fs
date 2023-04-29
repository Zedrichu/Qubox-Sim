/// <summary>
/// Declaration module containing the tags for different types of quantum gates
/// </summary>
module public QuLangProcessor.Tags
(** F#
 -*- coding: utf-8 -*-
Tags

Description: Quantum gate tags to distinguish types

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 11/04/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*)


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
  