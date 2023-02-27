module QuantumLanguage.Handler
(* F#
 -*- coding: utf-8 -*-
Quantum Language Handler

Description: Library to assist the parsing/compilation process of QuLang from user-input

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 21/02/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.2
@__Status --> DEV
*)

open System
open FSharp.Text
open QuantumLanguage.AST

let ParseQuLang code =
    let lexbuffer = Lexing.LexBuffer<_>.FromString code
    try  
        let output = Parser.start Lexer.tokenize lexbuffer
        output
    //Undefined string TOKEN encountered   
    with e -> printfn $"Parse error in program at : Line %i{lexbuffer.EndPos.pos_lnum+ 1},
                %i{lexbuffer.EndPos.pos_cnum - lexbuffer.EndPos.pos_bol},
                    Unexpected token: %s{Lexing.LexBuffer<_>.LexemeString lexbuffer}"
              (AST.Error "Parse Error", AST.Error "Parse Error")

let getMenuInput() = Int32.TryParse(Console.ReadLine())

let getInputCode() = Console.ReadLine()



// [<EntryPoint>]
// let main (args) =
//     printMenu() 
//     match getInput() with
//     | true, 1 -> printfn "Good choice"
//                  0
//     | _ -> printfn "Fucked up!"
//            0