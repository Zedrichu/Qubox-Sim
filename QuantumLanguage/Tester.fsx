#r "/Users/adrianzvizdenco/F#Setup/FsLexYacc.Runtime.dll"

open FSharp.Text.Lexing
open System
open System.IO
#load "AST.fs"
open QuantumLanguage.AST
#load "Parser.fs"
open QuantumLanguage.Parser
#load "Lexer.fs"
open QuantumLanguage.Lexer
#load "Program.fs"
open QuantumLanguage.Handler


let printMenu() =
    printfn "Menu: "
    printfn "1. AST"
    printfn "2. Quit"
    printf "Enter your choice:"

let getInput() = Int32.TryParse(Console.ReadLine())

let rec menu () =
    printfn ""
    printMenu()
    match getInput() with
    | true, 1 -> let code = Console.ReadLine()
                 printfn "%A" (ParseQuLang code(*[0..(String.length code - 2)]*))
                 menu()
    | true, 2 -> ()
    | _ -> menu()
menu()
