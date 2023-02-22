/* F#
 -*- coding: utf-8 -*-
Quantum Language Handler

Description: Library to assist the parsing/compilation process of QuLang from user-input

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 21/02/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.2
@__Status --> DEV
*/

namespace QuantumLanguage

module QuantumHandler =

open System
open QuantumAST
let ParseQuCode code =
    let lexbuf = Lexing.LexBuffer<_>.FromString code
    let output = QuantumParser.start QuantumLexer.tokenize lexbuf
    output

Console.ReadKey(true) |> ignore