/// <summary>
/// Interface to library assisting the parsing/compilation process from QuLang user-input
/// </summary>
module QuantumLanguage.Handler
(* F#
 -*- coding: utf-8 -*-
Quantum Language Handler

Description: Library to assist the parsing/compilation process from QuLang user-input.

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
    let lexbuffer = Lexing.LexBuffer<_>.FromString code // Create an input stream
    try
        // Create a TOKEN stream using the Lexer rules on the input stream
        // Pass the TOKEN stream to the Parser to obtain the AST
        let ast = Parser.start Lexer.tokenize lexbuffer
        (ast, Success)
    //Undefined string TOKEN encountered - syntax analyzer 
    with e ->
              let line = lexbuffer.EndPos.pos_lnum + 1
              let column = lexbuffer.EndPos.pos_cnum - lexbuffer.EndPos.pos_bol
              let token = Lexing.LexBuffer<_>.LexemeString lexbuffer
              printfn $"Parse error in program at : Line %i{line},
                %i{column}, Unexpected token: %A{token}"
              ((NOP, NOP), SyntaxError(token, line, column))

let getMenuInput() = Int32.TryParse(Console.ReadLine())

let getInputCode() = Console.ReadLine()

let printInnerMenu() =
    printfn "Menu:"
    printfn "1. AST optimizer"
    printfn "2. Evaluate AST"
    printfn "3. Compiler Q#"
    printfn "4. Translator QuLang (Prettify)"
    printfn "5. Back to main menu"
    printf "Enter your choice:"

let rec execute (ast:operator*operator) =
    printInnerMenu()
    match getMenuInput() with
    | true, 1 -> execute(ast)
    | true, 2 -> execute(ast)
    | true, 3 -> let reg, ops = ast
                 let qsharp = Compiler.compileOperator (Order(reg, ops))
                 printfn $"%A{qsharp}"
                 execute(ast)
    | true, 4 -> let reg, ops = ast
                 let quLang = Translator.transOperator (Order(reg, ops))                
                 printfn $"%A{quLang}"
                 execute(ast)
    | true, 5 -> ()
    | _ -> execute(ast)
    

let printMainMenu() =
    printfn "Menu: "
    printfn "1. Parse to AST"
    printfn "2. Quit"
    printf "Enter your choice:"
    
    
let rec mainMenu () =
    printfn ""
    printMainMenu()
    match getMenuInput() with
    | true, 1 -> let code = Console.ReadLine()
                 let ast, err = ParseQuLang code
                 printfn $"%A{ast} Status:%A{err}"
                 execute(ast)
    | true, 2 -> ()
    | _ -> mainMenu()

// [<EntryPoint>]
//     let main argv =
//         mainMenu()
//         0 // return an integer exit code