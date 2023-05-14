/// <summary>
/// Interface to library assisting the compilation of QuLang input
/// </summary>
module public QuLangProcessor.Handler
(** F#
 -*- coding: utf-8 -*-
Quantum Language Handler

Description: Facade of library assisting the compilation of QuLang input.
Acts as an API for different functionalities of the library.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 21/02/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.2
@__Status --> DEV
*)
open System
open FSharp.Text
open Microsoft.FSharp.Core
open QuLangProcessor.AST

/// <summary>
/// Interface method to parse QuLang code to circuit AST based on the grammar rules
/// </summary>
/// <param name="code">String in QuLang format awaiting parsing</param>
/// <returns>Tuple of AST for allocation and operation and error tag</returns>
let public parseQuLang (code:string) : Circuit Option * Error =
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
              printfn $"Parse error in program at : Line %i{line}, %i{column}, Unexpected token: %A{token}"
              // Return an empty AST and the syntax error
              (None, SyntaxError(token, line, column))

/// <summary>
/// Interface method to parse boolean expression from string format to AST
/// /// </summary>
/// <param name="expr">String awaiting parsing</param>
/// <returns>Tuple of boolean AST and error tag</returns>
let public parseBool (expr:string): BoolExpr Option * Error =
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
              (None, SyntaxError(token, line, column))
              
/// <summary>
/// Interface method to parse arithmetic expression from string format to AST
/// </summary>
/// <param name="expr">String awaiting parsing</param>
/// <returns>Tuple of arithmetic AST and error tag</returns>
let public parseArith (expr:string): ArithExpr option * Error =
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
              (None, SyntaxError(token, line, column))

/// <summary>
/// Interface method to back-compile generated circuit AST to QuLang code
/// </summary>
/// <param name="circuit">Generated AST for translation(AST.Circuit)</param>
/// <returns>QuLang string representation</returns>
let rec public backCompileCircuit (circuit:Circuit) : string = 
    let AllocQC(q, c), Flow(flow) = circuit
    $"Qalloc {BackCompiler.backCompileBit q};\nCalloc {BackCompiler.backCompileBit c};
        \n\n{BackCompiler.backCompileFlow flow}"
   
    
/// <summary>
/// Interface method to translate generated AST to Q# code
/// </summary>
/// <param name="circuit">Generated circuit AST for compilation (AST.Circuit)</param>
/// <returns>Q# string representation</returns>
let public translateCircuit (circuit:Circuit):string =
    let AllocQC(q,c), Flow(flow) = circuit
    let header = "namespace Quantum.App {\n\n
        \topen Microsoft.Quantum.Canon;\n
        \topen Microsoft.Quantum.Intrinsic;\n
        \topen Microsoft.Quantum.Math;\n
        
        \t@EntryPoint()\n
        \toperation SimulateCircuit() : Unit { \n
            \t\t// Placeholder"
    let str = Translator.translateAlloc q true + "\n" +
              Translator.translateAlloc c false + "\n\n" +
              Translator.translateFlow flow
    header + "\n" + str + "\n\t}\n}"
        
/// <summary>
/// Interface method to analyze the semantics of the generated circuit AST
/// </summary>
/// <param name="ast">Generated circuit AST for analysis</param>
/// <returns>Tuple of circuit memory and error tag</returns>
let public analyzeSemantics (ast:Circuit): Memory * Error =
    let reg, Flow ops = ast
    try
        let quantumMap, classicMap = Compiler.allocateBits reg
        let memory = Memory.empty.SetQuantumClassic quantumMap classicMap
        Compiler.semanticAnalyzer ops memory
        memory, Success        
    with e -> Memory.empty, SemanticError e.Message

/// <summary>
/// Interface method to optimize the generated AST with eager evaluation and reduction rules
/// </summary>
/// <param name="ast">Generated AST for optimization</param>
/// <param name="memory">Initialized circuit memory</param>
/// <returns>Tuple of optimized AST, updated circuit memory and error tag</returns>
let public optimizeAST (ast:Circuit) (memory:Memory):
    Circuit * Memory * Error =
    try
        let all, Flow ops = ast
        let optimized = Compiler.optimizeCircuit ops Map.empty Map.empty 0
        let _, memA, memB, optAST = optimized
        let updMemory = (memory.SetArithmetic memA).SetBoolean memB
        (all, Flow optAST), updMemory, Success
    with e -> ast, memory, EvaluationError e.Message

/// <summary>
/// Interface method to optimize logical expressions with eager evaluation and reduction rules
/// </summary>
/// <param name="b">Generated boolean expression for optimization</param>
/// <param name="memory">Initialized circuit memory</param>
/// <returns>Tuple of optimized boolean expression, updated circuit memory and error tag</returns>
let public optimizeLogic (b:BoolExpr) (memory:Memory) : BoolExpr * Memory * Error =
    try
        let optimized = Compiler.evalBool b memory.Boolean memory.Arithmetic
        optimized, memory, Success
    with e -> b, memory, EvaluationError e.Message

/// <summary>
/// Interface method to optimize arithmetic expressions with eager evaluation and reduction rules
/// </summary>
/// <param name="a">Generated arithmetic expression for optimization</param>
/// <param name="memory">Initialized circuit memory</param>
/// <returns>Tuple of optimized arithmetic expression, updated circuit memory and error tag</returns>
let public optimizeArithmetic (a:ArithExpr) (memory:Memory) : ArithExpr * Memory * Error =
    try
        let optimized = Compiler.evalArith a memory.Arithmetic
        optimized, memory, Success
    with e -> a, memory, EvaluationError e.Message

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
    printfn "3. Compiler Q#"
    printfn "4. Translator QuLang (Prettify)"
    printfn "5. Back to main menu"
    printf "Enter your choice:"

/// <summary>
/// Internal method to execute tools on initial parsed AST
/// </summary>
/// <param name="ast">AST</param>
let rec private execute (ast:Circuit) =
    printInnerMenu()
    match getMenuInput() with
    | true, 1 -> let memory, err = analyzeSemantics ast
                 printfn $"Memory:%A{memory}"
                 printfn $"Status:%A{err}"
                 execute(ast)
    | true, 2 -> let optimized, updMemory, err = optimizeAST ast Memory.empty
                 printfn $"AST:%A{ast}" 
                 printfn $"Memory:%A{updMemory}"
                 printfn $"Status:%A{err}"
                 execute(optimized)
    | true, 3 -> let qSharp = translateCircuit ast
                 printfn $"%A{qSharp}"
                 execute(ast)
    | true, 4 -> let quLang = backCompileCircuit ast              
                 printfn $"%A{quLang}"
                 execute(ast)
    | true, 5 -> ()
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
                 match ast with
                 | None -> printfn $"{err}"
                 | Some circ -> printfn $"%A{circ} Status:%A{err}"
                                execute(circ)
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

// Uncomment the following lines to run the program in the console
// [<EntryPoint>]
//     let main argv =
//         mainMenu()
//         0 // return an integer exit code