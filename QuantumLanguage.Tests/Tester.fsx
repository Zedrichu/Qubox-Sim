#r "/Users/adrianzvizdenco/F#Setup/FsLexYacc.Runtime.dll"

open FSharp.Text.Lexing
open System
#load "../QuantumLanguage/VisitorPattern.fs"
#load "../QuantumLanguage/AST.fs"
open QuantumLanguage.AST
#load "../QuantumLanguage/Parser.fs"
open QuantumLanguage.Parser
#load "../QuantumLanguage/Lexer.fs"
open QuantumLanguage.Lexer
#load "../QuantumLanguage/Interpreter.fs"
open QuantumLanguage.Interpreter
#load "../QuantumLanguage/Compiler.fs"
open QuantumLanguage.Compiler
#load "../QuantumLanguage/Translator.fs"
open QuantumLanguage.Translator
#load "../QuantumLanguage/Program.fs"
open QuantumLanguage.Handler


let printMenu() =
    printfn "Menu: "
    printfn "1. AST"
    printfn "2. Quit"
    printf "Enter your choice:"

let getInput() = Int32.TryParse(Console.ReadLine())

mainMenu()
