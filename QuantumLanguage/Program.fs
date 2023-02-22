module QuantumLanguage.QuantumHandler
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
    //Undefined string encountered   
    with e -> printfn "Parse error in program at : Line %i, %i, Unexpected token: %s" (lexbuffer.EndPos.pos_lnum+ 1) 
                        (lexbuffer.EndPos.pos_cnum - lexbuffer.EndPos.pos_bol) (Lexing.LexBuffer<_>.LexemeString lexbuffer)
              (AST.Barrier (BitS "a"), AST.Barrier (BitS "a"))

let getInput() = Int32.TryParse(Console.ReadLine())

let printMenu () = 
    printfn "Menu: "
    printfn "1. Quit"
    printf "Enter your choice:"

// [<EntryPoint>]
// let main (args) =
//     printMenu() 
//     match getInput() with
//     | true, 1 -> printfn "Good choice"
//                  0
//     | _ -> printfn "Fucked up!"
//            0