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

/// <summary>
/// Interface method to parse QuLang code to AST based on the grammar rules
/// </summary>
/// <param name="code">String in QuLang format awaiting parsing</param>
/// <returns>Tuple of AST for allocation and operation and error tag</returns>
let public parseQuLang (code:string):(operator * operator) * error =
    let lexbuffer = Lexing.LexBuffer<_>.FromString code // Create an input stream
    try
        // Create a TOKEN stream using the Lexer rules on the input stream
        // Pass the TOKEN stream to the Parser to obtain the AST
        let ast = Parser.start Lexer.tokenize lexbuffer
        (ast, Success)
    //Undefined string TOKEN encountered - syntax analyzer 
    with e ->
              // Collect error triggers and information
              let line = lexbuffer.EndPos.pos_lnum + 1
              let column = lexbuffer.EndPos.pos_cnum - lexbuffer.EndPos.pos_bol
              let token = Lexing.LexBuffer<_>.LexemeString lexbuffer
              printfn $"Parse error in program at : Line %i{line},
                %i{column}, Unexpected token: %A{token}"
              // Return an empty AST and the syntax error
              ((NOP, NOP), SyntaxError(token, line, column))

/// <summary>
/// Interface method to parse boolean expression from string format to AST
/// /// </summary>
/// <param name="expr">String awaiting parsing</param>
/// <returns>Tuple of boolean AST and error tag</returns>
let public parseBool (expr:string):(boolExpr * error) =
    let lexbuffer = Lexing.LexBuffer<_>.FromString expr // Create an input stream
    try
        // Create a TOKEN stream using the Lexer rules on the input stream
        // Pass the TOKEN stream to the Parser to obtain the AST
        let ast = Parser.startBool Lexer.tokenize lexbuffer
        (ast, Success)
    //Undefined string TOKEN encountered - syntax analyzer 
    with e ->
              // Collect error triggers and information
              let line = lexbuffer.EndPos.pos_lnum + 1
              let column = lexbuffer.EndPos.pos_cnum - lexbuffer.EndPos.pos_bol
              let token = Lexing.LexBuffer<_>.LexemeString lexbuffer
              printfn $"Parse error in program at : Line %i{line},
                %i{column}, Unexpected token: %A{token}"
              // Return an empty AST and the syntax error
              (Bool false, SyntaxError(token, line, column))
              
/// <summary>
/// Interface method to parse arithmetic expression from string format to AST
/// </summary>
/// <param name="expr">String awaiting parsing</param>
/// <returns>Tuple of arithmetic AST and error tag</returns>
let public parseArith (expr:string):(arithExpr * error) =
    let lexbuffer = Lexing.LexBuffer<_>.FromString expr // Create an input stream
    try
        // Create a TOKEN stream using the Lexer rules on the input stream
        // Pass the TOKEN stream to the Parser to obtain the AST
        let ast = Parser.startArith Lexer.tokenize lexbuffer
        (ast, Success)
    //Undefined string TOKEN encountered - syntax analyzer 
    with e ->
              // Collect error triggers and information
              let line = lexbuffer.EndPos.pos_lnum + 1
              let column = lexbuffer.EndPos.pos_cnum - lexbuffer.EndPos.pos_bol
              let token = Lexing.LexBuffer<_>.LexemeString lexbuffer
              printfn $"Parse error in program at : Line %i{line},
                %i{column}, Unexpected token: %A{token}"
              // Return an empty AST and the syntax error
              (Num 0, SyntaxError(token, line, column))

/// <summary>
/// Interface method to translate generated AST backwards to QuLang code
/// </summary>
/// <param name="ast">Generated AST for translation</param>
/// <returns>String of prettified QuLang code</returns>
let public translateAST (ast:operator * operator) =
    let reg, ops = ast
    Translator.transOperator (Order(reg, ops))

/// <summary>
/// Interface method to compile generated AST to Q# code
/// </summary>
/// <param name="ast">Generated AST for compilation</param>
/// <returns>String of Q# code</returns>
let public compileAST (ast:operator * operator) =
    let reg, ops = ast
    Compiler.compileOperator (Order(reg, ops))
    
/// <summary>
/// Interface method to analyze the semantics of the generated AST
/// </summary>
/// <param name="ast">Generated AST for analysis</param>
/// <returns>Tuple of circuit memory and error tag</returns>
let public analyzeSemantics (ast:operator * operator): Memory * error =
    let reg, ops = ast
    try
        let quantumMap, classicMap = Interpreter.allocateBits reg
        let memory = Memory.empty.SetQuantumClassic quantumMap classicMap
        Interpreter.semanticAnalyzer ops memory
        memory, Success        
    with e -> Memory.empty, SemanticError e.Message

/// <summary>
/// Interface method to optimize the generated AST with eager evaluation and reduction rules
/// </summary>
/// <param name="ast">Generated AST for optimization</param>
/// <param name="memory">Initialized circuit memory</param>
/// <returns>Tuple of optimized AST, updated circuit memory and error tag</returns>
let public optimizeAST (ast:operator) (memory:Memory) : operator * Memory * error =
    try
        let optimized = Interpreter.optimizeOperator ast Map.empty Map.empty 0
        let _, memA, memB, optimAST = optimized
        let updMemory = (memory.SetArithmetic memA).SetBoolean memB
        optimAST, updMemory, Success
    with e -> ast, memory, EvaluationError e.Message
    

let rec public collectOperators (ast:operator) (acc:operator list):operator list =
    match ast with
    | Order(op1, op2) -> let opList = collectOperators op2 acc
                         op1::opList
    | _ ->  ast::acc

/// <summary>
/// Internal method to get number of choice from user input
/// </summary>
/// <returns>Tuple of success bool value and choice value</returns>
let private getMenuInput() = Int32.TryParse(Console.ReadLine())

/// <summary>
/// Internal method to get user input as string
/// </summary>
/// <returns>String of user input</returns>
let private getInputCode() = Console.ReadLine()

let private printInnerMenu() =
    printfn "Menu:"
    printfn "1. Semantic Analysis"
    printfn "2. Optimize AST"
    printfn "3. Collect Operators"
    printfn "4. Compiler Q#"
    printfn "5. Translator QuLang (Prettify)"
    printfn "6. Back to main menu"
    printf "Enter your choice:"

/// <summary>
/// Internal method to execute tools on initial parsed AST
/// </summary>
/// <param name="ast">AST</param>
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
    | true, 3 -> let _, ops = ast
                 let opList = collectOperators ops List.Empty
                 printfn $"Operation List:%A{opList}"
    | true, 4 -> let qSharp = compileAST ast
                 printfn $"%A{qSharp}"
                 execute(ast)
    | true, 5 -> let quLang = translateAST ast              
                 printfn $"%A{quLang}"
                 execute(ast)
    | true, 6 -> ()
    | _ -> execute(ast)
    

let private printMainMenu() =
    printfn "Menu: "
    printfn "1. Parse QuLang circuit"
    printfn "2. Parse boolean expression"
    printfn "3. Parse arithmetic expression"
    printfn "4. Quit"
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
    | true, 2 -> let expr = Console.ReadLine()
                 let ast, err = parseBool expr
                 printfn $"%A{ast} Status:%A{err}"
                 mainMenu()
    | true, 3 -> let expr = Console.ReadLine()
                 let ast, err = parseArith expr
                 printfn $"%A{ast} Status:%A{err}"
                 mainMenu()
    | true, 4 -> ()
    | _ -> mainMenu()

// [<EntryPoint>]
//     let main argv =
//         mainMenu()
//         0 // return an integer exit code