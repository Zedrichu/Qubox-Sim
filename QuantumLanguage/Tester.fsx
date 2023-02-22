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
open QuantumLanguage.QuantumHandler

let code = Console.ReadLine()

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
    | true, 1 -> printfn "%A" (ParseQuLang code)
                 menu()
    | true, 2 -> ()
    | _ -> menu()
menu()
