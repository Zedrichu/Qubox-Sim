/// <summary>
/// Interface to library assisting the parsing/compilation process from QuLang user-input
/// </summary>
module public QuantumLanguage.Handler
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

let public parseQuLang (code:string):(operator * operator) * error =
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

let public translateAST (ast:operator * operator) =
    let reg, ops = ast
    Translator.transOperator (Order(reg, ops))

let public compileAST (ast:operator * operator) =
    let reg, ops = ast
    Compiler.compileOperator (Order(reg, ops))
    
let public analyzeSemantics (ast:operator * operator): Memory * error =
    let reg, ops = ast
    try
        let quantumMap, classicMap = Interpreter.allocateBits reg
        let memory = Memory.empty.setQuantumClassic quantumMap classicMap
        Interpreter.semanticAnalyzer ops memory
        memory, Success        
    with e -> Memory.empty, SemanticError e.Message

let public optimizeAST (ast:operator) (memory:Memory):(operator * Memory * error) =
    try
        let optimized = Interpreter.optimizeOperator ast Map.empty Map.empty
        let memA, memB, optimAST = optimized
        let updMemory = (memory.setArithmetic memA).setBoolean memB
        optimAST, updMemory, Success
    with e -> ast, memory, EvaluationError e.Message
    
let private getMenuInput() = Int32.TryParse(Console.ReadLine())

let private getInputCode() = Console.ReadLine()

let private printInnerMenu() =
    printfn "Menu:"
    printfn "1. Semantic Analysis"
    printfn "2. Optimize AST"
    printfn "3. Compiler Q#"
    printfn "4. Translator QuLang (Prettify)"
    printfn "5. Back to main menu"
    printf "Enter your choice:"

let rec private execute (ast:operator*operator) =
    printInnerMenu()
    match getMenuInput() with
    | true, 1 -> let memory, err = analyzeSemantics ast
                 printfn $"Memory:%A{memory}"
                 printfn $"Status:%A{err}"
                 execute(ast)
    | true, 2 -> let reg, ops = ast
                 let optimized, updMemory, err = optimizeAST ops Memory.empty
                 let ast = (reg, optimized)
                 printfn $"AST:%A{ast}" 
                 printfn $"Memory:%A{updMemory}"
                 printfn $"Status:%A{err}"
                 execute(ast)
    | true, 3 -> let qSharp = compileAST ast
                 printfn $"%A{qSharp}"
                 execute(ast)
    | true, 4 -> let quLang = translateAST ast              
                 printfn $"%A{quLang}"
                 execute(ast)
    | true, 5 -> ()
    | _ -> execute(ast)
    

let private printMainMenu() =
    printfn "Menu: "
    printfn "1. Parse to AST"
    printfn "2. Quit"
    printf "Enter your choice:"
    
    
let rec mainMenu () =
    printfn ""
    printMainMenu()
    match getMenuInput() with
    | true, 1 -> let code = Console.ReadLine()
                 let ast, err = parseQuLang code
                 printfn $"%A{ast} Status:%A{err}"
                 execute(ast)
                 mainMenu()
    | true, 2 -> ()
    | _ -> mainMenu()

[<EntryPoint>]
    let main argv =
        mainMenu()
        0 // return an integer exit code